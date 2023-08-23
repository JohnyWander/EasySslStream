using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.ConnectionV2.Server.Configuration.SubConfigTypes
{
    public class CertfificateVerificationConfig
    {
        public bool VerifyDomainName;
        public bool VerifyCertificateChain;
        public bool VerifyClientCertificates;

        public CertfificateVerificationConfig()
        {
            this.VerifyDomainName = false;
            this.VerifyCertificateChain = false;
            this.VerifyClientCertificates = false;
        }

    }
}
