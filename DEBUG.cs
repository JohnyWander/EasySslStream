using EasySslStream.CertGenerationClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream
{
#if DEBUG
    internal static class Program
    {
        public static void Main()
        {
            
            DynamicConfiguration.EnableDebugMode(DynamicConfiguration.DEBUG_MODE.Console);

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
            opensslCertGeneration.GenerateCA_Async("CA");
            var conf = new CSRConfiguration();
            List<Task> tasklist = new List<Task>();

            conf.CSRFileName = "cert.csr";
            conf.HashAlgorithm = CSRConfiguration.HashAlgorithms.sha256;
            conf.KeyLength = CSRConfiguration.KeyLengths.RSA_2048;
            conf.Encoding = CSRConfiguration.Encodings.UTF8;
            conf.CountryCode = "US";
            conf.State = "hhhĘdsdhhh";
            conf.City = "héésds";
            conf.Organization = "ÓsssÓhh";
            conf.CommonName = "certtt.com";
            conf.alt_names.Add("wp.com");
             opensslCertGeneration.GenerateCSR(conf,"CA");


        


        }
        public static async Task dummytask()
        {

            Console.WriteLine("dummy task start");
            await Task.Delay(2000);
            Console.WriteLine("dummy task end");


        }

    }
#endif
}
