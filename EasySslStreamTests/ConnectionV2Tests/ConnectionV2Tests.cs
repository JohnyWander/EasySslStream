using EasySslStream.ConnectionV2.Client;
using EasySslStream.ConnectionV2.Client.Configuration;
using EasySslStream.ConnectionV2.Server;
using EasySslStream.ConnectionV2.Server.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;

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
            srv = new Server(new IPEndPoint(IPAddress.Any, 5000), conf);
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
            if(srv != null)
            {
                try
                {
                    srv.StopServer();
                }
                catch(NullReferenceException)
                {
                }
                //srv = null;
            }

            if(client != null)
            {
                try
                {
                    client.Disconnect();
                }
                catch (NullReferenceException) 
                {
                }
            }                       
        }

        [Test]
        public async Task ConnectTest()
        {           
            Task locker = Task.Run(() => Locker());
            Task clientWaiter = Task.Run(() => ClientAwaiter());

            Task connection = client.Connect();
            srv.ClientConnected += () =>
            {
                Debug.WriteLine("CONNECTED");
                this.ClientWaiter.SetResult(true);
                srv.StopServer();

            };

            await connection;
            await clientWaiter;
            
            Assert.Multiple(() =>
            {
                Assert.That(srv.ConnectedClientsByEndpoint.Count != 0);
                Assert.That(srv.ConnectedClientsById.Count != 0);
            });
            await srv.RunningServerListener;           
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

            Server sr = new Server(new IPEndPoint(IPAddress.Any, 5001), sconf);
            Client cl = new Client("127.0.0.1", 5001, cconf);
            sr.StartServer($"{Workspace}\\{ServerWorkspace}\\Server.pfx", "123");
            Task connection =  cl.Connect();
            sr.ClientConnected += () =>
            {
                ClientWaiter.SetResult(true);
               
            };


            await connection;
            //await Task.Delay(1000);

            
            await clientWaiter;

            SslStream ServerStream = sr.ConnectedClientsById[0].sslStream;
            SslStream ClientStream = cl.sslStream;

            Assert.Multiple(() =>
            {
                Assert.That(ServerStream.SslProtocol == protocol);
                Assert.That(ClientStream.SslProtocol == protocol);
                Assert.That(ServerStream.SslProtocol == ClientStream.SslProtocol);
            });


            Debug.WriteLine(ServerStream.SslProtocol.ToString());
            sr.StopServer();
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


            string[] files = Directory.GetFiles($"{Workspace}\\{ClientWorkspace}\\TestTransferDir", "*", SearchOption.AllDirectories).ToArray();
            string PickedFile = files[rnd.Next(0, files.Length)];

            srv.ConnectedClientsById[0].ConnectionHandler.FileSavePath = $"{Workspace}\\{ServerWorkspace}";
            srv.ConnectedClientsById[0].ConnectionHandler.HandleReceivedFile += (string path) =>
            {
                TestEnder.SetResult(true);
            };

            client.ConnectionHandler.SendFile(PickedFile);

            await locker;

            SHA256 sha = SHA256.Create();

            FileStream source = new FileStream(PickedFile, FileMode.Open, FileAccess.Read);
            FileStream Destination = new FileStream($"{Workspace}\\{ServerWorkspace}\\{Path.GetFileName(PickedFile)}", FileMode.Open, FileAccess.Read);
            byte[] sourceHash = sha.ComputeHash(source);
            byte[] destinationHash = sha.ComputeHash(Destination);

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists($"{Workspace}\\{ServerWorkspace}\\{Path.GetFileName(PickedFile)}"), "Destination file does not exist");
                Assert.That(Enumerable.SequenceEqual(sourceHash, destinationHash), "Hashes do not match");

            });


        }

        [Test]
        public async Task DirectoryTransferClientToServerTest()
        {
            Task locker = Task.Run(() => Locker(20000000));
            Task clientWaiter = Task.Run(() => ClientAwaiter());
            Task Connection = client.Connect();
            srv.ClientConnected += () =>
            {
                this.ClientWaiter.SetResult(true);
            };
            await Connection;
            await clientWaiter;
            

            string directory = $"{Workspace}\\{ClientWorkspace}\\TestTransferDir";

            srv.ConnectedClientsById[0].ConnectionHandler.DirectorySavePath = $"{Workspace}\\{ServerWorkspace}\\Received";
            srv.ConnectedClientsById[0].ConnectionHandler.HandleReceivedDirectory += (string path) =>
            {
                TestEnder.SetResult(true);
                Debug.WriteLine(path);
            };

            client.ConnectionHandler.SendDirectory(directory);

            await locker;

            Debug.WriteLine("LockerEnded");
            string[] sourceFiles = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
            string[] destinationFiles = Directory.GetFiles($"{Workspace}\\{ServerWorkspace}\\Received\\TestTransferDir", "*.*", SearchOption.AllDirectories);
            bool directoriesMatch = sourceFiles.Count() == destinationFiles.Count();

            if (directoriesMatch)
            {
                Assert.Multiple(() =>
                {
                    SHA256 sha = SHA256.Create();
                    var entries = sourceFiles.Zip(destinationFiles, (s, d) => new { source = s, dest = d });
                    foreach (var entry in entries)
                    {
                        using (FileStream sourceStream = new FileStream(entry.source, FileMode.Open))
                        using (FileStream destStream = new FileStream(entry.dest, FileMode.Open))
                        {
                            byte[] sourceHash = sha.ComputeHash(sourceStream);
                            byte[] destHash = sha.ComputeHash(destStream);
                            Assert.That(Enumerable.SequenceEqual(sourceHash, destHash), $"Source file and destination file do not match {entry.source}\n{entry.dest}");
                        }
                    }
                });
            }
            else
            {
                Assert.Fail($"Directories do not match. Source file count - {sourceFiles.Length}, dest count - {destinationFiles.Length}");
            }



        }



    }
}
