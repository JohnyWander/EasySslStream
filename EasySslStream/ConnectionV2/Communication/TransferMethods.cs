using System;
using System.Collections.Generic;
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
            
        }

        #endregion

        #region helpers
        EncodingEnum resolveEncodingEnum(Encoding enc)
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

        #endregion
    }
}
