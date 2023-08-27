using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;

namespace EasySslStream.ConnectionV2.Client.Configuration
{
    public class ClientConfiguration
    {
        public int BufferSize = 4096;
        public bool verifyDomainName = false;
        public bool verifyCertificateChain = false;
        

        public bool serverVerifiesClient;
        public string pathToClientPfxCertificate;
        public string certificatePassword;

        public SslProtocols enabledSSLProtocols = SslProtocols.None;
        public ClientConfiguration()
        {

        }


        internal bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch && verifyDomainName == false)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors && verifyCertificateChain == false)
            {
                return true;

            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                //TODO: use more specific exception
                return false;
            }
            else
            {

                if (verifyCertificateChain == false && verifyDomainName == false)
                {
                    return true;
                }
                return false;
            }
        }

    }
}
