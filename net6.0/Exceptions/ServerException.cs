using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Exceptions
{
    internal class ServerException : Exception
    {


        public ServerException(string message) : base("Server Exception:" + message)
        {


        }


        public ServerException() : base("Unknown server Exception")
        {


        }

}

}
