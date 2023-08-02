using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Exceptions
{
    public class SignCsrException : Exception
    {
        public SignCsrException(string Message) : base(Message) { }
    }
}
