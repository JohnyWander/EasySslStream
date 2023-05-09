using EasySslStream.Connection.Full;
using EasySslStream.TESTING.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.TESTING
{
    public class ServerSide
    {
        public Server srv;

        /// <summary>
        /// Action delegate which launches when test has something to communnicate, by default it shows messages with Console.WriteLine()
        /// <br></br>You may do anything with the output data, write output to file for example.
        /// </summary>
        public Action<string> CommunnicateResults = (string Message) =>
        {
            Console.WriteLine(Message);
        };

        public ServerSide()
        {
            
            srv = new Server();
           
            

        }


        public void LaunchTestServer(ServerTestConfig config)
        {
            try
            {
                if (config.ListenOnIpString == null)
                {
                    srv.CertificateCheckSettings.VerifyCertificateChain = config.VerifyCertificateChain;
                    srv.CertificateCheckSettings.VerifyCertificateName = config.VerifyCertificateName;

                    srv.StartServer(config.ListenOnIPAddress,
                        config.ListenPort,
                        config.PathToDefaultServerCert,
                        config.CertificatePassword,
                        config.VerifyClients);
                }
                else
                {
                    srv.CertificateCheckSettings.VerifyCertificateChain = config.VerifyCertificateChain;
                    srv.CertificateCheckSettings.VerifyCertificateName = config.VerifyCertificateName;

                    srv.StartServer(config.ListenOnIpString,
                        config.ListenPort,
                        config.PathToDefaultServerCert,
                        config.CertificatePassword,
                        config.VerifyClients);

                    CommunnicateResults?.Invoke("Test server started successfully");

                }
            }catch(Exception ex)
            {
                string ErrorMessage = $"Test server Failed with exception: {ex.GetType().Name}\t and it's message: {ex.Message}";
                CommunnicateResults?.Invoke(ErrorMessage);

            }
        }


        /// <summary>
        /// Starts testing of all connection functions, and their stats
        /// </summary>
        /// <param name="filesAndDirectories"></param>
        /// <param name=""></param>
        public void StartTest(ServerTestObjects filesAndDirectories,ConnectionCommons.Unit speedUnit,int ExpectedClients)
        {
            // waits for client to connect
            while(srv.ConnectedClients.Count != ExpectedClients)
            {
                CommunnicateResults?.Invoke("Waiting for all expected clients to connect...");
                Thread.Sleep(1000);
                
            }

            // configuring connected clients
            foreach (SSLClient client in srv.ConnectedClients)
            {
                IFileReceiveEventAndStats freas = client.FileReceiveEventAndStats;
                freas.AutoStartFileReceiveSpeedCheck = true;


               

               
            
            
            }
           







        }



    }
}
