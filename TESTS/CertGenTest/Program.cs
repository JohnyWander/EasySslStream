using EasySslStream;
using EasySslStream.CertGenerationClasses;
using System.Security.Cryptography.X509Certificates;

namespace CertGenTest
{
    internal class Program
    {
        static void Main(string[] args)
        {

            DynamicConfiguration.EnableDebugMode(DynamicConfiguration.DEBUG_MODE.Console);

         
            DynamicConfiguration.CA_CONFIG.HashAlgorithm = CA_CertGen.HashAlgorithms.sha256;
            DynamicConfiguration.CA_CONFIG.KeyLength = CA_CertGen.KeyLengths.RSA_2048;
            DynamicConfiguration.CA_CONFIG.Days = 365;
            DynamicConfiguration.CA_CONFIG.CountryCode = "US";
            DynamicConfiguration.CA_CONFIG.CountryState = "WLKP";
            DynamicConfiguration.CA_CONFIG.Location = "ĘĘĘĘĘĘĘĘĘĘ";
            DynamicConfiguration.CA_CONFIG.Organisation = "bppĘĘĘ";
            DynamicConfiguration.CA_CONFIG.CommonName = "signercertdefaultdebub.com";

            DynamicConfiguration.CA_CONFIG.Encoding = CA_CertGen.Encodings.UTF8;

//DynamicConfiguration.CA_CONFIG.

            EasySslStream.CertGenerationClasses.OpensslCertGeneration opensslCertGeneration = new EasySslStream.CertGenerationClasses.OpensslCertGeneration();
             /// opensslCertGeneration.GenerateCA("CA"); synchronouse version

            List<Task> tasklist = new List<Task>();

            Task.Run(async () =>
            {

                tasklist.Add(Task.Run(() => dummytask()));
                tasklist.Add(Task.Run(() => opensslCertGeneration.GenerateCA_Async("CA")));



                await Task.WhenAll(tasklist);

         
            }).GetAwaiter().GetResult();



            var conf = new CSRConfiguration();

            conf.CSRFileName = "cert.csr";
            conf.HashAlgorithm = CSRConfiguration.HashAlgorithms.sha256;
            conf.KeyLength = CSRConfiguration.KeyLengths.RSA_2048;
            conf.Encoding = CSRConfiguration.Encodings.UTF8;
            conf.CountryCode = "US";
            conf.State = "ĘĘĘdsdĘ";
            conf.City = "ééésds";
            conf.Organization = "ÓsssÓÓ";
            conf.CommonName = "certtt.com";
            conf.alt_names.Add("Xl.com");
            // opensslCertGeneration.GenerateCSR(conf,"CA");


            tasklist.Clear();
            
            Task.Run(async () =>
            {

                tasklist.Add(Task.Run(() => dummytask()));
                tasklist.Add(Task.Run(() => opensslCertGeneration.GenerateCSRAsync(conf,"CA")));



                await Task.WhenAll(tasklist);


            }).GetAwaiter().GetResult();


            var config = new SignCSRConfig();
            config.SetDefaultConfig(SignCSRConfig.DefaultConfigs.Enduser);
            config.AddAltName(SignCSRConfig.AltNames.DNS, "X.com");
            opensslCertGeneration.SignCSR(config, "cert.csr","CA.crt","CA.key","certificate_SYNC.crt","CA");


           Task.Run(async () =>
            {
                tasklist.Add(Task.Run(() => dummytask()));
                tasklist.Add(Task.Run(() => opensslCertGeneration.SignCSRAsync(config, "cert.csr", "CA.crt", "CA.key", "certificate_ASYNC.crt", "CA")));

                await Task.WhenAll(tasklist);

            }).GetAwaiter().GetResult();




            opensslCertGeneration.ConvertX509ToPfx("certificate_SYNC.crt", "cert.csr.key","pfxcert.pfx","231","CA");

        }


        

        public static async Task dummytask()
        {

            Console.WriteLine("dummy task start");
            await Task.Delay(2000);
            Console.WriteLine("dummy task end");

            
        }

    }
} 