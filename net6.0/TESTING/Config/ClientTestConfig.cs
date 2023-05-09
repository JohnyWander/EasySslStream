using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.TESTING.Config
{
    internal class ClientTestConfig
    {
        /// <summary>
        /// Connect to server with this representation of ip
        /// </summary>
        public string ServerIPstring; 
        public IPAddress ServerIPAddress = IPAddress.Any;
        public int ServerPort;

        /// <summary>
        /// Determines if server certificate should be checked.<br></br>
        /// It won't be by default
        /// </summary>
        public bool VerifyServer = false;

        /// <summary>
        /// Use only if VerifyClients is set to true. Checks server certificate chain
        /// </summary>
        public bool VerifyCertificateChain = false;

        /// <summary>
        /// use only if VerifyClients is set to true. Checks if server certificate Common Name is correct
        /// </summary>
        public bool VerifyCertificateName = false;


        /// <summary>
        /// Use only if server verifies client cert. - Path to the pfx certificate, by default it uses cert obtained from Cert generation test class
        /// </summary>
        public string PathToDefaultServerCert;

        /// <summary>
        /// Use only if server verifies client cert. - Password to the specified pfx certificate, by default it is password used for cert generation test class
        /// </summary>
        public string CertificatePassword;




    }
}
