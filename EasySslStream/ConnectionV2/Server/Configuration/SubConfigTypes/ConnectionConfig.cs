using System.Security.Authentication;

namespace EasySslStream.ConnectionV2.Server.Configuration.SubConfigTypes
{
    public class ConnectionConfig
    {
        /// <summary>
        /// Ssl protocols enabled for connection - only tls 1.2 is enabled by default
        /// </summary>
        public SslProtocols enabledProtocols { get; set; }

        /// <summary>
        /// It true, server will try to verify client certificates. False by default.
        /// </summary>
        public bool VerifyClientCertificates;
        /// <summary>
        /// If true, and server is set to verify client certificates - Server will check if client certificate common name matches client dns name. True by default
        /// </summary>
        public bool VerifyDomainName;

        /// <summary>
        /// if true and server is set to verify client certificates - Server will check if client certificate is signed by trusted CA. True by default
        /// </summary>
        public bool VerifyCertificateChain;
            
                  
        public ConnectionConfig()
        {
            enabledProtocols = SslProtocols.Tls12;
            this.VerifyClientCertificates = false;
            this.VerifyDomainName = true;
            this.VerifyCertificateChain = true;
            

        }
    }
}
