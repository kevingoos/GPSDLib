using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using NmeaParser;

namespace TestNmeaParser
{
    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class GpsService
    {
        private const string ProxyAddress = "proxy";
        private const string ServerAddress = "178.50.179.166";
        private const int Port = 80;

        // ManualResetEvent instances signal completion.
        private static readonly ManualResetEvent ReceiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.
        private static string _response = string.Empty;


        

        public void StartService()
        {
            using (var client = ConnectViaHttpProxy(ServerAddress, Port, ProxyAddress, Port))
            {
                if (client.Connected)
                {
                    var device = new StreamDevice(client.GetStream());
                    device.MessageReceived += device_NmeaMessageReceived;
                    device.OpenAsync();
                }


                //Receive(client.Client);
                //ReceiveDone.WaitOne();
            }
        }

        private void device_NmeaMessageReceived(object sender, NmeaMessageReceivedEventArgs e)
        {
            
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

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                var state = new StateObject { workSocket = client };

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                var client = state.workSocket;
                
                var bytesRead = client.EndReceive(ar);
                _response = Encoding.UTF8.GetString(state.buffer, 0, state.buffer.Length);
                ReceiveDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine(_response);
        }


    }


}
