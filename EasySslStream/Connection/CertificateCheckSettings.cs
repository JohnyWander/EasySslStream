using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Connection
{
    /// <summary>
    /// Containts settings for certificate verification
    /// </summary>
    public class CertificateCheckSettings
    {
        public bool VerifyCertificateChain = true;
        public bool VerifyCertificateName = true;


      
    }
}
