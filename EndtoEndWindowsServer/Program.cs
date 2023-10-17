using EasySslStream.ConnectionV2.Server;
using EndtoEndTestServer;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

using EasySslStream.CertGenerationClasses;
using EasySslStream.CertGenerationClasses.GenerationConfigs;
using EasySslStream;

namespace EndtoEndWindowsServer
{
    internal class Program
    {


        static private List<SrvParam> AvaiableParams = new List<SrvParam>()
        {
            new SrvParam("Help","h","-h /--help Prompts help message",(string x) => WriteHelpMessage(x)),
            new SrvParam("Server","s","-s /--Server Sets ip to listen on", (string ip) => server.IpToListenOn = ip),
            new SrvParam("Port","p","-p / --Port  Sets port to listen on", (string port) => server.PortToListenOn = int.Parse(port.Trim())),
            new SrvParam("Console","c","-c / use to expose sending console",(string just)=> server.ExposeSendingConsole()),
            new SrvParam("Recert","r","-r / --Recert - Generates new server certificate",(string name)=>GenerateNewServerCert(name))
        };
        static private List<SrvParam> ActivatedParams = new List<SrvParam>();

        static private HandleServer server = new HandleServer();


        static void Main(string[] args)
        {
#if DEBUG
            args = new string[] { "-h", "-s", "127.0.0.1", "-p", "2000","-c" };
#endif

            if (args.Length > 0)
            {
                ParseCommandLineArgs(args);
                LaunchActivatedParams();
                server.Launch();
            }
            else
            {
                args = new string[] { "-h" };
                ParseCommandLineArgs(args);
                LaunchActivatedParams();
            }

           
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

                    ActivatedParams.Add(new SrvParam(p.ParamName, value, p.ParamAction));
                }

            }
        }

        static void LaunchActivatedParams()
        {
            foreach(SrvParam activeparam in ActivatedParams)
            {
                activeparam.ParamAction.Invoke(activeparam.ParamValue);
            }
        }

        static void WriteHelpMessage(string n)
        {
            foreach(SrvParam param in AvaiableParams)
            {
                Console.WriteLine($"{param.ParamShortName}, {param.ParamHelpMessage}");
            }
        }

        
        static void GenerateNewServerCert(string filename)
        {
            OpensslCertGeneration certgen = new OpensslCertGeneration();

            CaCertgenConfig CA = new CaCertgenConfig();
            CA.HashAlgorithm = Config.HashAlgorithms.sha256;
            CA.KeyLength = Config.KeyLengths.RSA_2048;
            CA.CommonName = "TestCa.com";
            CA.CountryCode = "JP";
            certgen.GenerateCA(CA);


            CSRConfiguration CSR = new CSRConfiguration();
            CSR.HashAlgorithm = Config.HashAlgorithms.sha256;
            CSR.KeyLength = Config.KeyLengths.RSA_2048;
            CSR.alt_names.Add("server.com");
            CSR.alt_names.Add("s.server.com");
            CSR.CountryCode = "US";
            CSR.CommonName = "server.com";
            certgen.GenerateCSR(CSR);

            SignCSRConfig SIGN = new SignCSRConfig();
            SIGN.SetDefaultConfig(SignCSRConfig.DefaultConfigs.Server);

            certgen.SignCSR(SIGN, "CSR.csr", "CA.crt", "CA.key", "Server.crt");

            certgen.ConvertX509ToPfx("Server.crt", "CSR.key", "servercert.pfx", "");
        }
    }
}