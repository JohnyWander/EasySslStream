using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EndtoEndTestServer
{
    internal class HandleServer
    {
        public string IpToListenOn;
        public int PortToListenOn;
        public bool UseConsole;
        public void ExposeSendingConsole()
        {

        }

        public void Launch()
        {
            Console.WriteLine(IpToListenOn);
            Console.WriteLine(PortToListenOn);
            Console.WriteLine(UseConsole);
        }

        public HandleServer()
        {
            
        }
    }
}
