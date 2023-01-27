using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Exceptions
{
    internal class ConnectionException :Exception
    {


        public ConnectionException(string message) : base("Connection:" + message)
        {


        }


        public ConnectionException() : base("Unknown Connection Exception")
        {


        }

    }
}
