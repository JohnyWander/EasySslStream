using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using EasySslStream.ConnectionV2.Communication.TranferTypeConfigs;
using System.IO.Compression;
using System.Buffers.Text;
using System.IO;

namespace EasySslStream.ConnectionV2.Communication
{
    public abstract class TransformMethods
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

        protected internal SslStream stream;
        protected internal int _bufferSize;

        #region bytes
        internal async Task WriteBytesAsync(byte[] bytes,SteerCodes code)
        {
           
            int steercode = (int)code;
            byte[] steerBytes = BitConverter.GetBytes(steercode);

            await stream.WriteAsync(steerBytes);
            await stream.WriteAsync(bytes);           
        }

        internal async Task<int> ReadBytes(byte[] OutBuffer)
        {        
           int ReceivedCount = await stream.ReadAsync(OutBuffer);
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


            Debug.WriteLine($"Encoding is {encodingEnum.ToString()}");
            Debug.WriteLine($"text to send is {message}");
            await stream.WriteAsync(steerBytes);
            await stream.WriteAsync(encodingBytes);
            await Task.Delay(10);                       
            await stream.WriteAsync(work.encoding.GetBytes(message));
        }

        internal async Task<string> GetTextAsync(int bufferSize)
        {

            byte[] EncodingBytes = new byte[16];
            int received = await stream.ReadAsync(EncodingBytes);

           

            int encodingCode = BitConverter.ToInt32(EncodingBytes.Take(received).ToArray());
            Encoding encoding = ResolveEncodingFromEnum((EncodingEnum)encodingCode);

            Debug.WriteLine($"encodingCode is: {encodingCode}");
            Debug.WriteLine($"encoding is {encoding.EncodingName}");

            byte[] textReadBuffer = new byte[bufferSize];
            int textBytesReceived = await stream.ReadAsync(textReadBuffer);

            Debug.WriteLine($"Got {textBytesReceived} text bytes");
            //string receivedText = encoding.
            return encoding.GetString(textReadBuffer.Take(textBytesReceived).ToArray());

        }
        #endregion

        #region Files

        internal async Task SendFileAsync(string path,SteerCodes code)
        {
            int steercode = (int)code;
            byte[] steerBytes = BitConverter.GetBytes(steercode);            
            await stream.WriteAsync(steerBytes);
            

            string fileName = Path.GetFileName(path);
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            await stream.WriteAsync(fileNameBytes);

            FileStream fileStream = new FileStream(path, FileMode.Open,FileAccess.Read);
            
            byte[] lenghtBytes = BitConverter.GetBytes(fileStream.Length);
            await stream.WriteAsync(lenghtBytes);
            
            byte[] DataChunk = new byte[this._bufferSize];
            
            long Sended = 0;           
            while(Sended != fileStream.Length)
            {
                Sended += await fileStream.ReadAsync(DataChunk,0,DataChunk.Length);
                await stream.WriteAsync(DataChunk);
               
            }

            await fileStream.DisposeAsync();
        }

        internal async Task<string> GetFileAsync(string SaveDir)
        {
            
            byte[] FilenameBytes = new byte[256];
            int receivedCount = await stream.ReadAsync(FilenameBytes);
            string Filename = Encoding.UTF8.GetString(FilenameBytes.Take(receivedCount).ToArray());

            
            byte[] FileLengthBuffer = new byte[16];
            int LengthBytesReceived = await stream.ReadAsync(FileLengthBuffer);
            long ExpectedFileLentgh = BitConverter.ToInt64(FileLengthBuffer);

            
            FileStream saveStream = new FileStream(Path.Combine(SaveDir, Filename),FileMode.Create,FileAccess.Write);
            long FileBytesReceived = 0;
            byte[] buffer = new byte[this._bufferSize];



           
            while(FileBytesReceived <= ExpectedFileLentgh)
            {               
                FileBytesReceived += await stream.ReadAsync(buffer);                                        
                await saveStream.WriteAsync(buffer);
            }

            saveStream.SetLength(ExpectedFileLentgh);
            await saveStream.DisposeAsync();

            return Filename;
        }

        #endregion

        #region Directories

        internal async Task SendDirectory(string path,SteerCodes code)
        {
            int steercode = (int)code;
            byte[] steerBytes = BitConverter.GetBytes(steercode);
            await stream.WriteAsync(steerBytes);

            string DirInfo = SerializeDirectoryTransferInfo(path);
            byte[] DirInfoBytes = Encoding.UTF8.GetBytes(DirInfo);
            byte[] DirInfoSizeBytes = BitConverter.GetBytes(DirInfoBytes.LongLength);

            await stream.WriteAsync(DirInfoSizeBytes);
            await Task.Delay(100);
            await stream.WriteAsync(DirInfoBytes);
            await Task.Delay(100);

            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            byte[] DataChunk = new byte[this._bufferSize];
            try
            {
                foreach (string file in files)
                {

                    FileStream f = new FileStream(file, FileMode.Open, FileAccess.Read);                   
                    long Sended = 0;
                    while (Sended != f.Length)
                    {
                        Sended += await f.ReadAsync(DataChunk, 0, DataChunk.Length);
                        await stream.WriteAsync(DataChunk);

                    }

                    f.Dispose();
                }
            }catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            
        }

        internal async Task<string> GetDirectoryAsync(string workdir)
        {
            

            byte[] DirInfoSizeBytes = new byte[16];
            int receivedDirInfoSizeBytes = await stream.ReadAsync(DirInfoSizeBytes);
            long DirInfoSize = BitConverter.ToInt64(DirInfoSizeBytes.Take(receivedDirInfoSizeBytes).ToArray());

            byte[] DirInfoBytes = new byte[DirInfoSize];
            int receivedDirInfoBytes = await stream.ReadAsync(DirInfoBytes);
            string DirInfo = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(DirInfoBytes)));

            string[] DirInfoLines = DirInfo.Split('\n');
            string DirectoryName = DirInfoLines[0];
            string[] FileInfos = DirInfoLines.Skip(1).ToArray();

            string WorkingDirectory = workdir + "\\" + DirectoryName.Split("###")[0];

            Debug.WriteLine(WorkingDirectory);

            
            if (!Directory.Exists(WorkingDirectory))
            {
                Directory.CreateDirectory(WorkingDirectory);
            }
            
           
            foreach(string file in FileInfos)
            {
                string[] splitted = file.Split("###");
                string AllPath = splitted[0];
                string Size = splitted[1];

                long FileSize = long.Parse(Size);
                long FileBytesReceived = 0;

                string Dirpath = Path.GetDirectoryName(AllPath);
                //Debug.WriteLine(Dirpath);

                string Filename = Path.GetFileName(AllPath);
                //Debug.WriteLine($"{WorkingDirectory}\\{Dirpath}\\{Filename}");
                Directory.CreateDirectory($"{WorkingDirectory}\\{Dirpath}");
                
                FileStream saveStream = new FileStream($"{WorkingDirectory}\\{Dirpath}\\{Filename}", FileMode.Create, FileAccess.Write);
                
                byte[] buffer = new byte[this._bufferSize];
                while (FileBytesReceived <= FileSize)
                {
                    FileBytesReceived += await stream.ReadAsync(buffer);
                    await saveStream.WriteAsync(buffer);
                }

                await saveStream.DisposeAsync();
                
            }
                


            return "XD";
        }



        #endregion


        #region helpers

        string SerializeDirectoryTransferInfo(string path)
        {
            StringBuilder sb = new StringBuilder();

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            
            string[] files = Directory.GetFiles(path,"*.*",SearchOption.AllDirectories);

            sb.AppendLine($"{Path.GetFileName(path)}###DIRNAME");
            
            foreach(string file in files)
            {                
                FileInfo fileInfo = new FileInfo(file);
                sb.AppendLine($"{file.Split(directoryInfo.Name + "\\")[1]}###{fileInfo.Length}");                
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        EncodingEnum ResolveEncodingEnum(Encoding enc)
        {
            if(enc == Encoding.UTF8)
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
            }else if(enc == Encoding.Unicode)
            {
                return EncodingEnum.Unicode;
            }else if(enc == Encoding.ASCII)
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
            if(enc == EncodingEnum.UTF8)
            {
                return Encoding.UTF8;
            }else if(enc == EncodingEnum.UTF32)
            {
                return Encoding.UTF32;
            }else if(enc == EncodingEnum.UTF7)
            {
                return Encoding.UTF7;
            }else if(enc == EncodingEnum.Unicode)
            {
                return Encoding.Unicode;
            }else if(enc == EncodingEnum.ASCII)
            {
                return Encoding.ASCII;
            }else if(enc == EncodingEnum.Custom)
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
