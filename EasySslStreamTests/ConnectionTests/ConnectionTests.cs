using EasySslStream.CertGenerationClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EasySslStream.Certgen.GenerationClasses.GenerationConfigs;
using EasySslStream.CertGenerationClasses.GenerationConfigs;
using EasySslStream;
using EasySslStream.Connection.Client;
using System.Net;
using NUnit.Framework;
using System.Diagnostics;

namespace EasySslStreamTests.ConnectionTests
{
    internal class ConnectionTests
    {

        string Workspace = "ConnectionWorkspace";
        string ServerWorkspace = "Server";
        string ClientWorkspace = "Client";

        TaskCompletionSource<object> TestEnder;

        X509Certificate2 TestClientCertificate;
        X509Certificate2 TestServerCertificate;

        Client client;
        Server server;


        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            OpensslCertGeneration certgen = new OpensslCertGeneration();


            CaCertgenConfig caconf = new CaCertgenConfig();
            caconf.CountryCode = "US";
            caconf.KeyLength = Config.KeyLengths.RSA_4096;
            caconf.HashAlgorithm = Config.HashAlgorithms.sha384;
            caconf.CommonName = "easysslstreamCA";

            certgen.GenerateCA(caconf, Workspace);

            if (!File.Exists(Workspace + "\\" + ServerWorkspace + "\\" + "Server.pfx"))
            {

                CSRConfiguration serverCSRConf = new CSRConfiguration();
                serverCSRConf.KeyLength = Config.KeyLengths.RSA_4096;
                serverCSRConf.HashAlgorithm = Config.HashAlgorithms.sha384;
                serverCSRConf.CountryCode = "JP";
                serverCSRConf.CommonName = "Server.com";
                serverCSRConf.alt_names.Add("*.Server.com");
            
                certgen.GenerateCSR(serverCSRConf,Workspace+"\\"+ServerWorkspace,"Server.csr","Server.key");

                SignCSRConfig signCSRConfig = new SignCSRConfig();
                signCSRConfig.SetDefaultConfig(SignCSRConfig.DefaultConfigs.Server);

                certgen.SignCSR(signCSRConfig, $"Server.csr",
                    $"..\\CA.crt",
                    $"..\\CA.key",
                    "Server.crt",
                    $"{Workspace}\\{ServerWorkspace}"
                    );

                certgen.ConvertX509ToPfx("Server.crt", "Server.key", "Server.pfx", "123", $"{Workspace}\\{ServerWorkspace}");

                
            }

            if (!File.Exists(Workspace + "\\" + ClientWorkspace + "\\" + "Client.pfx"))
            {
                CSRConfiguration clientCSRConf = new CSRConfiguration();
                clientCSRConf.KeyLength = Config.KeyLengths.RSA_4096;
                clientCSRConf.HashAlgorithm = Config.HashAlgorithms.sha384;
                clientCSRConf.CountryCode = "JP";
                clientCSRConf.CommonName = "client.com";
                clientCSRConf.alt_names.Add("*.client.com");

                certgen.GenerateCSR(clientCSRConf, Workspace + "\\" + ClientWorkspace, "Client.csr", "Client.key");

                SignCSRConfig signCSRconfig = new SignCSRConfig();
                signCSRconfig.SetDefaultConfig(SignCSRConfig.DefaultConfigs.Enduser);

                certgen.SignCSR(signCSRconfig, "Client.csr",
                   $"..\\CA.crt",
                   $"..\\CA.key",
                   "Client.crt",
                   $"{Workspace}\\{ClientWorkspace}"
                 );

                certgen.ConvertX509ToPfx("Client.crt", "Client.key", "Client.pfx", "123", $"{Workspace}\\{ClientWorkspace}");
            }

            if (!Directory.Exists($"{Workspace}\\{ServerWorkspace}\\TestTransferDir"))
            {
                Directory.CreateDirectory("TestTransferDir");
                PreparationMethods.CreateRandomTestDirectory($"{Workspace}\\{ServerWorkspace}\\TestTransferDir",512000,128000000,5,10);
            }

            if (!Directory.Exists($"{Workspace}\\{ClientWorkspace}\\TestTransferDir"))
            {
                Directory.CreateDirectory("TestTransferDir");
                PreparationMethods.CreateRandomTestDirectory($"{Workspace}\\{ClientWorkspace}\\TestTransferDir", 512000, 128000000, 5, 10);
            }


        }


        [SetUp]
        public void Setup()
        {
            client = new Client(8192);
            client.VerifyCertificateChain = false;
            client.VerifyCertificateName = false;
            server = new Server(8192);

            this.TestEnder = new TaskCompletionSource<object>();
            
            server.StartServer(IPAddress.Any, 5000, $"{Workspace}\\{ServerWorkspace}\\Server.pfx", "123", false);
          
            
        }

        
        



        [Test]
        public async Task SendStringMessageTest()
        {
            Task locker = Task.Run(async () =>
            {
                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    if (!TestEnder.Task.IsCompleted)
                    {
                        TestEnder.SetException(new Exception("Operation time out"));
                    }
                });
                await this.TestEnder.Task;
            });

            string Received="";
            server.HandleReceivedText = (string r) =>
            {
                Received = r;
                this.TestEnder.SetResult(null);
            };

            client.Connect("127.0.0.1", 5000);
            client.WriteText(Encoding.UTF8.GetBytes("Test Message"));
                  
            await locker;

            Assert.That(Received == "Test Message", $"Got {Received}");
        }
    }
}
