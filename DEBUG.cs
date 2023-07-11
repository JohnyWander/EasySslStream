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



            TESTING.ServerSide serverTest = new TESTING.ServerSide();
           
            TESTING.Config.ServerTestConfig testconfig = new TESTING.Config.ServerTestConfig();
            testconfig.ListenPort = 5000;
            testconfig.ListenOnIpString = "127.0.0.1";
            serverTest.LaunchTestServer(testconfig);
            serverTest.StartTest(new TESTING.Config.ServerTestObjects() , ConnectionCommons.Unit.MBs, 1);



            //Thread.Sleep(1000000);
            
        }



        }

    }
