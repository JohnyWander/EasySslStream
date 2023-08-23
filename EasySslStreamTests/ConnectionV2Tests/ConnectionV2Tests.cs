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


            srv.StartServer($"{Workspace}\\{ServerWorkspace}\\Server.pfx", "123");
            client = new Client("127.0.0.1", 5000, clientconf);
        }

        [Test]
        public async Task ConnectTest()
        {
            Task locker = Task.Run(() => Locker());
            Task clientWaiter = Task.Run(() => ClientAwaiter());

            client.Connect();

            srv.ClientConnected += () =>
            {
                Debug.WriteLine("CONNECTED");
                this.ClientWaiter.SetResult(true);
               // srv.StopServer();
            };

            await clientWaiter;


            Assert.Multiple(() =>
            {
                Assert.That(()=>)


            });

           // await srv.RunningServerListener;
           // Assert.Throws(async () => await srv.RunningServerListener);
            
        }



    }
}
