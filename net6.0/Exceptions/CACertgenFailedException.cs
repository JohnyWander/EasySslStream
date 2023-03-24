using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Exceptions
{
    internal class CACertgenFailedException : Exception
    {

        public CACertgenFailedException(string message)
        {

        }

        public CACertgenFailedException() : base("CA certificate failed, unknown error") {
        
        }


    }
}
