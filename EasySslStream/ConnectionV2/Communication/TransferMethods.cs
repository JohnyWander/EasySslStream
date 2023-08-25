using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.ConnectionV2.Communication
{
    public abstract class TransformMethods
    {
        protected internal SslStream stream;



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

    }
}
