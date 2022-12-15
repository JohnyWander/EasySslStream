using EasySslStream;
namespace CertGenTest
{
    internal class Program
    {
        static void Main(string[] args)
        {

            DynamicConfiguration.EnableDebugMode(DynamicConfiguration.DEBUG_MODE.Console);

         
            DynamicConfiguration.CA_CONFIG.HashAlgorithm = CA_CertGen.HashAlgorithms.sha256;
            DynamicConfiguration.CA_CONFIG.KeyLength = CA_CertGen.KeyLengths.RSA_1024;
            DynamicConfiguration.CA_CONFIG.Days = 365;
            DynamicConfiguration.CA_CONFIG.CountryCode = "US";
            DynamicConfiguration.CA_CONFIG.CountryState = "WLKP";
            DynamicConfiguration.CA_CONFIG.Location = "ĘĘĘĘĘĘĘĘĘĘ";
            DynamicConfiguration.CA_CONFIG.Organisation = "bppĘĘĘ";
            DynamicConfiguration.CA_CONFIG.CommonName = "test.domain.com";

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



            var conf = new ClientCSRConfiguration();

            conf.CSRFileName = "cert.csr";
            conf.HashAlgorithm = ClientCSRConfiguration.HashAlgorithms.sha256;
            conf.KeyLength = ClientCSRConfiguration.KeyLengths.RSA_2048;
            conf.Encoding = ClientCSRConfiguration.Encodings.UTF8;
            conf.CountryCode = "US";
            conf.State = "ĘĘĘĘ";
            conf.City = "ééé";
            conf.Organization = "Ó Ó Ó";
            conf.CommonName = "test.domain.com";

            // opensslCertGeneration.GenerateCSR(conf,"CA");


            tasklist.Clear();
            
            Task.Run(async () =>
            {

                tasklist.Add(Task.Run(() => dummytask()));
                tasklist.Add(Task.Run(() => opensslCertGeneration.GenerateCSRAsync(conf,"CA")));



                await Task.WhenAll(tasklist);


            }).GetAwaiter().GetResult();
            

        }


        public static async Task dummytask()
        {

            Console.WriteLine("dummy task start");
            await Task.Delay(2000);
            Console.WriteLine("dummy task end");

            
        }

    }
} 