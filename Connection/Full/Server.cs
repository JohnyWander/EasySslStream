﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// <summary>
    /// Server class that can handle multiple clients
    /// </summary>
    public class Server
    {
        /// <summary>
        /// List that contains connected clients 
        /// </summary>
        public List<SSLClient> ConnectedClients = new List<SSLClient>();

        /// <summary>
        /// Thread safe dictionary that contains connected clients referenced by int
        /// </summary>
        public ConcurrentDictionary<int,SSLClient> ConnectedClientsByNumber = new ConcurrentDictionary<int, SSLClient>();


        /// <summary>
        /// Thread safe dictionary that contains connected clients referenced by string endpoint ( 127.0.0.1:5000 etc)
        /// </summary>
        public ConcurrentDictionary<IPEndPoint,SSLClient> ConnectedClientsByEndPoint = new ConcurrentDictionary<IPEndPoint, SSLClient>();

     
        private X509Certificate2 serverCert = null;


        private TcpListener listener = null;

        private CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// Encoding for text messages
        /// </summary>
        public Encoding TextReceiveEncoding = Encoding.UTF8;

        /// <summary>
        /// Encoding for filenames
        /// </summary>
        public Encoding FileNameEncoding = Encoding.UTF8;

        /// <summary>
        /// Specifies how certificate verification should behave
        /// </summary>
        public CertificateCheckSettings CertificateCheckSettings = new CertificateCheckSettings();

        /// <summary>
        /// Action Delegate for handling text data received from client, by default it prints message by Console.WriteLine()
        /// </summary>
        public Action<string> HandleReceivedText = (string text) =>
        {
            Console.WriteLine(text);            
        };

        /// <summary>
        /// Action Delegate for handling bytes received from client, by default it prints int representation of them in console
        /// </summary>
        public Action<byte[]> HandleReceivedBytes = (byte[] bytes) =>
        {
            foreach(byte b in bytes) { Console.Write(Convert.ToInt32(b)+" "); }
            //return bytes
        };

        // Shutting down the server
      ////////////////////////////////////////////////////////////////////  
        private TaskCompletionSource<object> GentleStopLock = new TaskCompletionSource<object>();
        internal void WorkLock()
        {
                bool NoJobs = true;
               foreach(SSLClient client in ConnectedClients)
                {             
                    if (client.Busy)
                    {
                        NoJobs = false;
                    }
                }

               if(NoJobs == true)
                {
                    GentleStopLock.SetResult(null);
                }
              
            
          
        }
       
        /// <summary>
        /// Waits for currently running transfers to end, for all connections, then shuts down the server.
        /// </summary>
        public async void GentleStopServer(int interval = 100)
        {
            if (ConnectedClients.Count != 0)
            {
                Console.WriteLine("Waiting for all jobs to terminate");
                bool loopcancel = true;
                Task.Run(() =>
                {
                    while (loopcancel)
                    {
                        WorkLock();
                        Task.Delay(interval).Wait();
                    }
                });

                

                await GentleStopLock.Task;

                loopcancel = false;

                Parallel.ForEach(ConnectedClients, SSLClient =>
                {
                    SSLClient.Stop();
                });
                listener.Stop();


            }
            else
            {
                Parallel.ForEach(ConnectedClients, SSLClient =>
                {
                    SSLClient.Stop();
                });
                listener.Stop();
            }

        }




        /// <summary>
        /// Disposes all connected clients and stops server from listening
        /// </summary>
        public void StopServer()
        {
            Parallel.ForEach(ConnectedClients, SSLClient =>
            {
                SSLClient.Stop(); 
            });
       
            this.listener.Stop();
        }

    
        /// <summary>
        /// Sends text Message to client
        /// </summary>
        /// <param name="clientEndpoint">Client endpoint</param>
        /// <param name="Message">byte array representation of the message</param>
        public void WriteTextToClient(IPEndPoint clientEndpoint, byte[] Message)
        {
            ConnectedClientsByEndPoint[clientEndpoint].WriteText(Message);
        }

        /// <summary>
        /// Sends text Message to client
        /// </summary>
        /// <param name="ConnectionID"></param>
        /// <param name="Message"></param>
        public void WriteTextToClient(int ConnectionID, byte[] Message)
        {
            ConnectedClients[ConnectionID].WriteText(Message);
        }



        /// <summary>
        /// Sends file to client
        /// </summary>
        /// <param name="ConnectionID">Id of connection</param>
        /// <param name="Path">path to the file to send</param>
        public void WriteFileToClient(int ConnectionID,string Path)
        {
            ConnectedClients[ConnectionID].SendFile(Path);
        }

        /// <summary>
        /// Sends file to client
        /// </summary>
        /// <param name="clientEndpoint">client endpoint</param>
        /// <param name="Path">path to the file to send</param>
        public void WriteFileToClient(IPEndPoint clientEndpoint,string Path)
        {
            ConnectedClientsByEndPoint[clientEndpoint].SendFile(Path);
        }

        /// <summary>
        /// Location for the received file from clients
        /// </summary>
        public string ReceivedFilesLocation = AppDomain.CurrentDomain.BaseDirectory;


        /// <summary>
        /// Starts server
        /// </summary>
        /// <param name="ListenOnIp">Listening ip</param>
        /// <param name="port">Listening port</param>
        /// <param name="ServerPFXCertificatePath">Path to the Certificate with private key in pfx format</param>
        /// <param name="CertPassword">Password to the certificate use empty string if there's no password</param>
        /// <param name="VerifyClients">Set true if server is meant to check for client certificate, otherwise set false</param>
        public void StartServer(string ListenOnIp, int port, string ServerPFXCertificatePath, string CertPassword, bool VerifyClients)
        {
            
            serverCert = new X509Certificate2(ServerPFXCertificatePath, CertPassword, X509KeyStorageFlags.PersistKeySet);
            listener = new TcpListener(IPAddress.Parse(ListenOnIp), port);
            Thread listenerThread = new Thread(() =>
            {
                listener.Start();
                int connected = 0;
                while (listener.Server.IsBound)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    SSLClient connection = new SSLClient(client, serverCert, VerifyClients, this);
                    Console.WriteLine(client.Client.RemoteEndPoint?.ToString());
                }
                connected++;
            });
            listenerThread.Start();          
        }

        /// <summary>
        /// Starts server
        /// </summary>
        /// <param name="ListenOnIp">Listening ip</param>
        /// <param name="port">Listening port</param>
        /// <param name="ServerPFXCertificatePath">Path to the Certificate with private key in pfx format</param>
        /// <param name="CertPassword">Password to the certificate use empty string if there's no password</param>
        /// <param name="VerifyClients">Set true if server is meant to check for client certificate, otherwise set false</param>
        public void StartServer(IPAddress ListenOnIp, int port, string ServerPFXCertificatePath, string CertPassword, bool VerifyClients)
        {
           
            this.serverCert = new X509Certificate2(ServerPFXCertificatePath, CertPassword, X509KeyStorageFlags.PersistKeySet);
            listener = new TcpListener(ListenOnIp, port);
            listener.Start();
            Thread listenerThrewad = new Thread(() =>
            {
                int connected = 0;
                while (listener.Server.IsBound)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    SSLClient connection = new SSLClient(client, serverCert, VerifyClients, this);
                }
                connected++;
            });
            listenerThrewad.Start();
        }







    }

    /// <summary>
    /// Represents connected client 
    /// </summary>
    public sealed class SSLClient
    {
        internal bool Busy = false;

        private Channel<Action> ServerSendingQueue = Channel.CreateUnbounded<Action>();

        string ClientIP;
        int ClientPort;

        private Server srv;

        /// <summary>
        /// Encoding for filenames, UTF8 by default
        /// </summary>
        public Encoding FilenameEncoding = Encoding.UTF8;

        private string Terminatorstring = "<ENDOFTEXT>";

        TcpClient client_ = null;
        SslStream sslstream_ = null;

//////////////////////////////////////////
        /// Connection menagement
        /// 
        internal void Stop()
        {
            sslstream_.Dispose();
            client_.Dispose();
        }
        /// <summary>
        /// Closes connection with a client immediately. It will throw ServerException that should be handled by user.
        /// </summary>
        public void DisconnectClient()
        {
            sslstream_.Dispose();
            client_.Dispose();
        }


        TaskCompletionSource<object> GentleDisconnectTask = new TaskCompletionSource<object>();
        /// <summary>
        /// Gently Closes connection with client. Waits for ongoing transfer/s to finish.
        /// Optionally informs client about closing connection
        /// </summary>
        /// <param name="InformClient">False by default. If set to true client will be informed that it has been disconnected </param>
        public async void GentleDisconnectClient(bool InformClient = false)
        {
            await GentleDisconnectTask.Task;

            ServerSendingQueue = null;

            if (InformClient)
            {
                sslstream_.Write(BitConverter.GetBytes((int)SteerCodes.SendDisconnect));
              //  Console.WriteLine("CANCEL?");
            }


            sslstream_.Dispose();
            client_.Dispose();

        }


        private bool privateBusy
        {
            set
            {
                if (value == false)
                {
                    GentleDisconnectTask.SetResult(null);
                }
                else
                {
                    GentleDisconnectTask = new TaskCompletionSource<object>();
                }
            }
        }
////////////////////////////////////


        private bool ValidadeClientCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch && srv.CertificateCheckSettings.VerifyCertificateName == false)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors && srv.CertificateCheckSettings.VerifyCertificateChain == false)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private enum SteerCodes
        {
            SendText = 1,  
            SendFile = 2,
            SendRawBytes =3,
            SendDirectory = 4,

            SendDisconnect =99
        }

        /// <summary>
        /// Creates client instance
        /// </summary>
        /// <param name="client">Tcpclient instance</param>
        /// <param name="serverCert">Server certificate</param>
        /// <param name="VerifyClients"></param>
        /// <param name="srvinstance"></param>
        public SSLClient(TcpClient client, X509Certificate2 serverCert, bool VerifyClients, Server srvinstance = null)
        {
            Busy = false;
            client_ = client;
            srv = srvinstance;

            srv.ConnectedClients.Add(this);
            srv.ConnectedClientsByNumber.TryAdd(srv.ConnectedClients.Count, this);

           
            

            srv.ConnectedClientsByEndPoint.TryAdd((IPEndPoint)client.Client.RemoteEndPoint, this);
            // Console.WriteLine("?: "+srv.ConnectedClients.Count);

            if (VerifyClients == false)
            {
                sslstream_ = new SslStream(client.GetStream(), false);
                sslstream_.AuthenticateAsServer(serverCert, clientCertificateRequired: false, true);
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
                    try
                    {
                        while (true)
                        {
                            await ServerSendingQueue.Reader.WaitToReadAsync();
                            Action w = await ServerSendingQueue.Reader.ReadAsync();
                            await Task.Delay(100);
                            Busy = true; privateBusy = true;
                            w.Invoke();
                            Busy = false; privateBusy = false;
                            w = null;

                        }
                    }
                    catch (System.ObjectDisposedException e)
                    {
                        DynamicConfiguration.RaiseMessage.Invoke($"Server Closed: {e.Message}", "Server Exception");
                        throw new Exceptions.ServerException($"Server Closed: {e.Message}");
                    }
                    catch (System.IO.IOException e)
                    {
                        DynamicConfiguration.RaiseMessage.Invoke($"Server Closed or Client Disconnected:  {e.Message}", "Server Exception");
                        throw new Exceptions.ServerException($"Server Closed:  {e.Message}");
                    }
                    catch (Exception e)
                    {
                        DynamicConfiguration.RaiseMessage.Invoke($"Server Closed, unknown reason: {e.Message}", "Server Exception");
                        throw new Exceptions.ServerException($"Unknown Server Excetion: {e.Message}\n {e.StackTrace}");
                    }
                }).ConfigureAwait(false).GetAwaiter().GetResult();
            });
            ServerSender.Start();

            bool cancelConnection = false;

            Task.Run(async () =>
            {
                try
                {
                    while (cancelConnection == false)
                    {

                        int steer = await ConnSteer();
                        switch (steer)
                        {
                            case 1:
                                Busy = true; privateBusy = true;
                                srv.HandleReceivedText.Invoke(await GetText(srv.TextReceiveEncoding));
                                Busy = false; privateBusy = false;
                                break;
                            case 2:
                                Busy = true; privateBusy = true;
                                await GetFile(srv);
                                Busy = false; privateBusy = false;
                                break;
                            case 3:
                                Busy = true; privateBusy = true;
                                srv.HandleReceivedBytes.Invoke(await GetRawBytes());
                                Busy = false; privateBusy = false;
                                break;

                            case 99:
                                Console.WriteLine("Client closed connection");
                                this.sslstream_.Dispose();
                                this.client_.Dispose();
                                cancelConnection = true;

                                break;
                        }
                    }
                }
                catch (System.ObjectDisposedException e)
                {
                    DynamicConfiguration.RaiseMessage.Invoke($"Server Closed: {e.Message}", "Server Exception");
                    throw new Exceptions.ServerException($"Server Closed: {e.Message}");
                }
                catch (System.IO.IOException e )
                {
                    DynamicConfiguration.RaiseMessage.Invoke($"Server Closed or Client Disconnected:  {e.Message}", "Server Exception");
                    throw new Exceptions.ServerException($"Server Closed:  {e.Message}");
                }
                catch (Exception e) {
                    DynamicConfiguration.RaiseMessage.Invoke($"Server Closed, unknown reason: {e.Message}", "Server Exception");
                    throw new Exceptions.ServerException($"Unknown Server Excetion: {e.Message}\n {e.StackTrace}");

                }
            }).GetAwaiter().GetResult();

        }
        private async Task<int> ConnSteer()
        {
            byte[] buffer = new byte[64];
            int steer = -999999;
            int bytes_count = -1;
            bytes_count = await sslstream_.ReadAsync(buffer, 0, buffer.Length);
            steer = BitConverter.ToInt32(buffer, 0);
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

                if (Message.ToString().IndexOf(Terminatorstring) != -1) { break; }

            } while (bytes_count != 0);

            string toreturn = Message.ToString();
            toreturn = toreturn.Substring(0, toreturn.IndexOf(Terminatorstring));
            return toreturn;
        }

        private Task GetFile(Server srv)
        {
            
            //file name

            int filenamebytes = -1;
            byte[] filenamebuffer = new byte[128];
            filenamebytes = sslstream_.Read(filenamebuffer);
            string filename = srv.FileNameEncoding.GetString(filenamebuffer).Trim(Convert.ToChar(0x00));
            

            int lengthbytes = -1;
            byte[] file_length_buffer = new byte[512];
            lengthbytes = sslstream_.Read(file_length_buffer);
            int FileLength = BitConverter.ToInt32(file_length_buffer);

            
            string[] FilesInDirectory = Directory.GetFiles(srv.ReceivedFilesLocation);

            bool correct = false;
            int number_of_occurence = 1;
            while (correct == false)
            {
                if (FilesInDirectory.Contains(filename))
                {
                    filename = filename + number_of_occurence;
                    number_of_occurence++;
                    
                }
                else
                {
                    correct = true;
                    
                }
            }


            
            byte[] ReceiveBuffer = new byte[DynamicConfiguration.TransportBufferSize];

            if (srv.ReceivedFilesLocation != "")
            {
                Directory.SetCurrentDirectory(srv.ReceivedFilesLocation);
            }
           
            FileStream fs = new FileStream(filename.Trim(), FileMode.Create);

         
            while ((sslstream_.Read(ReceiveBuffer, 0, ReceiveBuffer.Length) != 0))
            {


                fs.Write(ReceiveBuffer);
                
                if (fs.Length >= FileLength)
                {            
                    break;
                }
            }
          

            long ReceivedFileLength = fs.Length;

            if (ReceivedFileLength > FileLength)
            {
                fs.SetLength(FileLength);
            }


            fs.Dispose();

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            
            return Task.CompletedTask;
        }


        private async Task<byte[]> GetRawBytes()
        {
            byte[] lenghtBuffer = new byte[2048];
            int received = 0;
            received = await sslstream_.ReadAsync(lenghtBuffer, 0, lenghtBuffer.Length);

           

            byte[] MessageBytes = new byte[BitConverter.ToInt32(lenghtBuffer)];

            int MessageReceivedBytes = await sslstream_.ReadAsync(MessageBytes);

            return MessageBytes;
         
        }

        /// <summary>
        /// Sends raw bytes to the client
        /// </summary>
        /// <param name="Message"></param>
        public void SendRawBytes(byte[] Message)
        {
            Task.Run(async () =>
            {
                Action SendSteer = () =>
                {
                    sslstream_.Write(BitConverter.GetBytes((int)SteerCodes.SendRawBytes));
                };
                await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(SendSteer);

                Action SendLength = () =>
                {
                    sslstream_.Write(BitConverter.GetBytes(Message.Length));
                }; await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(SendLength);


                Action Send = () =>
                {
                    sslstream_.Write(Message);
                };
                await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(Send);


            });
        }

        /// <summary>
        /// Writes text(byte arrayof the text) to client
        /// </summary>
        /// <param name="message"></param>
        public void WriteText(byte[] message)
        {

            List<byte> messagebytes = new List<byte>();
            messagebytes.AddRange(message);
            messagebytes.AddRange(Encoding.UTF8.GetBytes(Terminatorstring));


            Action WR = () =>
            {

                sslstream_.Write(BitConverter.GetBytes((int)SteerCodes.SendText));
                sslstream_.Write(messagebytes.ToArray());

            };
            ServerSendingQueue.Writer.TryWrite(WR);
        }
        /// <summary>
        /// Sends file to client
        /// </summary>
        /// <param name="path">Path to the file</param>
        public void SendFile(string path)
        {
            Task.Run(async () =>
            {
                SslStream str = sslstream_;
                byte[] chunk = new byte[DynamicConfiguration.TransportBufferSize];

                // informs server that file will be sent
                Action SendSteer = () =>
                {
                    sslstream_.Write(BitConverter.GetBytes((int)SteerCodes.SendFile));
                };
                await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(SendSteer);


                // informs server what the filename is
                string filename = Path.GetFileName(path);
                Action SendFilename = () =>
                {
                    sslstream_.Write(FilenameEncoding.GetBytes(filename));
                };
                await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(SendFilename);

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                Action SendFileLength = () =>
                {

                    sslstream_.Write(BitConverter.GetBytes((int)fs.Length));
                }; await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(SendFileLength);


                int bytesLeft = (int)fs.Length;
                
               

                await Task.Delay(2000);

                int Received = 0;

                while (Received != fs.Length)
                {
                    Received += await fs.ReadAsync(chunk, 0, chunk.Length);
                    //  Console.WriteLine(fs.Position+"/"+fs.Length);

                    //Console.WriteLine(chunk.Length);
                    await str.WriteAsync(chunk);

                    // await Task.Delay(10);
                }


                await fs.DisposeAsync();




            }).ConfigureAwait(false).GetAwaiter().GetResult();
            // write.Dispose();
            //  fs.Dispose();
        }

        
        public void SendDirectory(string DirPath)
        {
            Task.Run(async () =>
            {
                List<string> FileList = new List<string>();
                Directory.GetFiles(DirPath, "*.*", SearchOption.AllDirectories);
                byte[] datachunk = new byte[DynamicConfiguration.TransportBufferSize];

                // informs server that directory will be sent
                Action SendSteer = () =>
                {
                    sslstream_.Write(BitConverter.GetBytes((int)SteerCodes.SendDirectory));
                };
                await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(SendSteer);

                ///////////////////////////////



                
                foreach(string file in FileList)
                {



                }




            });
        }


    }












}
