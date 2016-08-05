using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestNmeaParser
{
    public class GpsService2
    {
        private const string ProxyAddress = "proxy";
        private const string ServerAddress = "178.50.25.14";
        private const int Port = 80;

        public void StartService()
        {
            var serverAddress = IPAddress.Parse(ServerAddress);
            var listener = new TcpListener(serverAddress, Port);
            listener.Start();

            var tcpClient = listener.AcceptTcpClient();

            var bytes = new byte[256];
            var stream = tcpClient.GetStream();
            stream.Read(bytes, 0, bytes.Length);


        }


        static TcpClient ConnectViaHttpProxy(string targetHost, int targetPort, string httpProxyHost, int httpProxyPort)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = Uri.UriSchemeHttp,
                Host = httpProxyHost,
                Port = httpProxyPort
            };

            var proxyUri = uriBuilder.Uri;

            var request = WebRequest.Create(
                "http://" + targetHost + ":" + targetPort);

            var webProxy = new WebProxy(proxyUri);

            request.Proxy = webProxy;
            request.Method = "CONNECT";

            webProxy.Credentials = new NetworkCredential("EXJ508", "Xlssx532");
            //webProxy.UseDefaultCredentials = true;

            var response = request.GetResponse();

            var responseStream = response.GetResponseStream();
            Debug.Assert(responseStream != null);

            const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var rsType = responseStream.GetType();
            var connectionProperty = rsType.GetProperty("Connection", Flags);

            var connection = connectionProperty.GetValue(responseStream, null);
            var connectionType = connection.GetType();
            var networkStreamProperty = connectionType.GetProperty("NetworkStream", Flags);

            var networkStream = networkStreamProperty.GetValue(connection, null);
            var nsType = networkStream.GetType();
            var socketProperty = nsType.GetProperty("Socket", Flags);
            var socket = (Socket)socketProperty.GetValue(networkStream, null);

            return new TcpClient { Client = socket };
        }
    }
}
