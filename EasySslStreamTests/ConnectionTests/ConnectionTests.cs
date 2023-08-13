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

namespace EasySslStreamTests.ConnectionTests
{
    internal class ConnectionTests
    {

        string Workspace = "ConnectionWorkspace";
        string ServerWorkspace = "Server";
        string ClientWorkspace = "Client";

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

            if (!File.Exists(Workspace + "\\" + ServerWorkspace + "\\" + "Server.crt"))
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
            }

            if (!File.Exists(Workspace + "\\" + ClientWorkspace + "\\" + "Client.crt"))
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
            }


        }


        [SetUp]
        public void Setup()
        {
            client = new Client();
            server = new Server();

        }


        [Test]
        public void Test()
        {

        }


    }
}
