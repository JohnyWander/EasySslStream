using EasySslStream.ConnectionV2.Communication.ConnectionSpeed;
using EasySslStream.ConnectionV2.Communication.TranferTypeConfigs;
using System.Diagnostics;
using System.Net.Security;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace EasySslStream.ConnectionV2.Communication
{
    public abstract class TransferMethods
    {
        enum EncodingEnum
        {
            UTF8 = 101,
            UTF32 = 102,
            UTF7 = 103,
            Unicode = 104,
            ASCII = 105,
            Custom = 999
        }

     
        

        protected TransferSpeedMeasurment SendSpeed;
        protected TransferSpeedMeasurment ReceiveSpeed;

        protected internal SslStream stream;
        protected internal int _bufferSize;

        
        protected internal TaskCompletionSource<object> PeerResponseWaiter = new TaskCompletionSource<object>();
        protected internal TaskCompletionSource<object> EndOfDirectroryFileTransmission = new TaskCompletionSource<object>();
        

        #region bytes
        internal async Task WriteBytesAsync(byte[] bytes, SteerCodes code)
        {            
            int steercode = (int)code;
            byte[] steerBytes = BitConverter.GetBytes(steercode);

            await stream.WriteAsync(steerBytes);
            await PeerResponseWaiter.Task;

            await stream.WriteAsync(bytes);
            await PeerResponseWaiter.Task;
        }

        internal async Task<int> ReadBytes(byte[] OutBuffer)
        {
            await stream.WriteAsync(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));

            int ReceivedCount = await stream.ReadAsync(OutBuffer);

            await stream.WriteAsync(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));
            return ReceivedCount;
        }
        #endregion


        #region strings


        internal async Task SendTextAsync(TextTransferWork work, SteerCodes code)
        {           
            EncodingEnum encodingEnum = ResolveEncodingEnum(work.encoding);
            string message = work.stringToSend;

            int steercode = (int)code;
            byte[] steerBytes = BitConverter.GetBytes(steercode);
            int encodingCode = (int)encodingEnum;
            byte[] encodingBytes = BitConverter.GetBytes(encodingCode);
            
            await stream.WriteAsync(steerBytes);
            await PeerResponseWaiter.Task;

            await stream.WriteAsync(encodingBytes);
            await PeerResponseWaiter.Task;

            await stream.WriteAsync(work.encoding.GetBytes(message));
            await PeerResponseWaiter.Task;          
        }

        internal async Task<string> GetTextAsync(int bufferSize)
        {
            await stream.WriteAsync(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));

            byte[] EncodingBytes = new byte[16];
            int received = await stream.ReadAsync(EncodingBytes);
            int encodingCode = BitConverter.ToInt32(EncodingBytes.Take(received).ToArray());

            await stream.WriteAsync(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));

            Encoding encoding = ResolveEncodingFromEnum((EncodingEnum)encodingCode);
            
            byte[] textReadBuffer = new byte[bufferSize];
            int textBytesReceived = await stream.ReadAsync(textReadBuffer);

            await stream.WriteAsync(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));
            return encoding.GetString(textReadBuffer.Take(textBytesReceived).ToArray());
        }
        #endregion

        #region Files
        internal async Task SendFileAsync(string path, SteerCodes code)
        {
            int steercode = (int)code;
            byte[] steerBytes = BitConverter.GetBytes(steercode);
            await stream.WriteAsync(steerBytes);
            await PeerResponseWaiter.Task;

            string fileName = Path.GetFileName(path);
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            await stream.WriteAsync(fileNameBytes);
            await PeerResponseWaiter.Task;

            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);

            byte[] lenghtBytes = BitConverter.GetBytes(fileStream.Length);
            await stream.WriteAsync(lenghtBytes);
            await PeerResponseWaiter.Task;

            byte[] DataChunk = new byte[this._bufferSize];

            long Sended = 0;
            this.SendSpeed.CurrentBufferPosition = 0;
            while (Sended != fileStream.Length)
            {
                Sended += await fileStream.ReadAsync(DataChunk, 0, DataChunk.Length);                
                await stream.WriteAsync(DataChunk);
                this.SendSpeed.CurrentBufferPosition = Sended;
            }
            await fileStream.DisposeAsync();


            await PeerResponseWaiter.Task;
        }

        internal async Task<string> GetFileAsync(string SaveDir)
        {
            await stream.WriteAsync(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));

            byte[] FilenameBytes = new byte[256];
            int receivedCount = await stream.ReadAsync(FilenameBytes);
            string Filename = Encoding.UTF8.GetString(FilenameBytes.Take(receivedCount).ToArray());
            await stream.WriteAsync(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));


            byte[] FileLengthBuffer = new byte[16];
            int LengthBytesReceived = await stream.ReadAsync(FileLengthBuffer);
            long ExpectedFileLentgh = BitConverter.ToInt64(FileLengthBuffer);
            await stream.WriteAsync(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));

                Directory.CreateDirectory(SaveDir);
            

            FileStream saveStream = new FileStream(Path.Combine(SaveDir, Filename), FileMode.OpenOrCreate, FileAccess.Write);
            long FileBytesReceived = 0;
            byte[] buffer = new byte[this._bufferSize];

            this.ReceiveSpeed.CurrentBufferPosition = 0;
            while (FileBytesReceived <= ExpectedFileLentgh)
            {
                FileBytesReceived += await stream.ReadAsync(buffer);
                await saveStream.WriteAsync(buffer);
                this.ReceiveSpeed.CurrentBufferPosition = FileBytesReceived;
            }

            saveStream.SetLength(ExpectedFileLentgh);
            await saveStream.DisposeAsync();

            await stream.WriteAsync(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));
            return Filename;
        }

        #endregion

        #region Directories


        internal Task SendDirectory(string path, SteerCodes code)
        {
            int steercode = (int)code;
            byte[] steerBytes = BitConverter.GetBytes(steercode);
            stream.Write(steerBytes);
            

            string DirName = Path.GetFileName(path);
            string[] Files = Directory.GetFiles(path,"*",SearchOption.AllDirectories);
            string fileCount = Convert.ToString(Files.Length);

            string BeginMessage=$"{DirName}###{fileCount}";
            string Base64BeginMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(BeginMessage));
            byte[] MessageBuffer = Encoding.UTF8.GetBytes(Base64BeginMessage);
            int MessageLength = MessageBuffer.Length;

            stream.Write(BitConverter.GetBytes(MessageLength));
            this.PeerResponseWaiter.Task.Wait();
            Task.Delay(100).Wait();

            stream.Write(MessageBuffer, 0, MessageLength);
            this.PeerResponseWaiter.Task.Wait();
            Task.Delay(100).Wait();

            foreach (string File in Files)
            {
                Task.Delay(1000).Wait();
                using(FileStream fs = new FileStream(File,FileMode.Open,FileAccess.Read))
                {
                    string relativePath = fs.Name.Split(DirName + "\\")[1];
                    string supportMessage = relativePath + "###" + fs.Length;
                    string BASE64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(supportMessage));
                    
                    byte[] BASE64MessageBuffer = Encoding.UTF8.GetBytes(BASE64Message);
                    byte[] supportLength = BitConverter.GetBytes(BASE64MessageBuffer.Length);

                    stream.Write(supportLength, 0, supportLength.Length);
                    this.PeerResponseWaiter.Task.Wait();

                    Task.Delay(1000).Wait();
                    
                    stream.Write(BASE64MessageBuffer,0,BASE64MessageBuffer.Length);
                    this.PeerResponseWaiter.Task.Wait();

                    Task.Delay(1000).Wait();

                    byte[] buffer = new byte[this._bufferSize];
                    int Read = 0;
                    int TotalRead = 0;
                    while(TotalRead <= fs.Length)
                    {
                       
                       Read = fs.Read(buffer, 0, buffer.Length);
                       TotalRead += Read;
                       stream.Write(buffer, 0, buffer.Length);                                       
                    }
                    stream.Flush();
                    
                }

                
               
            }

            return Task.CompletedTask;
        }

        internal  Task<string> GetDirectory(string workdir)
        {
            byte[] BeginMessagelengthBuffer = new byte[16];
            stream.Read(BeginMessagelengthBuffer,0,BeginMessagelengthBuffer.Length);
            stream.Write(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));

            byte[] MessageBuffer = new byte[BitConverter.ToInt32(BeginMessagelengthBuffer)];
            stream.Read(MessageBuffer,0,MessageBuffer.Length);
            stream.Write(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));

            string Base64BeginMessage = Encoding.UTF8.GetString(MessageBuffer);
            byte[] BeginMessageBytes = Convert.FromBase64String(Base64BeginMessage);
            string BeginMessage = Encoding.UTF8.GetString(BeginMessageBytes);

            //////////////////////////////////////

            string[] beginSplitted = BeginMessage.Split("###");
            string Dirname = beginSplitted[0];
            int FileCount = int.Parse(beginSplitted[1]);

            string WorkingDirectory = workdir + "\\" + Dirname;

            
            if (!Directory.Exists(WorkingDirectory))
            {
                Directory.CreateDirectory(WorkingDirectory);
            }
            else
            {
                Directory.Delete(WorkingDirectory, true);
                Directory.CreateDirectory(WorkingDirectory);
            }

            
            for(int i = 0; i < FileCount; i++)
            {
                Task.Delay(1000).Wait(); ;
                byte[] SupportMessageSizeBuffer = new byte[16];
                stream.Read(SupportMessageSizeBuffer,0,SupportMessageSizeBuffer.Length);               
                int SupportMessageSize = BitConverter.ToInt32(SupportMessageSizeBuffer);
                Console.WriteLine($"Support Message Size: {SupportMessageSize}");
                stream.Write(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));

                byte[] SupportMessageBuffer = new byte[SupportMessageSize];
                stream.Read(SupportMessageBuffer, 0, SupportMessageBuffer.Length);
                string BASE64ReceivedString = Encoding.UTF8.GetString(SupportMessageBuffer);
                string TrueSupportMessage = Encoding.UTF8.GetString(Convert.FromBase64String(BASE64ReceivedString));
                stream.Write(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));

                string[] splitTrueMessage = TrueSupportMessage.Split("###");
                string AllPath = splitTrueMessage[0];

                int fileSize = int.Parse(splitTrueMessage[1]);
                string Dirpath = Path.GetDirectoryName(AllPath);
                string Filename = Path.GetFileName(AllPath);

                Directory.CreateDirectory($"{WorkingDirectory}\\{Dirpath}");

                stream.Write(BitConverter.GetBytes((int)SteerCodes.ReceivedDataPropertly));

                
                using(FileStream SaveStream = new FileStream($"{WorkingDirectory}\\{Dirpath}\\{Filename}",FileMode.Create,FileAccess.Write))
                {
                    int received = 0;
                    int TotalReceived = 0;
                    byte[] buffer = new byte[this._bufferSize];

                    while ((stream.Read(buffer, 0, buffer.Length) != 0))
                    {
                        SaveStream.Write(buffer);
                        if (SaveStream.Length >= fileSize)
                        {
                            break;
                        }
                    }
                    stream.Flush();
                }


               
            }

            return Task.FromResult("XD");
        }



        #endregion


        #region helpers

      
        EncodingEnum ResolveEncodingEnum(Encoding enc)
        {
            if (enc == Encoding.UTF8)
            {
                return EncodingEnum.UTF8;
            }
            else if (enc == Encoding.UTF7)
            {
                return EncodingEnum.UTF7;
            }
            else if (enc == Encoding.UTF32)
            {
                return EncodingEnum.UTF32;
            }
            else if (enc == Encoding.Unicode)
            {
                return EncodingEnum.Unicode;
            }
            else if (enc == Encoding.ASCII)
            {
                return EncodingEnum.ASCII;
            }
            else
            {
                return EncodingEnum.Custom;
            }
        }

        Encoding ResolveEncodingFromEnum(EncodingEnum enc)
        {
            if (enc == EncodingEnum.UTF8)
            {
                return Encoding.UTF8;
            }
            else if (enc == EncodingEnum.UTF32)
            {
                return Encoding.UTF32;
            }
            else if (enc == EncodingEnum.UTF7)
            {
                return Encoding.UTF7;
            }
            else if (enc == EncodingEnum.Unicode)
            {
                return Encoding.Unicode;
            }
            else if (enc == EncodingEnum.ASCII)
            {
                return Encoding.ASCII;
            }
            else if (enc == EncodingEnum.Custom)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        #endregion
    }
}
