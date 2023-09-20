using EasySslStream.ConnectionV2.Server;
using EndtoEndTestServer;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace EndtoEndWindowsServer
{
    internal class Program
    {
       
        
        static private List<SrvParam> AvaiableParams = new List<SrvParam>()
        {
            new SrvParam("Server","s","-s /--Server Sets ip to listen on", (string ip) => SetIp(ip)),
            new SrvParam("Port","p","-p / --Port  Sets port to listen on", (string port) => SetPort(port)),
            new SrvParam("JustListen","j","-j / --JustListen true/ false -  Sets server to just listen to show debu messages",)
        };
        static private List<SrvParam> ActivatedParams;

        


        static void Main(string[] args)
        {

            ParseCommandLineArgs(args);

            Console.ReadKey();
        }

        static void ParseCommandLineArgs(string[] args)
        {

            StringBuilder fullargs = new StringBuilder();
            foreach (string arg in args)
            {
                if (arg.Contains("--"))
                {
                    arg.Replace("--", "-");
                }

                fullargs.Append(arg + " ");
            }

          
            string fullargsstring = fullargs.ToString();

            string[] splittedArgPairs = fullargsstring.Split('-',StringSplitOptions.RemoveEmptyEntries);

            foreach(string s in splittedArgPairs)
            {
                string pName;
                string value;
                string[] split = s.Split(" ");
                pName = split[0];
                value = split[1];

                IEnumerable<SrvParam> pickedParam;                
                if(pName.Length > 1)
                {
                    pickedParam = AvaiableParams.Where(arg => arg.ParamName == pName);
                }
                else
                {
                    pickedParam = AvaiableParams.Where(arg => arg.ParamShortName == pName);
                }

                if(pickedParam.Count() == 1)
                {
                    SrvParam p = pickedParam.ToArray()[0];
                    p.ParamValue = value;
                    Console.WriteLine(p.ParamName);
                    Console.WriteLine(p.ParamValue);
                }

            }



        }


        static void SetIp(string ip)
        {

        }

        static void SetPort(string port)
        {

        }

        static void SetJustListen(string )
        {

        }

    }
}