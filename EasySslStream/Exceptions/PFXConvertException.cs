using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Exceptions
{
    internal class PFXConvertException : Exception
    {
        public PFXConvertException(string message) : base(message) { }

        public PFXConvertException() : base("Unknown error when converting certificate to pfx format") { }
    }
}
