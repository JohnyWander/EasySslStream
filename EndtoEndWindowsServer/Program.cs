using EasySslStream.ConnectionV2.Server;
using System.Collections.Generic;

namespace EndtoEndWindowsServer
{
    internal class Program
    {
        private struct SrvParam
        {
            public string ParamName;
            public string ParamShortName;
            public string ParamHelpMessage;

            public string ParamValue;
            public Action ParamAction;
        }

        private List<SrvParam> AvaiableParams;
        private List<SrvParam> ActivatedParams;


        static void Main(string[] args)
        {
            
            

        }
    }
}