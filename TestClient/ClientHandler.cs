using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySslStream;
using EasySslStream.ConnectionV2.Client;
using EasySslStream.ConnectionV2.Client.Configuration;

namespace TestClient
{
    internal class ClientHandler
    {
        public string FileSavePath;
        public string DirectorySavePath;
        public string ConnectTo
        {
            private get { return ip + port; }
            set
            {
                string[] split = value.Split(':');
                ip = split[0];
                port = int.Parse(split[1]);
            }
        }
        string ip;
        int port;


        bool ServerVerifiesCerts = false;
        public string CertificatePath;
        public string CertificatePassword;

        public bool VerifyChain = false;
        public bool VerifyCN = false;

        public Client client;

        public void StartClient()
        {
            ClientConfiguration conf = new ClientConfiguration();
            conf.verifyCertificateChain = VerifyChain;
            conf.verifyDomainName = VerifyCN;

            if (ServerVerifiesCerts)
            {
                conf.pathToClientPfxCertificate = CertificatePath;
                conf.certificatePassword = CertificatePassword;
            }


            client = new Client(ip,port,conf);
            Console.WriteLine($"Connecting to {ip}:{port}");
            client.Connect().Wait();

            Console.WriteLine("Sucessfully connected to server");
            Console.WriteLine($"Cipher is {client.sslStream.CipherAlgorithm.ToString()}");
            Console.WriteLine($"Key exchange algorithm is {client.sslStream.KeyExchangeAlgorithm.ToString()}");
            Console.WriteLine($"Ssl protocol is {client.sslStream.SslProtocol.ToString()}");

            ConfigureCLient(client);

            client.RunningClient.Wait();
        }


        public void ConfigureCLient(Client client)
        {
            client.ConnectionHandler.DirectorySavePath = "DirectoriesFromServer";
            client.ConnectionHandler.FileSavePath = "FilesFromServer";

            client.ConnectionHandler.HandleReceivedText += (string received) =>
            {
                Console.WriteLine($"Server said [text] {received}");
            };

            client.ConnectionHandler.HandleReceivedBytes += (byte[] received) =>
            {
                Console.WriteLine("Server said [bytes]");
                received.ToList().ForEach(x => Console.WriteLine(x.ToString()));
            };

            client.ConnectionHandler.HandleReceivedFile += (string filepath) =>
            {

            };
        }

    
    }
}
