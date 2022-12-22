using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EasySslStream.Connection.Full
{
    public class Client
    {
        private Channel<Action> work = Channel.CreateUnbounded<Action>();

        public TcpClient client;
        public  SslStream stream;
        public static bool ValidateServerCertificate(object sender,X509Certificate certificate,X509Chain chain,SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            else
            {
                Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
                return true; // debug purposes
            }
            

        
           
        }
        public Client(string ip,int port)
        {
            Thread cThread = new Thread(() =>
            {
                client = new TcpClient(ip, port);

                stream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate));
                try
                {
                    stream.WriteTimeout = 10000;
                    stream.AuthenticateAsClient(ip);
                    stream.ReadTimeout = 10000;

                    Task.Run(async () =>
                    {
                        while (true)
                        {

                            Console.WriteLine("WAITING FOR JOB");
                            await work.Reader.WaitToReadAsync();
                            Action w = await work.Reader.ReadAsync();
                            await Task.Delay(100);
                            w.Invoke();

                        }


                    }).GetAwaiter().GetResult();


                    stream.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            cThread.Start();
        }

        public void write(byte[] message)
        {

            Action WR= () =>
            {

                stream.Write(message);
               

            };

            work.Writer.TryWrite( WR );

            
        }





    }
}
