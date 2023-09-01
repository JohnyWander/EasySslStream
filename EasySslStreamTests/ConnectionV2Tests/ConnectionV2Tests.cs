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
using System.Runtime.CompilerServices;

namespace EasySslStreamTests.ConnectionV2Tests
{
    [TestFixture]
    internal class ConnectionV2Tests : PreparationBase
    {
        Encoding utf8 = Encoding.UTF8;
        Encoding utf32 = Encoding.UTF8;


        string Workspace = "ConnectionWorkspace";
        string ServerWorkspace = "Server";
        string ClientWorkspace = "Client";

        TaskCompletionSource<object> TestEnder;
        TaskCompletionSource<object> ClientWaiter;

        Server srv;
        Client client;

        Random rnd = new Random();

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
            conf.BufferSize = 8192;

            conf.authOptions.VerifyDomainName = false;
            conf.authOptions.VerifyCertificateChain = false;
            conf.authOptions.VerifyClientCertificates = false;
            srv = new Server(new IPEndPoint(IPAddress.Any, 5000),conf);
            srv.StartServer($"{Workspace}\\{ServerWorkspace}\\Server.pfx", "123");

            ClientConfiguration clientconf = new ClientConfiguration();
            clientconf.serverVerifiesClient = false;            
            clientconf.verifyCertificateChain = false;
            clientconf.verifyDomainName = false;           
            client = new Client("127.0.0.1", 5000, clientconf);
        }


        [TearDown]
        public void teardown()
        {
            srv.StopServer();
            client.Disconnect();

            srv = null;
            client = null;
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
        //[TestCase(SslProtocols.Tls13)] // appears to not be supported
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

        [Test]
        [TestCase(32)]
        [TestCase(64)]
        [TestCase(1024)]
        [TestCase(2048)]
        [TestCase(4096)]
        public async Task ByteTransferClientToServerTest(int BytesToSend)
        {
            
            Task locker = Task.Run(() => Locker());
            Task clientWaiter = Task.Run(() => ClientAwaiter());

            Task Connection = client.Connect();
            srv.ClientConnected += () =>
            {
                this.ClientWaiter.SetResult(true);           
            };
            await clientWaiter;
            await Connection;
            byte[] testBytes = new byte[BytesToSend];
            byte[] ReceivedBytes = null;
            rnd.NextBytes(testBytes);

            
            var handler = srv.ConnectedClientsById[0].ConnectionHandler;
            handler.HandleReceivedBytes += (byte[] received) =>
            {
                ReceivedBytes = received;
                TestEnder.SetResult(true);

              
            };

            
            client.ConnectionHandler.SendBytes(testBytes);           
            await locker;

            Assert.That(Enumerable.SequenceEqual(testBytes, ReceivedBytes));
        }

        [Test]
        [TestCase(32)]
        [TestCase(64)]
        [TestCase(1024)]
        [TestCase(2048)]
        [TestCase(4096)]
        public async Task ByteTransferServerToClient(int BytesToSend)
        {
            Task locker = Task.Run(() => Locker());
            Task clientWaiter = Task.Run(() => ClientAwaiter());

            Task Connection = client.Connect();
            srv.ClientConnected += () =>
            {
                this.ClientWaiter.SetResult(true);
            };
            await clientWaiter;

            byte[] testBytes = new byte[BytesToSend];
            byte[] ReceivedBytes = null;
            rnd.NextBytes(testBytes);

            await Connection;

            var handler = srv.ConnectedClientsById[0].ConnectionHandler;

            
            client.ConnectionHandler.HandleReceivedBytes += (byte[] received) =>
            {
                ReceivedBytes = received;
                TestEnder.SetResult(true);
            };

            
            handler.SendBytes(testBytes);
            await locker;
            Assert.That(Enumerable.SequenceEqual(testBytes, ReceivedBytes));
        }

        [Test]
        [TestCase(32)]
        [TestCase(64)]
        [TestCase(128)]
        [TestCase(2048)]
        [TestCase(4096)]
        public async Task TransferStringClientToServerTest(int StringLength)
        {
            
            Task locker = Task.Run(() => Locker());
            Task clientWaiter = Task.Run(() => ClientAwaiter());

            Task Connection = client.Connect();
            srv.ClientConnected += () =>
            {
                this.ClientWaiter.SetResult(true);
            };
            await clientWaiter;
            await Connection;

            string received = null;
            srv.ConnectedClientsById[0].ConnectionHandler.HandleReceivedText += (string _received) =>
            {
                received = _received;
                TestEnder.SetResult(true);               
            };

            byte[] randomBytes = new byte[StringLength];
            rnd.NextBytes(randomBytes);
            string randomstring = Encoding.UTF8.GetString(randomBytes);

            client.ConnectionHandler.SendText(randomstring, Encoding.UTF8);
            
            await locker;
            Debug.WriteLine(received);
            Assert.That(received == randomstring);

        }

        [Test]
        [TestCase(32)]
        [TestCase(64)]
        [TestCase(128)]
        [TestCase(2048)]
        [TestCase(4096)]
        public async Task TransferStringServerToClientTest(int StringLength)
        {
            Task locker = Task.Run(() => Locker());
            Task clientWaiter = Task.Run(() => ClientAwaiter());
            Task Connection = client.Connect();
            srv.ClientConnected += () =>
            {
                this.ClientWaiter.SetResult(true);
            };
            await clientWaiter;
            await Connection;

            string received = null;

            client.ConnectionHandler.HandleReceivedText += (string _received) =>
            {
                received = _received;
                TestEnder.SetResult(true);
            };

            byte[] randomBytes = new byte[StringLength];
            rnd.NextBytes(randomBytes);
            string randomstring = Encoding.UTF8.GetString(randomBytes);

            srv.ConnectedClientsById[0].ConnectionHandler.SendText(randomstring, Encoding.UTF8);

            await locker;
            Debug.WriteLine(received);
            Assert.That(received == randomstring);
        }

        [Test]
        public async Task TransferFileClientToServer()
        {
            Task locker = Task.Run(() => Locker());
            Task clientWaiter = Task.Run(() => ClientAwaiter());
            Task Connection = client.Connect();
            srv.ClientConnected += () =>
            {
                this.ClientWaiter.SetResult(true);
            };
            await clientWaiter;
            await Connection;

            string[] files = Directory.EnumerateFiles($"{Workspace}\\{ClientWorkspace}\\TestTransferDir","*",SearchOption.AllDirectories).ToArray();
            string PickedFile = files[rnd.Next(0,files.Length)];

            srv.ConnectedClientsById[0].ConnectionHandler.FileSavePath = $"{Workspace}\\{ServerWorkspace}";
            srv.ConnectedClientsById[0].ConnectionHandler.HandleReceivedFile += (string path) =>
            {
                TestEnder.SetResult(true);
            };

            client.ConnectionHandler.SendFile(PickedFile);

            await locker;
            

        }


    }
}
