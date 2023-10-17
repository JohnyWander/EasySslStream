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
using EasySslStream.ConnectionV2.Client;

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
            
            config.connectionOptions.enabledProtocols = SslProtocols.Tls12;

            srv = new Server(IpToListenOn, PortToListenOn, config);
            srv.StartServer("servercert.pfx", "");
            Console.WriteLine("Successfully started server");
            Console.WriteLine($"Listening on {IpToListenOn}:{PortToListenOn}");
            

            srv.ClientConnected += () =>
            {
                ConnectedClient client = srv.ConnectedClientsById.Last().Value;
                Console.WriteLine($"Client connected {client.clientEndPoint.ToString()}");
                Console.WriteLine($"Cipher is {client.sslStream.CipherAlgorithm.ToString()}");
                Console.WriteLine($"Key exchange algorithm is {client.sslStream.KeyExchangeAlgorithm.ToString()}");
                Console.WriteLine($"Ssl protocol is {client.sslStream.SslProtocol.ToString()}");

                client.ConnectionHandler.FileSavePath = client.ConnectionID.ToString();
                client.ConnectionHandler.DirectorySavePath = client.ConnectionID.ToString();

                ConfigureEvents(client);

            };

            while (!srv.RunningServerListener.IsCompleted)
            {
                MainMenu();
            }


            srv.RunningServerListener.Wait();
        }

        void MainMenu()
        {
            Console.WriteLine("1. Select client");
            Console.WriteLine("2. Clear Console");
            Console.WriteLine("2. Shut down server");

            ConsoleKeyInfo c = Console.ReadKey(true);
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
                
                MainMenu();
            }
            else
            {
                Console.WriteLine("Select client to send to");
                foreach (KeyValuePair<int,ConnectedClient> IdClientPair in srv.ConnectedClientsById)
                {
                    Console.WriteLine($"{IdClientPair.Key} {IdClientPair.Value.clientEndPoint}");
                }

                ConnectedClient selected = selection();
                ActionMenu(selected);
                
            }
        }

        void ActionMenu(ConnectedClient client)
        {
            Console.WriteLine("1. Send text");
            Console.WriteLine("2. Send Bytes");
            Console.WriteLine("3. Send File");
            Console.WriteLine("4. Send Directory");
            Console.WriteLine("5. Pick another");

            int selection = GetUserInputToInt();

            switch (selection)
            {
                case 1:
                    bool async1 = AsyncVersion();
                    Console.WriteLine("Enter string to send");
                    if (async1)
                    {
                        client.ConnectionHandler.SendTextAsync(Console.ReadLine(), Encoding.UTF8).Wait();
                    }
                    else
                    {
                        client.ConnectionHandler.SendTextAsync(Console.ReadLine(),Encoding.UTF8);
                    }                                        
                    break;
                case 2:
                    SendBytes(client);
                   break;
                case 3:
                    bool async3 = AsyncVersion();
                    Console.WriteLine("Enter path to file");
                    if (async3)
                    {
                        client.ConnectionHandler.SendFileAsync(Console.ReadLine()).Wait();
                    }
                    else
                    {
                        client.ConnectionHandler.SendFile(Console.ReadLine());
                    }
                    break; 

                case 4:
                    bool async4 = AsyncVersion();
                    Console.WriteLine("Enter path to directory");
                    if (async4)
                    {
                        client.ConnectionHandler.SendDirectoryAsync(Console.ReadLine()).Wait();
                    }
                    else
                    {
                        client.ConnectionHandler.SendDirectory(Console.ReadLine());
                    }
                    break;
                case 5:
                    Console.Clear();
                    SelectionMenu();
                    break;
            }
            Console.WriteLine("DONE!");
        }

        bool AsyncVersion()
        {
            string[] validAsyncSwitches = new string[] { "Y", "y", "N", "n" };
            string asyncSwitch = "";
            while (!validAsyncSwitches.Contains(asyncSwitch))
            {
                Console.WriteLine("Test async version? (y/n)");
                asyncSwitch = Console.ReadLine();
            }

            if(asyncSwitch == "Y" || asyncSwitch == "y")
            {
                return true;
            }
            else if(asyncSwitch == "N" || asyncSwitch == "n")
            {
                return false;
            }
            else
            {
                throw new ArgumentException();
            }

        }

       

        void SendBytes(ConnectedClient client)
        {
            
            Console.WriteLine("Enter bytes to send like - FF,11,22,AB,CC ...");
            bool asyncAction = AsyncVersion();
            string[] providedByteStrings = Console.ReadLine().Trim(',').Split(",");
            byte[] byteArray = new byte[providedByteStrings.Length];

            for (int i = 0; i < providedByteStrings.Length; i++)
            {
                if (byte.TryParse(providedByteStrings[i], System.Globalization.NumberStyles.HexNumber, null, out byte parsedByte))
                {
                    byteArray[i] = parsedByte;
                }
                else
                {
                    Console.WriteLine($"Invalid byte at index {i}: {providedByteStrings[i]}");
                    ActionMenu(client);
                }
            }

            if (asyncAction)
            {
                client.ConnectionHandler.SendBytesAsync(byteArray).Wait();
            }
            else
            {
                client.ConnectionHandler.SendBytes(byteArray);
            }
            
            
            

        }

        ConnectedClient selection()
        {
            Console.WriteLine("select client");
            try
            {
                ConnectedClient selected = srv.ConnectedClientsById[GetUserInputToInt()];                
                return selected;
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Invalid selection, try again");
                return selection();
            }
                      
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


        int GetUserInputToInt()
        {
            try
            {
                ConsoleKeyInfo k = Console.ReadKey();
                int selection = Convert.ToInt32(new string(k.KeyChar, 1));
                return selection;
            }
            catch
            {
                Console.WriteLine("It's not valid number, please pick valid number from list above");
                return GetUserInputToInt();
            }
            
        }

        public HandleServer()
        {
            
        }
    }
}
