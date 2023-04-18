using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Exceptions
{
    [Serializable]
    internal class SignCSRConfigurationException : Exception
    {
        public SignCSRConfigurationException(string message) : base(message)
        {

        }

        public SignCSRConfigurationException() : base("Unknown configuration error")
        {

        }




    }
}
