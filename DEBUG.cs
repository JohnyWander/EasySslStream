using EasySslStream.CertGenerationClasses;
using EasySslStream.Connection.Full;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream
{

    public static class Program
    {

        static void Main()
        {

            Server srv = new Server(IPAddress.Any, 10000, "pfxcert.pfx.pfx","231",false);
            Console.WriteLine(srv.serverCert.GetCertHashString());
            




        }



    }
     
}
