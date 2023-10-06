using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    internal class cmdarg
    {
        public string ArgName;
        public string ArgValue;
        public string ArgHelpMessage;
        public Action<string> ArgAction;

        public bool IsRequired = false;

        public cmdarg(string Argname, string ArgHelpMessage, Action<string> ArgAction) // for list
        {
            this.ArgName = Argname;
            this.ArgHelpMessage = ArgHelpMessage;
            this.ArgAction = ArgAction;
        }

        public cmdarg(string Argname, string ArgHelpMessage, string ArgValue, Action<string> ArgAction) // for enabled
        {
            this.ArgName = Argname;
            this.ArgHelpMessage = ArgHelpMessage;
            this.ArgValue = ArgValue;
            this.ArgAction = ArgAction;

        }

        public void InvokeParamAction()
        {
            this.ArgAction.Invoke(this.ArgValue);
        }

        public cmdarg SetRequired()
        {
            IsRequired = true;
            return this;
        }
    }
}
