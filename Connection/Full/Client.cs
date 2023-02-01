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
        public string ReceivedFilesLocation = "";

        private bool cancelConnection;
        
        private enum SteerCodes
        {
            SendText = 1,
            SendFile = 2,
            SendRawBytes = 3,

            SendDisconnect = 99
        }
           
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
            else
            {
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
                    stream.WriteTimeout = 10000;     
                    stream.ReadTimeout = 10000;
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
                        catch(System.NullReferenceException)
                        {
                            DynamicConfiguration.RaiseMessage("Disconnected from server", "Client Exception");
                            throw new Exceptions.ConnectionException("Client disconnected from server");
                        }
                        catch (Exception e)
                        {
                            DynamicConfiguration.RaiseMessage.Invoke($"Connection crashed, unknown reason: {e.Message}", "Server Exception");
                            throw new Exceptions.ConnectionException($"Unknown Server Excpetion: {e.Message}\n {e.StackTrace}");

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
                            throw new Exceptions.ConnectionException($"Unknown Server Excpetion: {e.Message}\n {e.StackTrace}");

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
                    stream.WriteTimeout = 10000;
                    stream.AuthenticateAsClient(ip);
                    stream.ReadTimeout = 10000;

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
            int filenamebytes = -1;
            byte[] filenamebuffer = new byte[128];
            filenamebytes = stream.Read(filenamebuffer);

            string filename = FilenameEncoding.GetString(filenamebuffer).Trim(Convert.ToChar(0x00));

            int lengthbytes = -1;
            byte[] file_length_buffer = new byte[512];
            lengthbytes = stream.Read(file_length_buffer);
            int FileLength = BitConverter.ToInt32(file_length_buffer);

            string[] FilesInDirectory = Directory.GetFiles(ReceivedFilesLocation);

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

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            return Task.CompletedTask;
        }

        private Task GetDirectory()
        {
            //////////////////////////////
            /// Directory name
            int directoryBytesCount = 0;
            byte[] DirectoryNameBuffer = new byte[512];
            directoryBytesCount = stream.Read(DirectoryNameBuffer);

            string DirectoryName = FilenameEncoding.GetString(DirectoryNameBuffer).Trim(Convert.ToChar(0x00));

            if (ReceivedFilesLocation == "")
            {
                Console.WriteLine(DirectoryName);
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                Directory.CreateDirectory(DirectoryName);
            }
            //////////////////////////////
            /// File count
            int FileCountBytesCount = 0;
            byte[] FileCountBuffer = new byte[512];
            FileCountBytesCount = stream.Read(FileCountBuffer);

            int FileCount = BitConverter.ToInt32(FileCountBuffer);
            Console.WriteLine(FileCount);

            for(int i = 0; i <= FileCount; i++)
            {
                byte[] DataChunk = new byte[DynamicConfiguration.TransportBufferSize];

                int IneerDirectoryBytesCount = 0;
                byte[] InnerDirectoryNameBuffer = new byte[512];
                directoryBytesCount = stream.Read(InnerDirectoryNameBuffer);

                string innerPath = FilenameEncoding.GetString(InnerDirectoryNameBuffer).Trim(Convert.ToChar(0x00));

                Console.WriteLine("INNER PATH: " + innerPath);
                
                int FileLenthgBytesCount = 0;
                byte[] FileLengthBuffer = new byte[512];
                FileLenthgBytesCount = stream.Read(FileLengthBuffer);

                long FileLength = BitConverter.ToInt64(FileLengthBuffer);
                Console.WriteLine("FILE LENGTH: " + FileLength);
                
                if(FileLength == (long)-10)
                {
                    continue;
                }
                else
                {
                    A:
                    try
                    {
                        Directory.SetCurrentDirectory(DirectoryName);

                        Console.WriteLine(innerPath);
                        
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
                        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                    }
                    catch(DirectoryNotFoundException e)
                    {

                        Directory.CreateDirectory(Path.GetDirectoryName(innerPath));
                        goto A;

                    }
                 
                }
                


            }




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
