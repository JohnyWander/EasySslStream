using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.ConnectionV2.Server.Configuration.SubConfigTypes
{
    public class ConnectionConfig
    {
        


        
        public SslProtocols enabledProtocols { get; set; }
        public ConnectionConfig()
        {
                      
        }
    }
}
