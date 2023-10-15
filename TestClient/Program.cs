using EasySslStream;
using System.Runtime.CompilerServices;

namespace TestClient
{
    internal class Program
    {
        static List<cmdarg> ActivatedCmdArgs = new List<cmdarg>();

        static List<cmdarg> AvaiableCmdArgs = new List<cmdarg>()
        {
            new cmdarg("-fd","-fd - File save directory path, if not set file will be saved in program directory",(string fdpath) => client.FileSavePath = fdpath),
            new cmdarg("-dd","-dd - Directory save directory path, if not set directory will be saved",(string ddpath) => client.DirectorySavePath = ddpath),
            new cmdarg("-s","-s - Connect to  - server:port",(string cto) => client.ConnectTo = cto).SetRequired(),
            new cmdarg("-c","-c - Client Certificate - use if server verifies client certs",(string cpath) => client.CertificatePath = cpath),
            new cmdarg("-c","-cp - Password to client certificate - use if you provided certificate for connection", (string cpass) => client.CertificatePassword = cpass),
            new cmdarg("-vc","-vc - Verify certificate chain - use if client should verify chain of server certificate",(string _)=> client.VerifyChain = true),
            new cmdarg("-vn","-vn - Verify certificate name - use if client should verify server name with one in provided certificate",(string _)=> client.VerifyCN = true),
            new cmdarg("-h","-h - Displays help message",(string _) => DisplayHelp())


        };

        public static ClientHandler client = new ClientHandler();


        static void Main(string[] args)
        {
#if DEBUG
            args = new string[] { "-s", "127.0.0.1:2000" };

#endif

            if (args.Length > 0)
            {
                ParseCommandLineArgs2(args);
                CheckForRequiredArgs();
                InvokeParamSetters();
                
                client.StartClient();

                try
                {
                    client.client.RunningClient.Wait();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Connection failed with error - {e.GetType().Name} {e.Message}");
                    Console.ResetColor();
                }
            }
            else
            {
                DisplayHelp();
            }



        }

        static void InvokeParamSetters()
        {
            foreach (cmdarg p in ActivatedCmdArgs)
            {
                p.ArgAction.Invoke(p.ArgValue);
            }
        }
        static void DisplayHelp()
        {
            AvaiableCmdArgs.ForEach(cmdarg => Console.WriteLine(cmdarg.ArgHelpMessage));
        }

        static void ParseCommandLineArgs2(string[] args)
        {
            cmdarg ToActivate = null;
            bool AlreadyAssignedValue = false;

            foreach (string arg in args)
            {
                try
                {                    
                    cmdarg PickedArg = AvaiableCmdArgs.Where(x => x.ArgName == arg).Single();
                    ToActivate = new cmdarg(PickedArg.ArgName, PickedArg.ArgHelpMessage, null, PickedArg.ArgAction);
                    ActivatedCmdArgs.Add(ToActivate);
                    AlreadyAssignedValue = false;
                }
                catch (InvalidOperationException)
                {
                    if (AlreadyAssignedValue)
                    {
                        throw;// TODO: present error to user
                    }
                    else
                    {
                        ToActivate.ArgValue = arg;
                        AlreadyAssignedValue = true;
                    }
                }

            }
        }


        static void CheckForRequiredArgs()
        {
            List<cmdarg> RequiredArgs = AvaiableCmdArgs.Where(x => x.IsRequired == true).ToList();
            List<cmdarg> Activated = ActivatedCmdArgs;
            Activated.ForEach(active =>
            {
                IEnumerable<cmdarg> marked = RequiredArgs.Where(x => x.ArgName == active.ArgName);
                if (marked.Count() == 1)
                {
                    RequiredArgs.Remove(marked.Single());
                }
            });

            if (RequiredArgs.Count != 0)
            {
                RequiredArgs.ForEach(r => Console.WriteLine($"[ERROR] Missing required argument - {r.ArgHelpMessage}"));
                
            }
        }


    }
} 
    
