using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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

        string Terminator = "<ENDOFTEXT>";
        private Channel<Action> work = Channel.CreateUnbounded<Action>();

        public TcpClient client;
        public SslStream stream;

        /// <summary>
        /// True if server hostname must match subject name on the certificate. True by default
        /// </summary>
        public bool VerifyCertificateName = true;

        /// <summary>
        /// True if certificate sign chain should be valid, True by default
        /// </summary>
        public bool VerifyCertificateChain = true;



        /// <summary>
        /// Encoding of filenames UTF8 is default
        /// </summary>
        public Encoding FilenameEncoding = Encoding.UTF8;

        /// <summary>
        /// Encoding of received text from server, UTF8 is default
        /// </summary>
        public Encoding TextReceiveEncoding = Encoding.UTF8;

        /// <summary>
        /// Location of the received files
        /// </summary>
        public string ReceivedFilesLocation = AppDomain.CurrentDomain.BaseDirectory;

        private bool cancelConnection;

        private enum SteerCodes
        {
            SendText = 1,
            SendFile = 2,
            SendRawBytes = 3,
            SendDirectory = 4,

            SendDisconnect = 99,

            Confirmation = 200

        }

        public IFileReceiveEventAndStats FileReceiveEventAndStats = ConnectionCommons.Create();

        internal bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                DynamicConfiguration.RaiseMessage("CERT NOT AVAIABLE?!", "???");
                return false;
            }
            else
            {

                if (VerifyCertificateChain == false && VerifyCertificateName == false)
                {
                    return true;
                }


                DynamicConfiguration.RaiseMessage("???", "???");
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the server, disposes client's sslstream
        /// </summary>
        public void Disconnect()
        {
            stream.Dispose();
            client.Dispose();
        }


        private TaskCompletionSource<object> GentleDisconnectSource = new TaskCompletionSource<object>();

        public async void GentleDisconnect(bool informserver = false)
        {
            await GentleDisconnectSource.Task;
            work = null;
            if (informserver)
            {
                stream.Write(BitConverter.GetBytes((int)SteerCodes.SendDisconnect));
            }

            stream.Dispose();
            client.Dispose();
        }

        private bool privateBusy
        {
            set
            {
                if (value == false)
                {
                    GentleDisconnectSource.SetResult(null);
                }
                else
                {
                    GentleDisconnectSource = new TaskCompletionSource<object>();
                }
            }
        }


        /// <summary>
        /// Connects to the server
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            X509Certificate x = null;
            Thread cThread = new Thread(() =>
            {
                client = new TcpClient(ip, port);
                stream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate));

                try
                {
                    stream.WriteTimeout = -1;
                    stream.ReadTimeout = -1;
                    stream.AuthenticateAsClient(ip);

                    Thread ListeningThread = new Thread(() =>
                    {
                        try
                        {
                            Task.Run(async () =>
                            {
                                while (cancelConnection == false)
                                {
                                    int steer = await ConnSteer();
                                    switch (steer)
                                    {
                                        case 1:
                                            privateBusy = true;
                                            HandleReceivedText.Invoke(await GetText(TextReceiveEncoding));
                                            privateBusy = false;
                                            break;
                                        case 2:
                                            privateBusy = true;
                                            await GetFile();
                                            privateBusy = false;
                                            break;
                                        case 3:
                                            privateBusy = true;
                                            HandleReceivedBytes.Invoke(await GetRawBytes());
                                            privateBusy = false;
                                            break;
                                        case 4:
                                            privateBusy = true;
                                            await GetDirectory();
                                            privateBusy = false;
                                            break;


                                        case 99:
                                            //  privateBusy = true;
                                            Console.WriteLine("Server closed connection");
                                            this.stream.Dispose();
                                            this.client.Dispose();
                                            cancelConnection = true;
                                            // privateBusy = false;
                                            break;




                                    }



                                }


                            }).GetAwaiter().GetResult();

                        }
                        catch (System.ObjectDisposedException)
                        {
                            DynamicConfiguration.RaiseMessage.Invoke("Connection closed by client", "Client message");
                            throw new Exceptions.ConnectionException("Connection closed by client");
                        }
                        catch (System.IO.IOException e)
                        {
                            DynamicConfiguration.RaiseMessage.Invoke("Server Closed", "Client message");
                            throw new Exceptions.ConnectionException("Server closed or cannot be reached anymore" + e.Message);

                        }
                        catch (System.NullReferenceException)
                        {
                            DynamicConfiguration.RaiseMessage("Disconnected from server", "Client Exception");
                            throw new Exceptions.ConnectionException("Client disconnected from server");
                        }
                        catch (Exception e)
                        {
                            DynamicConfiguration.RaiseMessage.Invoke($"Connection crashed, unknown reason: {e.Message}", "Server Exception");
                            throw new Exceptions.ConnectionException($"Unknown Server Excpetion:{e.GetType().Name} {e.Message}\n {e.StackTrace}");

                        }

                    });
                    ListeningThread.Start();


                    Task.Run(async () =>
                    {
                        try
                        {
                            while (true)
                            {
                                await work.Reader.WaitToReadAsync();
                                Action w = await work.Reader.ReadAsync();

                                await Task.Delay(100);
                                privateBusy = true;
                                w.Invoke();
                                privateBusy = false;
                                w = null;
                            }
                        }
                        catch (System.ObjectDisposedException)
                        {
                            DynamicConfiguration.RaiseMessage.Invoke("Connection closed by client", "Server message");
                            throw new Exceptions.ConnectionException("Connection closed by client");
                        }
                        catch (System.IO.IOException)
                        {
                            DynamicConfiguration.RaiseMessage.Invoke("Server Closed", "Server message");
                            throw new Exceptions.ConnectionException("Server closed");

                        }
                        catch (Exception e)
                        {
                            DynamicConfiguration.RaiseMessage.Invoke($"Connection crashed, unknown reason: {e.Message}", "Server Exception");
                            throw new Exceptions.ConnectionException($"Unknown Server Excpetion:{e.GetType().Name} {e.Message}\n {e.StackTrace}");

                        }
                    }).ConfigureAwait(false).GetAwaiter().GetResult();

                    stream.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            cThread.Start();




        }
        /// <summary>
        /// Connects to the server that verifies client certificates
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="clientCertLocation">path to the client pfx cert with private key</param>
        /// <param name="certPassword">Password to the cert, use empty string if there is no password</param>
        public void Connect(string ip, int port, string clientCertLocation, string certPassword)
        {
            X509Certificate x = null;
            Thread cThread = new Thread(() =>
            {
                client = new TcpClient(ip, port);
                stream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate));
                X509Certificate2 clientCert = new X509Certificate2(clientCertLocation, certPassword, X509KeyStorageFlags.PersistKeySet);
                X509Certificate2Collection certs = new X509Certificate2Collection(clientCert);

                stream.AuthenticateAsClient(ip, certs, false);
                try
                {
                    stream.WriteTimeout = -1;
                    stream.AuthenticateAsClient(ip);
                    stream.ReadTimeout = -1;

                    Task.Run(async () => // sending queue
                    {
                        while (true)
                        {
                            await work.Reader.WaitToReadAsync();
                            Action w = await work.Reader.ReadAsync();
                            await Task.Delay(100);
                            privateBusy = true;
                            w.Invoke();
                            privateBusy = false;
                            w = null;
                        }
                    }).ConfigureAwait(false).GetAwaiter().GetResult();
                    stream.Close();
                    stream.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            cThread.Start();
        }

        private async Task<int> ConnSteer()
        {
            byte[] buffer = new byte[64];
            int steer = -999999;

            int bytes_count = -1;

            bytes_count = await stream.ReadAsync(buffer, 0, buffer.Length);
            steer = BitConverter.ToInt32(buffer, 0);
            await stream.FlushAsync();

            return steer;
        }

        /// <summary>
        /// Send byte array representation of string to server
        /// </summary>
        /// <param name="message"></param>
        public void WriteText(byte[] message)
        {

            List<byte> messagebytes = new List<byte>();
            messagebytes.AddRange(message);
            messagebytes.AddRange(Encoding.UTF8.GetBytes(Terminator));

            Action WR = () =>
            {
                stream.Write(BitConverter.GetBytes((int)SteerCodes.SendText));
                stream.Write(messagebytes.ToArray());
            };
            work.Writer.TryWrite(WR);
        }

        /// <summary>
        /// Sends file to server
        /// </summary>
        /// <param name="path">Path to the file</param>
        public void SendFile(string path)
        {
            Task.Run(async () =>
            {
                SslStream str = stream;
                byte[] chunk = new byte[DynamicConfiguration.TransportBufferSize];

                // Sends information to server what type of message will be sent
                Action SendSteer = () =>
                {
                    stream.Write(BitConverter.GetBytes((int)SteerCodes.SendFile));
                };
                await work.Writer.WaitToWriteAsync();
                await work.Writer.WriteAsync(SendSteer);

                // informs server what the filename is
                string filename = Path.GetFileName(path);
                Action SendFilename = () =>
                {
                    stream.Write(FilenameEncoding.GetBytes(filename));
                };
                await work.Writer.WaitToWriteAsync();
                await work.Writer.WriteAsync(SendFilename);

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                Action SendFileLength = () =>
                {

                    stream.Write(BitConverter.GetBytes((int)fs.Length));
                }; await work.Writer.WaitToWriteAsync();
                await work.Writer.WriteAsync(SendFileLength);


                int bytesLeft = (int)fs.Length;
                int Readed = 0;


                int times = (int)fs.Length / 512;

                await Task.Delay(2000);

                int Received = 0;

                while (Received != fs.Length)
                {
                    Received += await fs.ReadAsync(chunk, 0, chunk.Length);
                    await str.WriteAsync(chunk);
                }

                await fs.DisposeAsync();

            }).ConfigureAwait(false).GetAwaiter().GetResult();
            // write.Dispose();
            //  fs.Dispose();
        }

        /// <summary>
        /// Sends directory over Sslstream
        /// </summary>
        /// <param name="DirPath">path to the directory</param>
        /// <param name="StopAndThrowOnFailedTransfer">Stops transfer when any file transfer failed, if true. if false it ignores files that couldn't be send</param>
        /// <param name="FailSafeSendInterval">Sometimes connection crashes after file info is sent, inverval can be set to prevent this issue from happenning
        /// Higher = slower transfer, smaller chance to fail
        /// 20ms by default </param>
        /// <exception cref="Exceptions.ConnectionException"></exception>
        public void SendDirectory(string DirPath, bool StopAndThrowOnFailedTransfer = true, int FailSafeSendInterval = 20)
        {
            Task.Run(async () =>
            {


                string[] Files = Directory.GetFiles(DirPath, "*.*", SearchOption.AllDirectories);
                //Console.WriteLine(Files.Length);
                byte[] datachunk = new byte[DynamicConfiguration.TransportBufferSize];

                // informs client that directory will be sent
                Action SendSteer = () =>
                {
                    stream.Write(BitConverter.GetBytes((int)SteerCodes.SendDirectory));
                };
                await work.Writer.WaitToWriteAsync();
                await work.Writer.WriteAsync(SendSteer);





                // Informs client about directory name
                Action SendDirectoryName = () =>
                {
                    stream.Write(FilenameEncoding.GetBytes(Path.GetFileName(DirPath)));
                    //  Console.WriteLine(Path.GetFileName(DirPath));
                };
                await work.Writer.WaitToWriteAsync();
                await work.Writer.WriteAsync(SendDirectoryName);

                Action SendFileAmount = () =>
                {
                    stream.Write(BitConverter.GetBytes(Files.Length));
                }; await work.Writer.WaitToWriteAsync(); await work.Writer.WriteAsync(SendFileAmount);

                bool LoopCancel = false;
                ///////////////////////////////      






                foreach (string file in Files)
                {
                    byte[] chunk = new byte[DynamicConfiguration.TransportBufferSize];
                    string innerPath = file.Split(Path.GetFileName(DirPath)).Last().Trim('\\').Trim(Convert.ToChar(0x00));



                    Action SendInnerDirectory = () =>
                    {
                        try
                        {
                            FileStream fs = new FileStream(file, FileMode.Open);
                            //    Task.Delay(100).Wait();
                            //    sslstream_.Write(srv.FileNameEncoding.GetBytes(innerPath));
                            //   Task.Delay(10000).Wait();
                            //   sslstream_.Write(BitConverter.GetBytes(fs.Length));

                            // Transfer will fail for unknown(for me) reason,if filename or directory name contains diacretic characters,
                            // Converting names to base64 prevents this error from occuring
                            string mes = innerPath + "$$$" + fs.Length;
                            mes = Convert.ToBase64String(FilenameEncoding.GetBytes(mes));
                            //Console.WriteLine(mes);
                            byte[] message = FilenameEncoding.GetBytes(mes);
                            stream.Write(message, 0, mes.Length);

                            Task.Delay(FailSafeSendInterval).Wait();

                            int sent = 0;

                            while (sent != fs.Length)
                            {
                                sent += fs.Read(chunk, 0, chunk.Length);

                                stream.Write(chunk);


                            }

                            stream.Flush();
                            fs.Dispose();

                            // Task.Delay(100).Wait();

                        }
                        catch (System.UnauthorizedAccessException e)
                        {
                            if (DynamicConfiguration.RaiseMessage != null)
                            {
                                DynamicConfiguration.RaiseMessage("Access denied to files in directory to transfer", "Directory transfer error");
                            }

                            if (StopAndThrowOnFailedTransfer)
                            {
                                throw new Exceptions.ServerException($"Acces denied to files in the folder {e.Message}\n{e.StackTrace}");
                                LoopCancel = true;
                            }
                            else
                            {
                                stream.Write(BitConverter.GetBytes((long)-10));
                            }

                        }





                    }; await work.Writer.WaitToWriteAsync(); await work.Writer.WriteAsync(SendInnerDirectory);

                    if (LoopCancel == true)
                    {
                        break;
                    }



                    //await Task.Delay(2000);


                }





            });



        }












        private async Task<string> GetText(Encoding enc)
        {
            byte[] buffer = new byte[64];

            int bytes_count = -1;
            StringBuilder Message = new StringBuilder();

            Decoder decoder = enc.GetDecoder();
            do
            {
                bytes_count = await stream.ReadAsync(buffer, 0, buffer.Length);

                char[] messagechars = new char[decoder.GetCharCount(buffer, 0, bytes_count, true)];
                decoder.GetChars(buffer, 0, bytes_count, messagechars, 0);
                Message.Append(messagechars);

                if (Message.ToString().IndexOf(Terminator) != -1) { break; }

            } while (bytes_count != 0);

            string toreturn = Message.ToString();
            toreturn = toreturn.Substring(0, toreturn.IndexOf(Terminator));
            return toreturn;
        }

        private Task GetFile()
        {
            
                CancellationTokenSource cts = new CancellationTokenSource();

                if (this.FileReceiveEventAndStats.AutoStartFileReceiveSpeedCheck)
                {
                    Task.Run(() =>
                    {
                        this.FileReceiveEventAndStats.StartFileReceiveSpeedCheck(this.FileReceiveEventAndStats.DefaultIntervalForFileReceiveCheck,
                            this.FileReceiveEventAndStats.DefaultSpeedUnit, cts.Token);
                    });
                }


                int filenamebytes = -1;
                byte[] filenamebuffer = new byte[128];
                filenamebytes = stream.Read(filenamebuffer);

                string filename = FilenameEncoding.GetString(filenamebuffer).Trim(Convert.ToChar(0x00));
               // Console.WriteLine(filename);

                int lengthbytes = -1;
                byte[] file_length_buffer = new byte[512];
                lengthbytes = stream.Read(file_length_buffer);
                int FileLength = BitConverter.ToInt32(file_length_buffer);

                this.FileReceiveEventAndStats.CurrentBytes = 0;
                this.FileReceiveEventAndStats.TotalBytes = FileLength;


                string[] FilesInDirectory = Directory.GetFiles(ReceivedFilesLocation);

                bool correct = false;
                int number_of_occurence = 1;
                while (correct == false)
                {
                    if (FilesInDirectory.Contains(filename))
                    {
                        filename = filename + number_of_occurence;
                        //Console.WriteLine(filename); 
                        number_of_occurence++;
                    }
                    else
                    {
                        correct = true;
                    }
                }

                int bytesReceived = 0;
                byte[] ReceiveBuffer = new byte[DynamicConfiguration.TransportBufferSize];

                if (ReceivedFilesLocation != "")
                {
                     Directory.SetCurrentDirectory(ReceivedFilesLocation);
                }
                FileStream fs = new FileStream(filename.Trim(), FileMode.Create);

                while ((stream.Read(ReceiveBuffer, 0, ReceiveBuffer.Length) != 0))
                {
                    fs.Write(ReceiveBuffer);
                    FileReceiveEventAndStats.CurrentBytes = (int)fs.Position;
                    FileReceiveEventAndStats.FireDataChunkReceived();
                    if (fs.Length >= FileLength)
                    {
                        break;
                    }
                }
                long ReceivedFileLength = fs.Length;

                if (ReceivedFileLength > FileLength) // as buffer is usually larger than last chunk of bytes
                {                                    // we have to cut stream to oryginal file length
                    fs.SetLength(FileLength);        // to remove NULL bytes from the stream
                }


                fs.Dispose();
                cts.Cancel();
                 Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                return Task.CompletedTask;
          
        }

        private Task GetDirectory()
        {
            //////////////////////////////
            /// Directory name
            int directoryBytesCount = 0;
            byte[] DirectoryNameBuffer = new byte[1024];
            directoryBytesCount = stream.Read(DirectoryNameBuffer);

            string DirectoryName = FilenameEncoding.GetString(DirectoryNameBuffer).Trim(Convert.ToChar(0x00)).TrimStart('\\').TrimStart('/');







            if (ReceivedFilesLocation == "")
            {
             //   Console.WriteLine(DirectoryName);
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                Directory.CreateDirectory(DirectoryName);
            }
            //////////////////////////////
            /// File count
            int FileCountBytesCount = 0;
            byte[] FileCountBuffer = new byte[512];
            FileCountBytesCount = stream.Read(FileCountBuffer);

            int FileCount = BitConverter.ToInt32(FileCountBuffer);
            //Console.WriteLine(FileCount);
            Directory.SetCurrentDirectory(DirectoryName);


            try
            {


                for (int i = 0; i <= FileCount; i++)
                {
                    stream.Flush();
                    byte[] DataChunk = new byte[DynamicConfiguration.TransportBufferSize];



                    int IneerDirectoryBytesCount = -1;
                    byte[] InnerDirectoryNameBuffer = new byte[512];

                    stream.Read(InnerDirectoryNameBuffer, 0, InnerDirectoryNameBuffer.Length);
                    stream.Flush();
                    Task.Delay(100).Wait();





                    // Transfer will fail for unknown(for me) reason,if filename or directory name contains diacretic characters,
                    // Converting names to base64 prevents this error from occuring

                    string innerPath = FilenameEncoding.GetString(InnerDirectoryNameBuffer).Trim(Convert.ToChar(0x00));
                    innerPath  = FilenameEncoding.GetString(Convert.FromBase64String(innerPath));
                    //Console.WriteLine(innerPath);
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
                            Directory.CreateDirectory(Path.GetDirectoryName(innerPath));
                        }
                        // Console.WriteLine(innerPath);

                        FileStream fs = new FileStream(innerPath, FileMode.Create, FileAccess.Write);

                        while ((stream.Read(DataChunk, 0, DataChunk.Length) != 0))
                        {
                            fs.Write(DataChunk);
                            if (fs.Length >= FileLength)
                            {
                                break;
                            }
                        }
                        long ReceivedFileLength = fs.Length;

                        if (ReceivedFileLength > FileLength) // as buffer is usually larger than last chunk of bytes
                        {                                    // we have to cut stream to oryginal file length
                            fs.SetLength(FileLength);        // to remove NULL bytes from the stream
                        }

                        fs.Dispose();

                        stream.Flush();



                    }



                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            return Task.CompletedTask;
        }










        public async Task<byte[]> GetRawBytes()
        {
            byte[] lenghtBuffer = new byte[2048];
            int received = 0;
            received = await stream.ReadAsync(lenghtBuffer, 0, lenghtBuffer.Length);

            byte[] MessageBytes = new byte[BitConverter.ToInt32(lenghtBuffer)];

            int MessageReceivedBytes = await stream.ReadAsync(MessageBytes);

            return MessageBytes;
        }
        /// <summary>
        /// Sends raw bytes message 
        /// </summary>
        /// <param name="Message">bytes to send</param>
        public void SendRawBytes(byte[] Message)
        {
            Task.Run(async () =>
            {
                Action SendSteer = () =>
                {
                    stream.Write(BitConverter.GetBytes((int)SteerCodes.SendRawBytes));
                };
                await work.Writer.WaitToWriteAsync();
                await work.Writer.WriteAsync(SendSteer);

                Action SendLength = () =>
                {
                    stream.Write(BitConverter.GetBytes(Message.Length));
                }; await work.Writer.WaitToWriteAsync();
                await work.Writer.WriteAsync(SendLength);

                Action Send = () =>
                {
                    stream.Write(Message);
                };
                await work.Writer.WaitToWriteAsync();
                await work.Writer.WriteAsync(Send);


            });
        }
    }
}