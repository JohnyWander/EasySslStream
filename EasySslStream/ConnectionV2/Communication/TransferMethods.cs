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

            string DirInfo = SerializeDirectoryTransferInfo(path);
            byte[] DirInfoBytes = Encoding.UTF8.GetBytes(DirInfo);
            byte[] DirInfoSizeBytes = BitConverter.GetBytes(DirInfoBytes.LongLength);

            stream.Write(DirInfoSizeBytes);
            Task.Delay(100).Wait();
            stream.Write(DirInfoBytes);
            Task.Delay(100).Wait();

            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

            try
            {

                foreach (string file in files)
                {
                    Task.Delay(100).Wait();

                    byte[] buffer = new byte[this._bufferSize];

                    FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                    int ReadFromFileStream = 0;

                    int sent = 0;

                    byte[] chunk = new byte[this._bufferSize];
                    while (sent != fs.Length)
                    {
                        sent += fs.Read(chunk, 0, chunk.Length);

                        stream.Write(chunk);
                        
                    }
                    stream.Flush();

                    fs.Close();
                }

            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Task.CompletedTask;
        }

        internal Task<string> GetDirectory(string workdir)
        {
           
            byte[] DirInfoSizeBytes = new byte[16];
            int receivedDirInfoSizeBytes = stream.Read(DirInfoSizeBytes);
            long DirInfoSize = BitConverter.ToInt64(DirInfoSizeBytes.Take(receivedDirInfoSizeBytes).ToArray());

            byte[] DirInfoBytes = new byte[DirInfoSize];
            int receivedDirInfoBytes = stream.Read(DirInfoBytes);
            string DirInfo = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(DirInfoBytes)));

            string[] DirInfoLines = DirInfo.Split('\n');
            string DirectoryName = DirInfoLines[0];
            string[] FileInfos = DirInfoLines.Where(x=>x.Contains("###")).Skip(1).ToArray();

            string WorkingDirectory = workdir + "\\" + DirectoryName.Split("###")[0];

            if (!Directory.Exists(WorkingDirectory))
            {
                Directory.CreateDirectory(WorkingDirectory);
            }
            else
            {
                Directory.Delete(WorkingDirectory, true);
                Directory.CreateDirectory(WorkingDirectory);
            }

            foreach (string file in FileInfos)
            {
                string[] splitted = file.Split("###");
                string AllPath = splitted[0];
                string Size = splitted[1];
                long FileSize = long.Parse(Size);
                long FileBytesReceived = 0;

                string Dirpath = Path.GetDirectoryName(AllPath);
                string Filename = Path.GetFileName(AllPath);

                Directory.CreateDirectory($"{WorkingDirectory}\\{Dirpath}");

                FileStream saveStream = new FileStream($"{WorkingDirectory}\\{Dirpath}\\{Filename}", FileMode.OpenOrCreate, FileAccess.Write);


                byte[] buffer = new byte[this._bufferSize];

                while ((stream.Read(buffer, 0, buffer.Length) != 0))
                {
                    saveStream.Write(buffer);
                    if (saveStream.Length >= FileSize)
                    {                        
                        break;
                    }
                }
                    stream.Flush();
                saveStream.SetLength(FileSize);

                saveStream.Dispose();
                
            }

            return Task.FromResult(WorkingDirectory);
        }



        #endregion


        #region helpers

        string SerializeDirectoryTransferInfo(string path)
        {
            StringBuilder sb = new StringBuilder();
            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            sb.AppendLine($"{Path.GetFileName(path)}###DIRNAME");

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                sb.AppendLine($"{file.Split(directoryInfo.Name + "\\")[1]}###{fileInfo.Length}");
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
        }

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
