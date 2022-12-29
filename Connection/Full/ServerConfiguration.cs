using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Connection.Full
{
    internal class ServerConfiguration
    {

        public Action<byte[]>  TextMessageHandler_;
        public Action<byte[]> FileReceiveHandler_;


        public ServerConfiguration(Action<byte[]> TextMessageHandler,Action<byte[]> FileReceiveHandler)
        {
            TextMessageHandler_ = TextMessageHandler;
            FileReceiveHandler_ = FileReceiveHandler;

        }


    }
}
