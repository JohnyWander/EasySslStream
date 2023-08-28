﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EasySslStream.Connection.Client
{
    /// <summary>
    /// Represents connected client 
    /// </summary>
    public sealed class SSLClient
    {
        #region enums
        private enum SteerCodes
        {
            SendText = 1,
            SendFile = 2,
            SendRawBytes = 3,
            SendDirectory = 4,
            SendDirectoryV2 = 5,
            Confirmation = 200,
            SendDisconnect = 99
        }

        #endregion

        #region Settable fields
        /// <summary>
        /// Location for the received file from clients
        /// </summary>
        public string ReceivedFilesLocation = AppDomain.CurrentDomain.BaseDirectory;

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

        /// <summary>
        /// U can choose encrypiton protocols like SslProtocols = SslProtocols.TLS11|SslProtocols , leave "null" for default configuration
        /// </summary>
        

        #endregion

        #region Client instance related
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
        #endregion

        #region Connection Statistics

        public IFileReceiveEventAndStats FileReceiveEventAndStats = ConnectionCommons.CreateFileReceive();
        public IFileSendEventAndStats FileSendEventAndStats = ConnectionCommons.CreateFileSend();
        public IDirectorySendEventAndStats DirectorySendEventAndStats = ConnectionCommons.CreateDirectorySendEventAndStats();
        public IDirectoryReceiveEventAndStats DirectoryReceiveEventAndStats = ConnectionCommons.CreateDirectoryReceiveEventAndStats();
        #endregion

        #region ConnectionEvents

        //public event ServerEvent ReceivedFile;
        public event ServerEvent ReceivedDirectory;
        public event ServerEvent ReceivedFile;

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
            foreach (byte b in bytes) { Console.Write(Convert.ToInt32(b) + " "); }
            //return bytes
        };

        #endregion

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
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                Console.WriteLine("CERT NOV AVAIABLE????");
                return false;
            }
            else
            {
                return false;
            }

        }


        #region Connection handling

        /// <summary>
        /// Creates client instance
        /// </summary>
        /// <param name="client">Tcpclient instance</param>
        /// <param name="serverCert">Server certificate</param>
        /// <param name="VerifyClients"></param>
        /// <param name="srvinstance"></param>
        public SSLClient(TcpClient client, X509Certificate2 serverCert, bool VerifyClients, Server? srvinstance = null)
        {
            Busy = false;
            client_ = client;
            srv = srvinstance;
            bool cancelConnection = false;


            srv.ConnectedClients.Add(this);
            srv.ConnectedClientsByNumber.TryAdd(srv.ConnectedClients.Count, this);
            srv.ConnectedClientsByEndPoint.TryAdd((IPEndPoint)client.Client.RemoteEndPoint, this);

            if (VerifyClients == false)
            {
                sslstream_ = new SslStream(client.GetStream(), false);
                if (srv.SslProtocols == null) { sslstream_.AuthenticateAsServer(serverCert, clientCertificateRequired: false, true); }
                else { sslstream_.AuthenticateAsServer(serverCert, clientCertificateRequired: false, srv.SslProtocols, true); }
            }
            else
            {
                sslstream_ = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidadeClientCert));
                if (srv.SslProtocols == null) { sslstream_.AuthenticateAsServer(serverCert, clientCertificateRequired: true, true); }
                else { sslstream_.AuthenticateAsServer(serverCert, clientCertificateRequired: true, srv.SslProtocols, true); }
            }

            srv.RaiseClientConnected();

            StaertClientServer();
            StartClientReceiver();                     
        }
        private void StaertClientServer()
        {
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
                        throw new Exceptions.ServerException($"Server Closed: {e.Message}");
                    }
                    catch (System.IO.IOException e)
                    {
                        throw new Exceptions.ServerException($"Server Closed:  {e.Message}");
                    }
                    catch (Exception e)
                    {
                        throw new Exceptions.ServerException($"Unknown Server Excetion: {e.Message}\n {e.StackTrace}");
                    }
                }).ConfigureAwait(false).GetAwaiter().GetResult();
            });
            ServerSender.Start();
        }
        private void StartClientReceiver()
        {
            Task.Run(async () =>
            {
                bool cancelConnection = false;
                try
                {
                    while (cancelConnection == false)
                    {

                        int steer = await ConnSteer();
                        switch (steer)
                        {
                            case 1:
                                Busy = true; privateBusy = true;
                                this.HandleReceivedText.Invoke(await GetText(srv.TextReceiveEncoding));
                                Busy = false; privateBusy = false;
                                break;
                            case 2:
                                Busy = true; privateBusy = true;
                                await GetFile(srv);
                                this.ReceivedFile.Invoke();
                                Busy = false; privateBusy = false;
                                break;
                            case 3:
                                Busy = true; privateBusy = true;
                                this.HandleReceivedBytes?.Invoke(await GetRawBytes());
                                Busy = false; privateBusy = false;
                                break;

                            case 4:
                                Busy = true; privateBusy = true;
                                await GetDirectory();
                                this.ReceivedDirectory?.Invoke();
                                Busy = false; privateBusy = false;
                                break;

                            case 5:
                                Busy = true; privateBusy = true;
                                await GetDirectoryV2();
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
                    throw new Exceptions.ServerException($"Server Closed: {e.Message}");
                }
                catch (System.IO.IOException e)
                {
                    throw new Exceptions.ServerException($"Server Closed:  {e.Message}");
                }
                catch (Exception e)
                {
                    throw new Exceptions.ServerException($"Unknown Server Excetion: {e.Message}\n {e.StackTrace}");
                }
            }).GetAwaiter().GetResult();
        }
        #endregion







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
            CancellationTokenSource cancelCST = new CancellationTokenSource();

            if (this.FileReceiveEventAndStats.AutoStartFileReceiveSpeedCheck)
            {
                Task.Run(() =>
                {
                    this.FileReceiveEventAndStats.StartFileReceiveSpeedCheck(this.FileReceiveEventAndStats.DefaultIntervalForFileReceiveCheck,
                        this.FileReceiveEventAndStats.DefaultReceiveSpeedUnit, cancelCST.Token);
                });
            }

            //file name

            int filenamebytes = -1;
            byte[] filenamebuffer = new byte[128];
            filenamebytes = sslstream_.Read(filenamebuffer);
            string filename = srv.FileNameEncoding.GetString(filenamebuffer).Trim(Convert.ToChar(0x00));


            int lengthbytes = -1;
            byte[] file_length_buffer = new byte[512];
            lengthbytes = sslstream_.Read(file_length_buffer);
            long FileLength = BitConverter.ToInt64(file_length_buffer);

            this.FileReceiveEventAndStats.CurrentReceivedBytes = 0;
            this.FileReceiveEventAndStats.TotalBytesToReceive = FileLength;




            string[] FilesInDirectory = Directory.GetFiles(this.ReceivedFilesLocation);

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



            byte[] ReceiveBuffer = new byte[srv.bufferSize];

            if (this.ReceivedFilesLocation != "")
            {
                Directory.SetCurrentDirectory(this.ReceivedFilesLocation);
            }

            FileStream fs = new FileStream(filename.Trim(), FileMode.Create);


            while ((sslstream_.Read(ReceiveBuffer, 0, ReceiveBuffer.Length) != 0))
            {


                fs.Write(ReceiveBuffer);
                FileReceiveEventAndStats.CurrentReceivedBytes = fs.Position;
                FileReceiveEventAndStats.FireDataChunkReceived();



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
            // cancelCST.Cancel();
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

        private Task GetDirectory()
        {

            CancellationTokenSource GDCancel = new CancellationTokenSource();

            if (DirectoryReceiveEventAndStats.AutoStartDirectoryReceiveSpeedCheck)
            {
                Task.Run(() =>
                {
                    DirectoryReceiveEventAndStats.StartDirectoryReceiveSpeedCheck(DirectoryReceiveEventAndStats.DirectoryReceiveCheckInterval,
                        DirectoryReceiveEventAndStats.DefaultDirectoryReceiveUnit, GDCancel.Token);

                });


            }




            //////////////////////////////
            /// Directory name
            int directoryBytesCount = 0;
            byte[] DirectoryNameBuffer = new byte[1024];
            directoryBytesCount = sslstream_.Read(DirectoryNameBuffer);

            string DirectoryName = FilenameEncoding.GetString(DirectoryNameBuffer).Trim(Convert.ToChar(0x00)).TrimStart('\\').TrimStart('/');

            string WorkDir = "";

            if (this.ReceivedFilesLocation == AppDomain.CurrentDomain.BaseDirectory)
            {
                WorkDir = AppDomain.CurrentDomain.BaseDirectory + "\\";
                Directory.CreateDirectory(WorkDir + DirectoryName);
            }
            else if (this.ReceivedFilesLocation == "")
            {
                WorkDir = AppDomain.CurrentDomain.BaseDirectory + "\\";
            }
            else
            {
                WorkDir = this.ReceivedFilesLocation + "\\";
            }
            //////////////////////////////
            /// File count
            int FileCountBytesCount = 0;
            byte[] FileCountBuffer = new byte[512];
            FileCountBytesCount = sslstream_.Read(FileCountBuffer);

            int FileCount = BitConverter.ToInt32(FileCountBuffer);
            DirectoryReceiveEventAndStats.TotalFilesToReceive = FileCount;

            //Console.WriteLine(FileCount);
            // Directory.SetCurrentDirectory(DirectoryName);


            try
            {


                for (int i = 0; i <= FileCount; i++)
                {
                    DirectoryReceiveEventAndStats.CurrentReceiveFile++;
                    sslstream_.Flush();
                    byte[] DataChunk = new byte[srv.bufferSize];



                    int IneerDirectoryBytesCount = -1;
                    byte[] InnerDirectoryNameBuffer = new byte[512];

                    sslstream_.Read(InnerDirectoryNameBuffer, 0, InnerDirectoryNameBuffer.Length);
                    sslstream_.Flush();
                    Task.Delay(100).Wait();





                    // Transfer will fail for unknown(for me) reason,if filename or directory name contains diacretic characters,
                    // Converting names to base64 prevents this error from occuring

                    string innerPath = FilenameEncoding.GetString(InnerDirectoryNameBuffer).Trim(Convert.ToChar(0x00));
                    innerPath = FilenameEncoding.GetString(Convert.FromBase64String(innerPath));
                    Console.WriteLine(innerPath);
                    string[] msplit = innerPath.Split("$$$");
                    innerPath = msplit[0].TrimStart('\\').TrimStart('/');
                    long FileLength = Convert.ToInt64(msplit[1]);


                    //Console.WriteLine("FILE LENGTH: " + FileLength);

                    if (FileLength == (long)-10)
                    {
                        continue;
                    }
                    else
                    {


                        if (innerPath.Contains("\\"))
                        {
                            Directory.CreateDirectory(WorkDir + DirectoryName + "\\" + Path.GetDirectoryName(innerPath));
                        }
                        // Console.WriteLine(innerPath);





                        FileStream fs = new FileStream(WorkDir + DirectoryName + "\\" + innerPath, FileMode.Create, FileAccess.Write);

                        DirectoryReceiveEventAndStats.CurrentReceiveFileCurrentBytes = 0;
                        DirectoryReceiveEventAndStats.CurrentReceiveFileTotalBytes = fs.Length;
                        DirectoryReceiveEventAndStats.CurrentReceivedFileName = innerPath;

                        while ((sslstream_.Read(DataChunk, 0, DataChunk.Length) != 0))
                        {
                            fs.Write(DataChunk);
                            if (fs.Length >= FileLength)
                            {

                                DirectorySendEventAndStats.CurrentSendFileCurrentBytes = (int)fs.Length;
                                break;
                            }
                            DirectorySendEventAndStats.CurrentSendFileCurrentBytes = (int)fs.Length;
                        }
                        long ReceivedFileLength = fs.Length;

                        if (ReceivedFileLength > FileLength) // as buffer is usually larger than last chunk of bytes
                        {                                    // we have to cut stream to oryginal file length
                            fs.SetLength(FileLength);        // to remove NULL bytes from the stream
                        }

                        fs.Dispose();

                        sslstream_.Flush();
                        DirectoryReceiveEventAndStats.RaiseOnFileFromDirectoryReceiveProcessed();

                        GDCancel.Cancel();

                    }



                }

            }
            catch (Exception e)
            {
                throw new Exceptions.ServerException("Error occured while receiving directory" + e.GetType().Name + "\n" + e.Message);
            }

            //Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            return Task.CompletedTask;
        }



        private Task GetDirectoryV2()
        {
            CancellationTokenSource GDCancel = new CancellationTokenSource();

            if (DirectoryReceiveEventAndStats.AutoStartDirectoryReceiveSpeedCheck)
            {
                Task.Run(() =>
                {

                    DirectoryReceiveEventAndStats.StartDirectoryReceiveSpeedCheck(DirectoryReceiveEventAndStats.DirectoryReceiveCheckInterval,
                       DirectoryReceiveEventAndStats.DefaultDirectoryReceiveUnit, GDCancel.Token);

                });




            }

            //////////////////////////////
            /// Directory name
            int directoryBytesCount = 0;
            byte[] DirectoryNameBuffer = new byte[1024];
            directoryBytesCount = sslstream_.Read(DirectoryNameBuffer);

            string DirectoryName = FilenameEncoding.GetString(DirectoryNameBuffer).Trim(Convert.ToChar(0x00)).TrimStart('\\').TrimStart('/');

            int FileInfosBytesCount = 0;
            byte[] FileInfosReceiveBuffer = new byte[1024];
            FileInfosBytesCount = sslstream_.Read(FileInfosReceiveBuffer);

            string base64Message = FilenameEncoding.GetString(FileInfosReceiveBuffer).Trim(Convert.ToChar(0x00));


            string DecodedMessage = FilenameEncoding.GetString(Convert.FromBase64String(base64Message));



            string WorkDir = this.ReceivedFilesLocation+"\\";

           




            string[] files = DecodedMessage.Split("^^^");
            foreach (string file in files)
            {
                string[] InfoSplit = file.Split("@@@");
                string InnerPath = InfoSplit[0];
                long FileLength = Convert.ToInt64(InfoSplit[1]);
                DirectoryReceiveEventAndStats.CurrentReceiveFile++;

                byte[] DataChunk = new byte[srv.bufferSize];

                
                    Directory.CreateDirectory(WorkDir + DirectoryName + "\\" + Path.GetDirectoryName(InnerPath));
                

                // Console.WriteLine(WorkDir);
                //Console.WriteLine(DirectoryName);
                //Console.WriteLine(InnerPath);
                FileStream fs = new FileStream(WorkDir + DirectoryName + "\\" + InnerPath, FileMode.Create, FileAccess.Write);
                DirectoryReceiveEventAndStats.CurrentReceiveFileCurrentBytes = 0;
                DirectoryReceiveEventAndStats.CurrentReceiveFileTotalBytes = fs.Length;
                DirectoryReceiveEventAndStats.CurrentReceivedFileName = InnerPath;
                while ((sslstream_.Read(DataChunk, 0, DataChunk.Length) != 0))
                {
                    fs.Write(DataChunk);
                    if (fs.Length >= FileLength)
                    {
                        DirectoryReceiveEventAndStats.CurrentReceiveFileCurrentBytes = (int)fs.Length;

                        break;
                    }
                    DirectoryReceiveEventAndStats.CurrentReceiveFileCurrentBytes = (int)fs.Length;
                }
                long ReceivedFileLength = fs.Length;

                if (ReceivedFileLength > FileLength) // as buffer is usually larger than last chunk of bytes
                {                                    // we have to cut stream to oryginal file length
                    fs.SetLength(FileLength);        // to remove NULL bytes from the stream
                }

                fs.Dispose();

                sslstream_.Flush();
                DirectoryReceiveEventAndStats.RaiseOnFileFromDirectoryReceiveProcessed();



            }

            GDCancel.Cancel();







            return Task.CompletedTask;
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

                CancellationTokenSource cancellConnectionSpeedCheck = new CancellationTokenSource();

                Task.Run(() =>
                {
                    FileSendEventAndStats.StartFileSendSpeedCheck(FileSendEventAndStats.FileSendSpeedCheckInterval,
                        FileSendEventAndStats.DefaultFileSendCheckUnit, cancellConnectionSpeedCheck.Token);
                });



                SslStream str = sslstream_;
                byte[] chunk = new byte[srv.bufferSize];

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
                FileSendEventAndStats.TotalBytesToSend = fs.Length;

                Action SendFileLength = () =>
                {

                    sslstream_.Write(BitConverter.GetBytes(fs.Length));
                }; await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(SendFileLength);






                await Task.Delay(2000);

                long Received = 0;

                while (Received != fs.Length)
                {
                    Received += await fs.ReadAsync(chunk, 0, chunk.Length);
                    //  Console.WriteLine(fs.Position+"/"+fs.Length);

                    //Console.WriteLine(chunk.Length);
                    await str.WriteAsync(chunk);
                    FileSendEventAndStats.CurrentSendBytes = Received;
                    // await Task.Delay(10);
                }


                await fs.DisposeAsync();


                cancellConnectionSpeedCheck.Cancel();

            }).ConfigureAwait(false).GetAwaiter().GetResult();
            // write.Dispose();
            //  fs.Dispose();
        }

        /// <summary>
        /// Sends directory over Sslstream, supports max 4096 bytes buffer size!
        /// if it crashes please use SendDirectoryV2
        /// </summary>
        /// <param name="DirPath">path to the directory</param>
        /// <param name="StopAndThrowOnFailedTransfer">Stops transfer when any file transfer failed, if true. if false it ignores files that couldn't be send</param>
        /// <param name="FailSafeSendInterval">Sometimes connection crashes after file info is sent, inverval can be set to prevent this issue from happenning
        /// Higher = slower transfer, smaller chance to fail
        /// 20ms by default </param>
        /// <exception cref="Exceptions.ServerException"></exception>
        /// 

        public void SendDirectory(string DirPath, bool StopAndThrowOnFailedTransfer = true, int FailSafeSendInterval = 20)
        {
            Task.Run(async () =>
            {
                CancellationTokenSource SDCancel = new CancellationTokenSource();

                if (DirectorySendEventAndStats.AutoStartDirectorySendSpeedCheck)
                {
                    Task.Run(() =>
                    {
                        DirectorySendEventAndStats.StartDirectorySendSpeedCheck(DirectorySendEventAndStats.DirectorySendCheckInterval,
                            DirectorySendEventAndStats.DefaultDirectorySendUnit, SDCancel.Token);

                    });
                }
                string[] Files = Directory.GetFiles(DirPath, "*.*", SearchOption.AllDirectories);
                DirectorySendEventAndStats.TotalFilesToSend = Files.Length;

                byte[] datachunk = new byte[srv.bufferSize];
                // informs client that directory will be sent
                Action SendSteer = () =>
                {
                    sslstream_.Write(BitConverter.GetBytes((int)SteerCodes.SendDirectory));
                };
                await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(SendSteer);



                // Informs client about directory name
                Action SendDirectoryName = () =>
                {
                    sslstream_.Write(srv.FileNameEncoding.GetBytes(Path.GetFileName(DirPath)));
                    Console.WriteLine(Path.GetFileName(DirPath));
                };

                await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(SendDirectoryName);

                Action SendFileAmount = () =>
                {
                    sslstream_.Write(BitConverter.GetBytes(Files.Length));
                }; await ServerSendingQueue.Writer.WaitToWriteAsync(); await ServerSendingQueue.Writer.WriteAsync(SendFileAmount);


                ///////////////////////////////      





                bool LoopCancel = false;
                foreach (string file in Files)
                {
                    DirectorySendEventAndStats.CurrentSendFile++;
                    byte[] chunk = new byte[srv.bufferSize];
                    string innerPath = file.Split(Path.GetFileName(DirPath)).Last().Trim('\\').Trim(Convert.ToChar(0x00));
                    DirectorySendEventAndStats.CurrentSendFilename = Path.GetFileName(innerPath);
                    //Console.WriteLine(innerPath);

                    Action SendInnerDirectory = () =>
                    {
                        try
                        {
                            FileStream fs = new FileStream(file, FileMode.Open);
                            DirectorySendEventAndStats.CurrentSendFileCurrentBytes = 0;
                            DirectorySendEventAndStats.CurrentSendFileTotalBytes = fs.Length;
                            DirectorySendEventAndStats.CurrentSendFilename = innerPath;

                            //    Task.Delay(100).Wait();
                            //    sslstream_.Write(srv.FileNameEncoding.GetBytes(innerPath));
                            //   Task.Delay(10000).Wait();
                            //   sslstream_.Write(BitConverter.GetBytes(fs.Length));



                            // Transfer will fail for unknown(for me) reason,if filename or directory name contains diacretic characters,
                            // Converting names to base64 prevents this error from occuring

                            string mes = innerPath + "$$$" + fs.Length;
                            mes = Convert.ToBase64String(srv.FileNameEncoding.GetBytes(mes));
                            // Console.WriteLine(mes);
                            byte[] message = srv.FileNameEncoding.GetBytes(mes);
                            sslstream_.Write(message, 0, mes.Length);

                            Task.Delay(FailSafeSendInterval).Wait();

                            int sent = 0;

                            while (sent != fs.Length)
                            {
                                sent += fs.Read(chunk, 0, chunk.Length);
                                sslstream_.Write(chunk);
                                DirectorySendEventAndStats.CurrentSendFileCurrentBytes = sent;
                            }
                            sslstream_.Flush();
                            fs.Dispose();
                            DirectorySendEventAndStats.RaiseOnFileFromDirectorySendProcessed();
                        }
                        catch (System.UnauthorizedAccessException e)
                        {
                      
                            if (StopAndThrowOnFailedTransfer)
                            {
                                throw new Exceptions.ServerException($"Acces denied to files in the folder {e.Message}\n{e.StackTrace}");
                                LoopCancel = true;
                            }
                            else
                            {
                                sslstream_.Write(BitConverter.GetBytes((long)-10));
                            }

                        }





                    }; await ServerSendingQueue.Writer.WaitToWriteAsync(); await ServerSendingQueue.Writer.WriteAsync(SendInnerDirectory);

                    if (LoopCancel == true)
                    {
                        break;
                    }



                    //await Task.Delay(2000);

                    DirectorySendEventAndStats.RaiseOnFileFromDirectorySendProcessed();
                }

                while (ServerSendingQueue.Reader.Count > 0)
                {
                    await Task.Delay(10);
                }
                SDCancel.Cancel();



            });
            //SDCancel.Cancel();


        }





        public void SendDirectoryV2(string DirPath, bool StopAndThrowOnFailedTransfer = true, int FailSafeSendInterval = 20)
        {

            Task.Run(async () =>
            {

                CancellationTokenSource SDCancele = new CancellationTokenSource();
                if (DirectorySendEventAndStats.AutoStartDirectorySendSpeedCheck)
                {

                    Thread t = new Thread(() =>
                    {

                        DirectorySendEventAndStats.StartDirectorySendSpeedCheck(DirectorySendEventAndStats.DirectorySendCheckInterval,
                        DirectorySendEventAndStats.DefaultDirectorySendUnit, SDCancele.Token).Wait();

                    });
                    t.Start();

                }




                List<string> FileInfos = new List<string>();
                string[] Files = Directory.GetFiles(DirPath, "*.*", SearchOption.AllDirectories);
                DirectorySendEventAndStats.TotalFilesToSend = Files.Length;

                foreach (string File in Files)
                {
                    string f = File.Split(Path.GetFileName(DirPath)).Last().Trim('\\').Trim(Convert.ToChar(0x00)); ;

                    FileInfo inf = new FileInfo(File);
                    string Info = f;
                    Info += $"@@@{inf.Length}";
                    FileInfos.Add(Info);
                    //    Console.WriteLine(Info);
                }

                string Message = "";
                foreach (string FileInfo in FileInfos)
                {
                    Message += FileInfo + "^^^";

                }
                Message = Message.TrimEnd('^');
                //Console.WriteLine(Message);

                string Base64Message = Convert.ToBase64String(srv.FileNameEncoding.GetBytes(Message));
                byte[] Base64Buffer = srv.FileNameEncoding.GetBytes(Base64Message);




                byte[] datachunk = new byte[srv.bufferSize];

                Action SendSteer = () =>
                {
                    sslstream_.Write(BitConverter.GetBytes((int)SteerCodes.SendDirectoryV2));
                };
                await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(SendSteer);

                Action SendDirectoryName = () =>
                {
                    sslstream_.Write(srv.FileNameEncoding.GetBytes(Path.GetFileName(DirPath)));
                    Console.WriteLine(Path.GetFileName(DirPath));
                };
                await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(SendDirectoryName);



                Action SendFileInfos = () =>
                {
                    sslstream_.Write(Base64Buffer, 0, Base64Buffer.Length);
                };
                await ServerSendingQueue.Writer.WaitToWriteAsync();
                await ServerSendingQueue.Writer.WriteAsync(SendFileInfos);


                bool LoopCancel = false;
                foreach (string file in Files)
                {
                    DirectorySendEventAndStats.CurrentSendFile++;
                    byte[] chunk = new byte[srv.bufferSize];
                    string innerPath = file.Split(Path.GetFileName(DirPath)).Last().Trim('\\').Trim(Convert.ToChar(0x00));
                    DirectorySendEventAndStats.CurrentSendFilename = Path.GetFileName(innerPath);
                    //Console.WriteLine(innerPath);

                    Action SendInnerDirectory = () =>
                    {
                        try
                        {
                            FileStream fs = new FileStream(file, FileMode.Open);
                            DirectorySendEventAndStats.CurrentSendFileCurrentBytes = 0;
                            DirectorySendEventAndStats.CurrentSendFileTotalBytes = fs.Length;
                            DirectorySendEventAndStats.CurrentSendFilename = innerPath;

                            //    Task.Delay(100).Wait();
                            //    sslstream_.Write(srv.FileNameEncoding.GetBytes(innerPath));
                            //   Task.Delay(10000).Wait();
                            //   sslstream_.Write(BitConverter.GetBytes(fs.Length));



                            // Transfer will fail for unknown(for me) reason,if filename or directory name contains diacretic characters,
                            // Converting names to base64 prevents this error from occuring

                            string mes = innerPath + "$$$" + fs.Length;
                            mes = Convert.ToBase64String(srv.FileNameEncoding.GetBytes(mes));
                            // Console.WriteLine(mes);
                            byte[] message = srv.FileNameEncoding.GetBytes(mes);
                            sslstream_.Write(message, 0, mes.Length);

                            Task.Delay(FailSafeSendInterval).Wait();

                            int sent = 0;

                            while (sent != fs.Length)
                            {
                                sent += fs.Read(chunk, 0, chunk.Length);

                                sslstream_.Write(chunk);
                                DirectorySendEventAndStats.CurrentSendFileCurrentBytes = sent;

                            }
                            sslstream_.Flush();
                            fs.Dispose();
                            DirectorySendEventAndStats.RaiseOnFileFromDirectorySendProcessed();
                        }
                        catch (System.UnauthorizedAccessException e)
                        {

                            if (StopAndThrowOnFailedTransfer)
                            {
                                throw new Exceptions.ServerException($"Acces denied to files in the folder {e.Message}\n{e.StackTrace}");
                                LoopCancel = true;
                            }
                            else
                            {
                                sslstream_.Write(BitConverter.GetBytes((long)-10));
                            }
                        }
                    }; await ServerSendingQueue.Writer.WaitToWriteAsync(); await ServerSendingQueue.Writer.WriteAsync(SendInnerDirectory);

                    if (LoopCancel == true)
                    {
                        break;
                    }
                    DirectorySendEventAndStats.RaiseOnFileFromDirectorySendProcessed();
                }
                while (ServerSendingQueue.Reader.Count > 0)
                {
                    await Task.Delay(10);
                }
                SDCancele.Cancel();
            });
        }
    }
}