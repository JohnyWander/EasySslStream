using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using EasySslStream.ConnectionV2.Server;
using EasySslStream.ConnectionV2.Server.Configuration;
using EasySslStream.ConnectionV2.Client;
using EasySslStream.ConnectionV2.Client.Configuration;
using System.Diagnostics;
using NuGet.Frameworks;
using System.Net.Security;
using System.Security.Authentication;

namespace EasySslStreamTests.ConnectionV2Tests
{
    [TestFixture]
    internal class ConnectionV2Tests : PreparationBase
    {
        string Workspace = "ConnectionWorkspace";
        string ServerWorkspace = "Server";
        string ClientWorkspace = "Client";

        TaskCompletionSource<object> TestEnder;
        TaskCompletionSource<object> ClientWaiter;

        Server srv;
        Client client;


        async Task Locker(int customDelay = 20000)
        {
            Task.Run(async () =>
            {
                await Task.Delay(customDelay);
                if (!TestEnder.Task.IsCompleted)
                {
                    TestEnder.SetException(new Exception("Operation time out"));
                }
            });
            await this.TestEnder.Task;
        }

        async Task ClientAwaiter()
        {
            Task.Run(async () =>
            {
                await Task.Delay(10000);
                if (!ClientWaiter.Task.IsCompleted)
                {
                    ClientWaiter.SetException(new Exception("Waiting for client timed out"));
                    Debug.WriteLine("client didn't connect");
                }
            });
            await ClientWaiter.Task;
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            base.CreateCertificates(Workspace, ServerWorkspace, ClientWorkspace); 
            base.CreateFolder(Workspace, ServerWorkspace, ClientWorkspace);
        }



        [SetUp]
        public void Setup()
        {
            TestEnder = new TaskCompletionSource<object>();
            ClientWaiter = new TaskCompletionSource<object>();
            

            ServerConfiguration conf = new ServerConfiguration();
            conf.connectionOptions.bufferSize = 8192;

            conf.authOptions.VerifyDomainName = false;
            conf.authOptions.VerifyCertificateChain = false;
            conf.authOptions.VerifyClientCertificates = false;
            srv = new Server(new IPEndPoint(IPAddress.Any, 5000),conf);
           
            ClientConfiguration clientconf = new ClientConfiguration();
            clientconf.serverVerifiesClient = false;            
            clientconf.verifyCertificateChain = false;
            clientconf.verifyDomainName = false;           
            client = new Client("127.0.0.1", 5000, clientconf);
        }

        [Test]
        public async Task ConnectTest()
        {
            srv.StartServer($"{Workspace}\\{ServerWorkspace}\\Server.pfx", "123");
            Task locker = Task.Run(() => Locker());
            Task clientWaiter = Task.Run(() => ClientAwaiter());

            client.Connect();
            srv.ClientConnected += () =>
            {
                Debug.WriteLine("CONNECTED");
                this.ClientWaiter.SetResult(true);
                srv.StopServer();

            };
            await clientWaiter;
            Assert.Multiple(() =>
            {
                Assert.That(srv.ConnectedClientsByEndpoint.Count != 0);
                Assert.That(srv.ConnectedClientsById.Count != 0);               
            });            
            await srv.RunningServerListener;
            // Assert.Throws(async () => await srv.RunningServerListener);
            Assert.That(srv.RunningServerListener.IsCompleted);                 
        }

        [Test]
        [TestCase(SslProtocols.Tls)]
        [TestCase(SslProtocols.Tls11)]
        [TestCase(SslProtocols.Tls12)]
        [TestCase(SslProtocols.Tls13)]
        public async Task ProtocolNegotiationTest(SslProtocols protocol)
        {
            Task locker = Task.Run(() => Locker());
            Task clientWaiter = Task.Run(() => ClientAwaiter());

            ServerConfiguration sconf = new ServerConfiguration();
            sconf.authOptions.VerifyClientCertificates = false;
            sconf.enabledSSLProtocols = protocol;
            

            ClientConfiguration cconf = new ClientConfiguration();
            cconf.enabledSSLProtocols = protocol;
            cconf.verifyCertificateChain = false;
            cconf.verifyDomainName = false;
            
            Server server = new Server(new IPEndPoint(IPAddress.Any, 5000),sconf);
            Client cl = new Client("127.0.0.1",5000,cconf);


            server.ClientConnected += () =>
            {
                this.ClientWaiter.SetResult(true);
            };

            server.StartServer($"{Workspace}\\{ServerWorkspace}\\Server.pfx", "123");
            
            cl.Connect();
            await clientWaiter;

            SslStream ServerStream = server.ConnectedClientsById[0].sslStream;
            SslStream ClientStream = cl.sslStream;

            Assert.Multiple(() =>
            {
                Assert.That(ServerStream.SslProtocol == protocol);
                Assert.That(ClientStream.SslProtocol == protocol);
                Assert.That(ServerStream.SslProtocol == ClientStream.SslProtocol);
            });


            Debug.WriteLine(ServerStream.SslProtocol.ToString());
            server.StopServer();
            ServerStream.Dispose();
            ClientStream.Dispose();
            
        }



    }
}
