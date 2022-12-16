using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Exceptions
{
    internal class CountryCodeInvalidException : Exception
    {
        public CountryCodeInvalidException(string message) : base(message)
        {

        }

        public CountryCodeInvalidException(int Length)
          : base($"Provided Country code is invalid it's length must be 2 - Length: {Length}")
        {


        }
    }
}
