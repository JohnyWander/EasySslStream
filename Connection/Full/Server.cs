using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EasySslStream.Connection.Full
{
    public class Server
    {
        List<SSLClient> ConnectedClients = new List<SSLClient>();
        public X509Certificate2 serverCert = null;
        private TcpListener listener = null;

        private CancellationTokenSource cts = new CancellationTokenSource();


        public Encoding TextReceiveEncoding = Encoding.UTF8;
        public Encoding FileNameEncoding = Encoding.UTF8;

        public CertificateCheckSettings CertificateCheckSettings = new CertificateCheckSettings();

        public Action<string> HandleReceivedText = (string text) =>
        {
            Console.WriteLine(text);
        };




        public string ReceivedFilesLocation = AppDomain.CurrentDomain.BaseDirectory;
       

    
        public void StartServer(string ListenOnIp, int port, string ServerPFXCertificatePath, string CertPassword, bool VerifyClients)
        {
            serverCert = new X509Certificate2(ServerPFXCertificatePath, CertPassword, X509KeyStorageFlags.PersistKeySet);
            listener = new TcpListener(IPAddress.Parse(ListenOnIp), port);
            Thread listenerThread = new Thread(() =>
            {

                listener.Start();


                while (listener.Server.IsBound)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ConnectedClients.Add(new SSLClient(client, serverCert, VerifyClients,this));
                }
            });
            listenerThread.Start();
        }
        public void StartServer(IPAddress ListenOnIp, int port, string ServerPFXCertificatePath, string CertPassword, bool VerifyClients)
        {
            this.serverCert = new X509Certificate2(ServerPFXCertificatePath, CertPassword, X509KeyStorageFlags.PersistKeySet);
            listener = new TcpListener(ListenOnIp, port);
            listener.Start();


            Thread listenerThrewad = new Thread(() =>
            {
                while (listener.Server.IsBound)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ConnectedClients.Add(new SSLClient(client, serverCert, VerifyClients,this));
                }
            });
            listenerThrewad.Start();
        }



       


      
    }


    public sealed class SSLClient : ConnectionMethods
    {
        private Channel<Action> ServerSendingQueue = Channel.CreateUnbounded<Action>();

        string ClientIP;
        int ClientPort;

        private Server srv;


        private string Terminatorstring = "<ENDOFTEXT>";

        TcpClient client_ = null;
        SslStream sslstream_ = null;

        private bool ValidadeClientCert(object sender,X509Certificate certificate,X509Chain chain,SslPolicyErrors sslPolicyErrors)
        {
            if(sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            else if(sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch && srv.CertificateCheckSettings.VerifyCertificateName == false)
            {
                return true;
            }else if(sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors && srv.CertificateCheckSettings.VerifyCertificateChain == false)
            {
                return true;
            }
            else
            {
                return false;
            }
           
        }

        enum OrderCodes
        {

            
            // transmission

        }


        public SSLClient(TcpClient client,X509Certificate2 serverCert,bool VerifyClients,Server srvinstance=null)
        {
            client_ = client;
            srv = srvinstance;
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

        


            Thread ServerSender = new Thread(() =>
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        await ServerSendingQueue.Reader.WaitToReadAsync();
                        Action w = await ServerSendingQueue.Reader.ReadAsync();
                        await Task.Delay(100);
                        w.Invoke();
                        w = null;

                    }
                }).ConfigureAwait(false).GetAwaiter().GetResult();


            });
            ServerSender.Start();


            bool cancelConnection = false;

            Task.Run(async () =>
            {
              //  try
              //  {
                    while (cancelConnection == false)
                    {
                        Console.WriteLine("Waiting for steer");
                        int steer = await ConnSteer();

                        Console.WriteLine(steer);


                        switch (steer)
                        {
                            case 1:
                                srv.HandleReceivedText.Invoke(await GetText(srv.TextReceiveEncoding));
                                break;
                            case 2:
                                await GetFile(srv);
                                break;

                        }



                    }
              //  }
              //  catch (Exception e)
              //  {
               //     Console.WriteLine(e.Message);
              //  }
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

        private async Task<string> GetText(Encoding enc)
        {
            
            byte[] buffer = new byte[64];

            int bytes_count = -1;
            StringBuilder Message = new StringBuilder();




            Decoder decoder = enc.GetDecoder();
            do
            {
                bytes_count = await sslstream_.ReadAsync(buffer, 0, buffer.Length);

                char[] messagechars = new char[decoder.GetCharCount(buffer, 0, bytes_count, true)];
                decoder.GetChars(buffer, 0, bytes_count, messagechars, 0);
                Message.Append(messagechars);

               // Console.WriteLine(Message.ToString());

                if (Message.ToString().IndexOf(Terminatorstring) != -1) { break; } 



            } while (bytes_count != 0);

            string toreturn = Message.ToString();
            toreturn = toreturn.Substring( 0, toreturn.IndexOf(Terminatorstring));
            return toreturn;

        }

        private Task GetFile(Server srv)
        {
            //file name
            int filenamebytes = -1;
            byte[] filenamebuffer = new byte[128];
            filenamebytes = sslstream_.Read(filenamebuffer);
            

            string filename = srv.FileNameEncoding.GetString(filenamebuffer).Trim(Convert.ToChar(0x00));
           // Console.WriteLine("filename is: " + filename);

            int lengthbytes = -1;
            byte[] file_length_buffer = new byte[512];
            lengthbytes =  sslstream_.Read(file_length_buffer);
            int FileLength = BitConverter.ToInt32(file_length_buffer);

           // Console.WriteLine("File lenhth is: " + FileLength);
            /////////
            string[] FilesInDirectory = Directory.GetFiles(srv.ReceivedFilesLocation);

            bool correct = false;
            int number_of_occurence = 1;
            while (correct == false)
            {
                if (FilesInDirectory.Contains(filename))
                {
                    filename = filename + number_of_occurence;
                    number_of_occurence++;
                   // Console.WriteLine("contains");
                }
                else
                {
                    correct = true;
                   // Console.WriteLine("CORRECT");
                }
            }


            int bytesReceived = 0;
            byte[] ReceiveBuffer = new byte[DynamicConfiguration.TransportBufferSize];

            if(srv.ReceivedFilesLocation != "")
            {
                Directory.SetCurrentDirectory(srv.ReceivedFilesLocation);
            }
            //File.WriteAllText("debugfilename.txt", filename);
            FileStream fs = new FileStream(filename.Trim(), FileMode.Create) ;

            var watch = new Stopwatch();

            watch.Start();
            while ((sslstream_.Read(ReceiveBuffer, 0, ReceiveBuffer.Length) != 0))
            {


                fs.Write(ReceiveBuffer);
              //  Console.WriteLine(fs.Length + "/" + FileLength);
                if (fs.Length >= FileLength)
                {
              //      Console.WriteLine("END");
                    break;
                }
            }
            watch.Stop();
            Console.WriteLine("Time elapsed: "+watch.ElapsedMilliseconds + " ms");

            long ReceivedFileLength = fs.Length;
            
            if(ReceivedFileLength > FileLength)
            {
                fs.SetLength(FileLength);
            }


            fs.Dispose();

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            return Task.CompletedTask;
        }

    



    }












}
