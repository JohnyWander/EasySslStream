using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Connection.Full
{
    public class Server
    {

        List<SSLClient> ConnectedClients = new List<SSLClient>();

        public X509Certificate2 serverCert = null;
        private TcpListener listener = null;

        private CancellationTokenSource cts = new CancellationTokenSource();
        

        public Server(string ListenOnIp,int port,string ServerPFXCertificatePath,string CertPassword,bool VerifyClients)
        {

            serverCert = new X509Certificate2(ServerPFXCertificatePath,CertPassword, X509KeyStorageFlags.PersistKeySet);
            listener = new TcpListener(IPAddress.Parse(ListenOnIp), port);
            Thread listenerThread = new Thread(() =>
            {

                listener.Start();


                while (listener.Server.IsBound)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ConnectedClients.Add(new SSLClient(client, serverCert, VerifyClients));
                }
            });
            listenerThread.Start();
        }

        public Server(IPAddress ListenOnIp,int port,string ServerPFXCertificatePath,string CertPassword,bool VerifyClients)
        {
            this.serverCert = new X509Certificate2(ServerPFXCertificatePath,CertPassword,X509KeyStorageFlags.PersistKeySet);
            listener = new TcpListener(ListenOnIp, port);
            listener.Start();


            Thread listenerThrewad = new Thread(() =>
            {
                while (listener.Server.IsBound)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ConnectedClients.Add(new SSLClient(client, serverCert, VerifyClients));
                }
            });
              listenerThrewad.Start();
        }


      
    }


    public sealed class SSLClient
    {
        string ClientIP;
        int ClientPort;


        TcpClient client_ = null;
        SslStream sslstream_ = null;

        private bool ValidadeClientCert(object sender,X509Certificate certificate,X509Chain chain,SslPolicyErrors sslPolicyErrors)
        {
            if(sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            else
            { return false; }
        }

        enum OrderCodes
        {

            
            // transmission

        }


        public SSLClient(TcpClient client,X509Certificate2 serverCert,bool VerifyClients)
        {
            client_ = client;
            
            if(VerifyClients == false)
            {
                sslstream_ = new SslStream(client.GetStream(), false);
                sslstream_.AuthenticateAsServer(serverCert,clientCertificateRequired:false,true);
            }
            else
            {
                sslstream_ = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidadeClientCert));
                sslstream_.AuthenticateAsServer(serverCert, clientCertificateRequired: true, true);
            }

            try
            {
                

            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }



            bool cancelConnection = false;

            Task.Run(async () =>
            {

                while (cancelConnection == false)
                {
                    Console.WriteLine("Waiting for steer");
                    int steer = await ConnSteer();
                    Console.WriteLine(steer);
                    





                }

            }).GetAwaiter().GetResult();

        }

        private async Task<int> ConnSteer()
        {
            byte[] buffer = new byte[64];
            int steer = -999999;

            int bytes_count = -1;

        
                bytes_count = await sslstream_.ReadAsync(buffer, 0, buffer.Length);
                steer = BitConverter.ToInt32(buffer,0);
                await sslstream_.FlushAsync();

            
           

            return steer;


        }




    }












}
