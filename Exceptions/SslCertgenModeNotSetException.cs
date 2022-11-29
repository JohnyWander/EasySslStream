using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Exceptions
{
    internal class SslCertgenModeNotSetException : Exception
    {

        public SslCertgenModeNotSetException() 
            : base("Certgen mode is not set! Please set it using SelectCertGenMode function in dynamic configuration") { 
        
        
        }
       
    }
}
