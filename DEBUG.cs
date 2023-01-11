using EasySslStream.CertGenerationClasses;
using EasySslStream.Connection.Full;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream
{

    public static class Program
    {

        static void Main()
        {
            DynamicConfiguration.EnableDebugMode(DynamicConfiguration.DEBUG_MODE.Console);

            DynamicConfiguration.CA_CONFIG.Encoding = CA_CertGen.Encodings.UTF8;
            DynamicConfiguration.CA_CONFIG.KeyLength = CA_CertGen.KeyLengths.RSA_2048;
            DynamicConfiguration.CA_CONFIG.HashAlgorithm = CA_CertGen.HashAlgorithms.sha256;
            DynamicConfiguration.CA_CONFIG.Location = "New York";
            DynamicConfiguration.CA_CONFIG.Organisation = "White Hause";
            DynamicConfiguration.CA_CONFIG.CommonName = "gfvgv.com";
            DynamicConfiguration.CA_CONFIG.CountryCode = "US";
            DynamicConfiguration.CA_CONFIG.CountryState = "Florida";
            DynamicConfiguration.CA_CONFIG.Days = 365;

            CertGenerationClasses.OpensslCertGeneration gen = new OpensslCertGeneration();
           // gen.GenerateCA();


            CSRConfiguration conf = new CSRConfiguration();
            conf.Organization = "ME";
            conf.KeyLength = CSRConfiguration.KeyLengths.RSA_2048;
            conf.CommonName = "mypc.com";
            conf.alt_names.Add("mypc.com");
            conf.alt_names.Add("xd.mypc.com");
            conf.State = "Florida";
            conf.City = "Texas";
            conf.CountryCode = "US";
            conf.Encoding = CSRConfiguration.Encodings.UTF8;
            conf.HashAlgorithm = CSRConfiguration.HashAlgorithms.sha256;


            //gen.GenerateCSR(conf);

            SignCSRConfig signconf = new SignCSRConfig();
            signconf.copyallextensions = true;
            signconf.SetAuthorityKeyIdentifiers(SignCSRConfig.authorityKeyIdentifiers.keyid_and_issuer);
            signconf.SetBasicConstrainsList(SignCSRConfig.basicConstrains.CAFalse);
            signconf.SetExtendedKeyUsage(new SignCSRConfig.ExtendedKeyUsage[] {
            SignCSRConfig.ExtendedKeyUsage.clientAuth,
            SignCSRConfig.ExtendedKeyUsage.serverAuth
            });
            signconf.days = 365;


            //gen.SignCSR(signconf,"certificate.csr","CA.crt","CA.key","cert.crt");

            //gen.ConvertX509ToPfx("cert.crt", "certificate.csr.key", "cert.pfx", "123");


            DynamicConfiguration.TransportBufferSize = 4096;

          
                Server server = new Server();
                server.CertificateCheckSettings.VerifyCertificateName = false;
                server.CertificateCheckSettings.VerifyCertificateChain = false;

         

                server.StartServer(IPAddress.Any, 10000, "pfxcert.pfx.pfx", "231", false);

           
                 Thread.Sleep(10000);

            server.TestList();
          
            

        }



    }
     
}
