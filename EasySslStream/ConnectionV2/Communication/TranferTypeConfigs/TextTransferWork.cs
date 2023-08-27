using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.ConnectionV2.Communication.TranferTypeConfigs
{
    internal struct TextTransferWork
    {
        public Encoding encoding;
        public string stringToSend;

        public TextTransferWork(Encoding encoding,string message)
        {
            this.encoding = encoding; this.stringToSend = message;
        }
    }
}
