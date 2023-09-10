using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;


namespace EasySslStream.ConnectionV2.Client.Configuration
{
    public class ClientConfiguration
    {
        public int BufferSize = 8192;
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
