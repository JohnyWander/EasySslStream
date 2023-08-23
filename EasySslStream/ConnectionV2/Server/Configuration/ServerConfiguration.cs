using EasySslStream.ConnectionV2.Server.Configuration.SubConfigTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.ConnectionV2.Server.Configuration
{
    public class ServerConfiguration
    {
        public ConnectionConfig connectionOptions;
        public CertfificateVerificationConfig authOptions;
        public SslProtocols enabledSSLProtocols = SslProtocols.None;
        public ServerConfiguration()
        {
            connectionOptions = new ConnectionConfig();
            authOptions = new CertfificateVerificationConfig();
            
        }

        private bool ValidadeClientCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch && authOptions.VerifyDomainName == false)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors && authOptions.VerifyCertificateChain == false)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                Console.WriteLine("CERT NOV AVAIABLE????");
                return false;
            }
            else
            {
                return false;
            }

        }
    }
}
