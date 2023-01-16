using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public Action<string> HandleReceivedText = (string text) =>
        {
            Console.WriteLine(text);
        };

        public Action<byte[]> HandleReceivedBytes = (byte[] bytes) =>
        {
            foreach (byte b in bytes) { Console.Write(Convert.ToInt32(b) + " "); }
            //return bytes
        };

        string Terminator = "<ENDOFTEXT>";
        private Channel<Action> work = Channel.CreateUnbounded<Action>();

        public TcpClient client;
        public SslStream stream;

        public bool VerifyCertificateName = true;
        public bool VerifyCertificateChain = true;

        public Encoding FilenameEncoding = Encoding.UTF8;
        public Encoding TextReceiveEncoding = Encoding.UTF8;
        public string ReceivedFilesLocation = "";


        public bool cancelConnection;
       

        private enum SteerCodes
        {
            SendText = 1,
            SendFile = 2,
            SendRawBytes = 3
        }
       

        public bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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

                    Thread ListeningThread = new Thread(() =>
                    {
                        Task.Run(async () =>
                        {
                            //  try
                            //  {
                            while (cancelConnection == false)
                            {
                               // Console.WriteLine("Waiting for steer");
                                int steer = await ConnSteer();

                                Console.WriteLine(steer);


                                switch (steer)
                                {
                                    case 1:
                                        HandleReceivedText.Invoke(await GetText(TextReceiveEncoding));
                                        break;
                                    case 2:
                                        await GetFile();
                                        break;
                                    case 3:
                                        HandleReceivedBytes.Invoke(await GetRawBytes());
                                        break;

                                }



                            }
                            //  }
                            //  catch (Exception e)
                            //  {
                            //     Console.WriteLine(e.Message);
                            //  }
                        }).GetAwaiter().GetResult();

                    

                    });
                    ListeningThread.Start();



                    Task.Run(async () =>
                    {
                        while (true)
                        {


                            await work.Reader.WaitToReadAsync();
                            Action w = await work.Reader.ReadAsync();
                            await Task.Delay(100);
                            w.Invoke();
                            w = null;

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

                    Task.Run(async () =>
                    {
                        while (true)
                        {
                            await work.Reader.WaitToReadAsync();
                            Action w = await work.Reader.ReadAsync();
                            await Task.Delay(100);
                            w.Invoke();
                            w = null;

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


        public void SendFile(string path)
        {
            Task.Run(async () =>
            {
                SslStream str = stream;
                byte[] chunk = new byte[DynamicConfiguration.TransportBufferSize];

                // informs server that file will be sent
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

                // Console.WriteLine(Message.ToString());

                if (Message.ToString().IndexOf(Terminator) != -1) { break; }



            } while (bytes_count != 0);

            string toreturn = Message.ToString();
            toreturn = toreturn.Substring(0, toreturn.IndexOf(Terminator));
            return toreturn;

        }

        private Task GetFile()
        {
            //file name
            int filenamebytes = -1;
            byte[] filenamebuffer = new byte[128];
            filenamebytes = stream.Read(filenamebuffer);


            string filename = FilenameEncoding.GetString(filenamebuffer).Trim(Convert.ToChar(0x00));
            // Console.WriteLine("filename is: " + filename);

            int lengthbytes = -1;
            byte[] file_length_buffer = new byte[512];
            lengthbytes = stream.Read(file_length_buffer);
            int FileLength = BitConverter.ToInt32(file_length_buffer);

            // Console.WriteLine("File lenhth is: " + FileLength);
            /////////
            string[] FilesInDirectory = Directory.GetFiles(ReceivedFilesLocation);

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

            if (ReceivedFilesLocation != "")
            {
                Directory.SetCurrentDirectory(ReceivedFilesLocation);
            }
            //File.WriteAllText("debugfilename.txt", filename);
            FileStream fs = new FileStream(filename.Trim(), FileMode.Create);

            var watch = new Stopwatch();

           // watch.Start();
            while ((stream.Read(ReceiveBuffer, 0, ReceiveBuffer.Length) != 0))
            {


                fs.Write(ReceiveBuffer);
                //  Console.WriteLine(fs.Length + "/" + FileLength);
                if (fs.Length >= FileLength)
                {
                    //      Console.WriteLine("END");
                    break;
                }
            }
           // watch.Stop();
           // Console.WriteLine("Time elapsed: " + watch.ElapsedMilliseconds + " ms");

            long ReceivedFileLength = fs.Length;

            if (ReceivedFileLength > FileLength)
            {
                fs.SetLength(FileLength);
            }


            fs.Dispose();

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
