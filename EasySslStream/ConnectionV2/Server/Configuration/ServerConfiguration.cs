using EasySslStream.ConnectionV2.Server.Configuration.SubConfigTypes;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace EasySslStream.ConnectionV2.Server.Configuration
{
    public class ServerConfiguration
    {

        public ConnectionConfig connectionOptions { get; set; } 
       

        /// <summary>
        /// Default buffer size for transport buffer - 8192 by default - Fast and stable
        /// </summary>
        public int BufferSize { get; set; } = 8192;


        public ServerConfiguration()
        {
            connectionOptions = new ConnectionConfig();           
        }

        internal bool ValidadeClientCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch && connectionOptions.VerifyDomainName == false)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors && connectionOptions.VerifyCertificateChain == false)
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
