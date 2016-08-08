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
    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 4096;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
     
    public class GpsdService
    {
        private readonly string _serverAddress;
        private readonly int _serverPort;

        private bool proxyEnabled = false;
        private string _proxyAddress;
        private int _proxyPort;

        private bool proxyAuthenticationEnabled = false;
        private string _proxyUsername;
        private string _proxyPassword;



        public GpsdService(string serverAddress, int serverPort)
        {
            _serverAddress = serverAddress;
            _serverPort = serverPort;
        }

        public void SetProxy(string proxyAddress, int proxyPort)
        {
            _proxyAddress = proxyAddress;
            _proxyPort = proxyPort;
        }

        public void SetProxyAuthentication(string username, string password)
        {
            _proxyUsername = username;
            _proxyPassword = password;
        }
        
        public void StartService()
        {
            using (var client = GetTcpClient())
            {
                while (client.Connected)
                {
                    var result = new byte[256];
                    client.Client.Receive(result);
                    
                    var response = Encoding.ASCII.GetString(result, 0, result.Length);
                    var resultClass = JsonConvert.DeserializeObject<GpsdData>(response);
                    Console.WriteLine(resultClass.ToString());
                    
                    var byteData = Encoding.ASCII.GetBytes("?WATCH={\"enable\":true,\"json\":true}");
                    client.Client.Send(byteData);

                    Thread.Sleep(10);
                }
                
                client.Close();
            }
        }

        private TcpClient GetTcpClient()
        {
            return proxyEnabled ? ConnectViaHttpProxy() : new TcpClient(_serverAddress, _serverPort);
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

            if (proxyAuthenticationEnabled)
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
    }
}
