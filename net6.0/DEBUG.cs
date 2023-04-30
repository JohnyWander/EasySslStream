using EasySslStream.CertGenerationClasses;
using EasySslStream.Connection.Full;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream
{
    
      public static class Program

      {

        static void Main()
        {



            Server srv = new Server();
            srv.CertificateCheckSettings.VerifyCertificateChain = false;
            srv.CertificateCheckSettings.VerifyCertificateName = false;
            srv.StartServer(IPAddress.Any, 5000, "cert.pfx", "123", false);
            
            
            //Thread.Sleep(1000000);
            
        }



        }

    }
