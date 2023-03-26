using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Exceptions
{
    internal class CSRConfigurationException : Exception
    {

        public CSRConfigurationException() : base("At least one of CSR's required values are NOT set")
        {

        }

        public CSRConfigurationException(string Message) : base(Message)
        {

        }

    }
}
