using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Exceptions
{
    internal class SignCSRConfigurationException : Exception
    {
        public SignCSRConfigurationException(string message)
        {

        }

        public SignCSRConfigurationException() : base("Unknown configuration error")
        {

        }




    }
}
