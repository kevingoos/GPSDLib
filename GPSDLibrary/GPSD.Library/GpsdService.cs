using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace GPSD.Library
{     
    public class GpsdService
    {
        private readonly string _serverAddress;
        private readonly int _serverPort;

        private bool _proxyEnabled;
        private string _proxyAddress;
        private int _proxyPort;

        private bool _proxyAuthenticationEnabled;
        private string _proxyUsername;
        private string _proxyPassword;

        public bool IsRunning { get; set; }

        public GpsdService(string serverAddress, int serverPort)
        {
            _serverAddress = serverAddress;
            _serverPort = serverPort;
            IsRunning = true;
        }

        public void StartService()
        {
            using (var client = GetTcpClient())
            {
                if (!client.Connected) return;
                
                var networkStream = client.GetStream();
                var result = new byte[256];
                networkStream.Read(result, 0, result.Length);

                var responseData = Encoding.ASCII.GetString(result, 0, result.Length);
                var gpsData = JsonConvert.DeserializeObject<GpsdData>(responseData);
                Console.WriteLine(gpsData.ToString());

                var byteData = Encoding.ASCII.GetBytes("?WATCH={\"enable\":true,\"json\":true}");
                networkStream.Write(byteData, 0, byteData.Length);
                
                while (IsRunning && client.Connected)
                {
                    networkStream.Read(result, 0, result.Length);
                    var response = Encoding.ASCII.GetString(result, 0, result.Length);
                    Console.WriteLine(response);
                    Thread.Sleep(10);
                }
                client.Close();
            }
        }

        private TcpClient GetTcpClient()
        {
            return _proxyEnabled ? ConnectViaHttpProxy() : new TcpClient(_serverAddress, _serverPort);
        }

        #region Proxies

        public void SetProxy(string proxyAddress, int proxyPort)
        {
            _proxyEnabled = true;
            _proxyAddress = proxyAddress;
            _proxyPort = proxyPort;
        }

        public void SetProxyAuthentication(string username, string password)
        {
            _proxyAuthenticationEnabled = true;
            _proxyUsername = username;
            _proxyPassword = password;
        }

        public void DisableProxy()
        {
            _proxyEnabled = false;
        }

        private TcpClient ConnectViaHttpProxy()
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = Uri.UriSchemeHttp,
                Host = _proxyAddress,
                Port = _proxyPort
            };

            var proxyUri = uriBuilder.Uri;
            var request = WebRequest.Create("http://" + _serverAddress + ":" + _serverPort);
            var webProxy = new WebProxy(proxyUri);

            request.Proxy = webProxy;
            request.Method = "CONNECT";

            if (_proxyAuthenticationEnabled)
            {
                webProxy.Credentials = new NetworkCredential(_proxyUsername, _proxyPassword);
            }
            else
            {
                webProxy.UseDefaultCredentials = true;
            }

            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            Debug.Assert(responseStream != null);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var rsType = responseStream.GetType();
            var connectionProperty = rsType.GetProperty("Connection", flags);

            var connection = connectionProperty.GetValue(responseStream, null);
            var connectionType = connection.GetType();
            var networkStreamProperty = connectionType.GetProperty("NetworkStream", flags);

            var networkStream = networkStreamProperty.GetValue(connection, null);
            var nsType = networkStream.GetType();
            var socketProperty = nsType.GetProperty("Socket", flags);
            var socket = (Socket)socketProperty.GetValue(networkStream, null);

            return new TcpClient { Client = socket };
        }

        #endregion
    }
}
