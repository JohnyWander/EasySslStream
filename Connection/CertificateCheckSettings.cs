using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Connection
{
    public class CertificateCheckSettings
    {
        public bool VerifyCertificateChain = true;
        public bool VerifyCertificateName = true;


        public CertificateCheckSettings()
        {
         
        }

    }
}
