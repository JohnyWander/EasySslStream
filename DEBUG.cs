using EasySslStream.CertGenerationClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream
{
    internal static class Program
    {
        public static void Main()
        {


            DynamicConfiguration.CA_CONFIG.HashAlgorithm = CA_CertGen.HashAlgorithms.sha256;
            DynamicConfiguration.CA_CONFIG.KeyLength = CA_CertGen.KeyLengths.RSA_1024;
            DynamicConfiguration.CA_CONFIG.Days = 356;
            DynamicConfiguration.CA_CONFIG.CountryCode = "US";
            DynamicConfiguration.CA_CONFIG.CountryState = "WLKP";
            DynamicConfiguration.CA_CONFIG.Location = "ĘĘĘĘĘĘĘĘĘĘ";
            DynamicConfiguration.CA_CONFIG.Organisation = "bppĘĘĘ";
            DynamicConfiguration.CA_CONFIG.CommonName = "test.domain.com";

            DynamicConfiguration.CA_CONFIG.Encoding = CA_CertGen.Encodings.UTF8;

            EasySslStream.CertGenerationClasses.OpensslCertGeneration opensslCertGeneration = new EasySslStream.CertGenerationClasses.OpensslCertGeneration();
            opensslCertGeneration.GenerateCA_Async();


        }


    }
}
