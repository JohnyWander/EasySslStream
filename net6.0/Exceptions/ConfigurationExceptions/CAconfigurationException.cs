using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Exceptions
{
    [Serializable]
    public class CAconfigurationException : Exception
    {

        public CAconfigurationException()
        { }

        public CAconfigurationException(string message)
            : base(message)
        { }

        public CAconfigurationException(string message, Exception innerException)
            : base(message, innerException)
        { }

    }
}
