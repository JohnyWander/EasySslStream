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
        string Terminator= "<ENDOFTEXT>";
        private Channel<Action> work = Channel.CreateUnbounded<Action>();

        public TcpClient client;
        public  SslStream stream;

        public bool VerifyCertificateName = true;
        public bool VerifyCertificateChain = true;
        private enum SteerCodes
        {
            SendText = 1,
            
        }

        public bool ValidateServerCertificate(object sender,X509Certificate certificate,X509Chain chain,SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch && VerifyCertificateName == false)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors && VerifyCertificateChain == false)
            {
                return true;
            }
            else
            {
                return false;
            }




        }
        public Client()
        {
           
        }
       
        public void Connect(string ip, int port)
        {
            X509Certificate x = null;
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

        public void Connect(string ip, int port,string clientCertLocation,string certPassword)
        {
            X509Certificate x = null;
            Thread cThread = new Thread(() =>
            {
                client = new TcpClient(ip, port);

                stream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate));
                
                X509Certificate2 clientCert = new X509Certificate2(clientCertLocation, certPassword, X509KeyStorageFlags.PersistKeySet);



                X509Certificate2Collection certs = new X509Certificate2Collection(clientCert);
                
                stream.AuthenticateAsClient(ip,certs,false);
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

        public void WriteFile(string filePath)
        {

        }


        public void WriteText(byte[] message)
        {

            List<byte> messagebytes = new List<byte>();
            messagebytes.AddRange(message);
            messagebytes.AddRange(Encoding.UTF8.GetBytes(Terminator));
           

            Action WR= () =>
            {

                stream.Write(BitConverter.GetBytes((int)SteerCodes.SendText));
                stream.Write(messagebytes.ToArray());
                
            };
            work.Writer.TryWrite( WR ); 
        }





    }
}
