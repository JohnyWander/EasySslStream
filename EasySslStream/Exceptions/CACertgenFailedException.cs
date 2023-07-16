using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Exceptions
{
    [Serializable]
    internal class CACertgenFailedException : Exception
    {
        string defaultMessage = "Certificate signing failed, Unknown error";
        public CACertgenFailedException(string message) : base (message)
        {

        }

        public CACertgenFailedException() : base("CA certificate failed, unknown error") {
        
        }


    }
}
