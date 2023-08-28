using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using EasySslStream.ConnectionV2.Communication.TranferTypeConfigs;

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

        #region helpers
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
