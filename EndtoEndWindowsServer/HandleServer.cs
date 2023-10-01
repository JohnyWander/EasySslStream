using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Authentication;
using EasySslStream;
using EasySslStream.ConnectionV2.Server;
using EasySslStream.ConnectionV2.Server.Configuration;

namespace EndtoEndTestServer
{
    internal class HandleServer
    {
        public string IpToListenOn;
        public int PortToListenOn = 2000;
        public bool UseConsole;

        Server srv;
        public void ExposeSendingConsole()
        {
            UseConsole = true;
        }

        public void Launch()
        {
            ServerConfiguration config = new ServerConfiguration();
            config.BufferSize = 8096;
            config.connectionOptions.enabledProtocols = SslProtocols.Tls12;
            
            srv = new Server(IpToListenOn, PortToListenOn,config);
            srv.StartServer("servercert.pfx", "123");
            Console.WriteLine("Successfully started server");


            srv.ClientConnected += () =>
            {
                ConnectedClient client = srv.ConnectedClientsById.Last().Value;
                Console.WriteLine($"Client connected {client.clientEndPoint.ToString()}");
                Console.WriteLine($"Cipher is {client.sslStream.CipherAlgorithm.ToString()}");

                client.ConnectionHandler.FileSavePath = client.ConnectionID.ToString();
                client.ConnectionHandler.DirectorySavePath = client.ConnectionID.ToString();

                client.ConnectionHandler.HandleReceivedBytes += (byte[] received) =>
                {
                    Console.WriteLine($"{client.clientEndPoint.ToString()}");
                };

            };

            

                
            

            srv.RunningServerListener.Wait();
        }

        public HandleServer()
        {
            
        }
    }
}
