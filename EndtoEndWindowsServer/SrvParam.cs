using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndtoEndTestServer
{
    public class SrvParam
    {
        public string ParamName;
        public string ParamShortName;
        public string ParamHelpMessage;

        public string ParamValue = "";
        public Action<string> ParamAction;

        public SrvParam(string ParamName, string ParamShortName, string ParamHelpMessage, Action<string> paramAction)
        {
            this.ParamName = ParamName;
            this.ParamShortName = ParamShortName;
            this.ParamHelpMessage = ParamHelpMessage;

            this.ParamAction = paramAction;
        }

    }
}
