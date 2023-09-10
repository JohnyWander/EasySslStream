using System.Security.Authentication;

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
