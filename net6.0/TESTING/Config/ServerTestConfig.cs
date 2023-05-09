using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.TESTING.Config
{
    public class ServerTestConfig
    {
        /// <summary>
        /// Listen on string representation of ip
        /// </summary>
        public string ListenOnIpString;

        /// <summary>
        /// Listen on ip represented by IPAddress class
        /// </summary>
        public IPAddress ListenOnIPAddress = IPAddress.Any;

        /// <summary>
        /// Port to listen on
        /// </summary>
        public int ListenPort;

        /// <summary>
        /// path to the pfx certificate, by default it uses cert obtained from Cert generation test class
        /// </summary>
        public string PathToDefaultServerCert = "cert.pfx";

        /// <summary>
        /// Password to the specified pfx certificate, by default it is password used for cert generation test class
        /// </summary>
        public string CertificatePassword = "123";

        /// <summary>
        /// By default server won't check client certificates
        /// </summary>
        public bool VerifyClients = false;

        /// <summary>
        /// Use only if VerifyClients is set to true. Checks provided by client certificate chain
        /// </summary>
        public bool VerifyCertificateChain = false;

        /// <summary>
        /// use only if VerifyClients is set to true. Checks if client certificate Common Name is correct
        /// </summary>
        public bool VerifyCertificateName = false;

    }
}
