using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using GPSD.Library.Models;
using Newtonsoft.Json;

namespace GPSD.Library
{
    public class GpsdService
    {
        #region Private Properties

        private TcpClient _client;

        private readonly string _serverAddress;
        private readonly int _serverPort;

        private bool _proxyEnabled;
        private string _proxyAddress;
        private int _proxyPort;
        private bool _proxyAuthenticationEnabled;
        private string _proxyUsername;
        private string _proxyPassword;

        private GpsLocation _previousGpsLocation;

        #endregion

        #region Properties

        public bool IsRunning { get; set; }

        public GpsdVersion GpsdVersion { get; set; }
        public int ReadFrequenty { get; set; } = 1000;

        public GpsdOptions GpsOptions { get; set; }

        #endregion

        #region Events

        public delegate void LocationEventHandler(object source, GpsLocation e);
        public event LocationEventHandler OnLocationChanged;

        #endregion

        #region Constructors

        public GpsdService(string serverAddress, int serverPort)
        {
            _serverAddress = serverAddress;
            _serverPort = serverPort;
            GpsOptions = GpsdConstants.DefaultGpsdOptions;
            IsRunning = true;
        }

        public GpsdService(string serverAddress, int serverPort, GpsdOptions gpsOptions = null) : this(serverAddress, serverPort)
        {
            GpsOptions = gpsOptions ?? GpsdConstants.DefaultGpsdOptions;
        }

        #endregion

        #region Service Functionality

        public void StartService()
        {
            using (_client = GetTcpClient())
            {
                if (!_client.Connected) return;

                var networkStream = _client.GetStream();
                var streamReader = new StreamReader(networkStream);

                var gpsdDataParser = new GpsdDataParser();

                while (IsRunning && _client.Connected)
                {
                    var gpsData = streamReader.ReadLine();
                    var message = gpsdDataParser.GetGpsData(gpsData);

                    var version = message as GpsdVersion;
                    if (version != null)
                    {
                        GpsdVersion = version;
                        Console.WriteLine(GpsdVersion.ToString());

                        ExecuteCommand(networkStream, GpsOptions.GetCommand());
                    }

                    var gpsLocation = message as GpsLocation;
                    if (gpsLocation != null &&
                        (_previousGpsLocation == null ||
                         gpsLocation.Time.Subtract(new TimeSpan(0, 0, 0, 0, ReadFrequenty)) > _previousGpsLocation.Time))
                    {
                        OnLocationChanged?.Invoke(this, gpsLocation);
                        _previousGpsLocation = gpsLocation;
                        Thread.Sleep(ReadFrequenty);
                    }
                }
            }
        }

        public void StopService()
        {
            IsRunning = false;

            ExecuteCommand(_client.GetStream(), GpsdConstants.DisableCommand);
            _client.Close();
        }

        #endregion

        #region Events

        

        #endregion

        #region Helper Functions

        private static void ExecuteCommand(Stream stream, string command)
        {
            var streamWriter = new StreamWriter(stream);
            streamWriter.WriteLine(command);
            streamWriter.Flush();
        }

        #endregion

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

        private TcpClient GetTcpClient()
        {
            return _proxyEnabled ? ConnectViaHttpProxy() : new TcpClient(_serverAddress, _serverPort);
        }

        private TcpClient ConnectViaHttpProxy()
        {
            var proxy = WebRequest.GetSystemWebProxy();

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
