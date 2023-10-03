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
using EasySslStream.ConnectionV2.Communication;

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

            srv = new Server(IpToListenOn, PortToListenOn, config);
            srv.StartServer("servercert.pfx", "123");
            Console.WriteLine("Successfully started server");


            srv.ClientConnected += () =>
            {
                ConnectedClient client = srv.ConnectedClientsById.Last().Value;
                Console.WriteLine($"Client connected {client.clientEndPoint.ToString()}");
                Console.WriteLine($"Cipher is {client.sslStream.CipherAlgorithm.ToString()}");

                client.ConnectionHandler.FileSavePath = client.ConnectionID.ToString();
                client.ConnectionHandler.DirectorySavePath = client.ConnectionID.ToString();

                ConfigureEvents(client);

            };

            while (!srv.RunningServerListener.IsCompleted)
            {


            }


            srv.RunningServerListener.Wait();
        }

        void MainMenu()
        {
            Console.WriteLine("1. Select client");
            Console.WriteLine("2. Clear Console");
            Console.WriteLine("2. Shut down server");

            ConsoleKeyInfo c = Console.ReadKey();
            ConsoleKey key = c.Key;

            switch (key)
            {
                case ConsoleKey.D1 or ConsoleKey.NumPad1:

                    SelectionMenu();
                    break;

                case ConsoleKey.D2 or ConsoleKey.NumPad2:

                    Console.Clear();
                    break;

                case ConsoleKey.D3 or ConsoleKey.NumPad3:
                    this.srv.StopServer();
                    break;

            }
        }

        void SelectionMenu()
        {
            if(srv.ConnectedClientsById.Count == 0)
            {
                Console.WriteLine("There are not connected clients");
            }
            else
            {
                foreach (KeyValuePair<int,ConnectedClient> IdClientPair in srv.ConnectedClientsById)
                {
                    Console.WriteLine($"{IdClientPair}")
                }
            }
        }

        void ActionMenu()
        {

        }


        void ConfigureEvents(ConnectedClient client)
        {
            client.ConnectionHandler.HandleReceivedBytes += (byte[] received) =>
            {
                Console.WriteLine($"{client.clientEndPoint.ToString()} said [Bytes] ");
                received.ToList().ForEach(x => Console.Write(x.ToString()));
                Console.Write("\n");
            };

            client.ConnectionHandler.HandleReceivedText += (string text) =>
            {
                Console.WriteLine($"{client.clientEndPoint.ToString()} said [text] {text}");
            };

            client.ConnectionHandler.HandleReceivedFile += (string path) =>
            {
                Console.WriteLine($"{client.clientEndPoint.ToString()} sended [FILE] saved - {path}");
            };

            client.ConnectionHandler.HandleReceivedDirectory += (string path) =>
            {
                Console.WriteLine($"{client.clientEndPoint.ToString()} sended [Directory] - {path}");
            };
        }

        public HandleServer()
        {
            
        }
    }
}
