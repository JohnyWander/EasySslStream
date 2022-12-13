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
            DynamicConfiguration.CA_CONFIG.Days = 356;
            DynamicConfiguration.CA_CONFIG.CountryCode = "US";
            DynamicConfiguration.CA_CONFIG.CountryState = "WLKP";
            DynamicConfiguration.CA_CONFIG.Location = "Poznań";
            DynamicConfiguration.CA_CONFIG.Organisation = "bppĘĘĘ";
            DynamicConfiguration.CA_CONFIG.CommonName = "test.domain.pl";

            DynamicConfiguration.CA_CONFIG.Encoding = CA_CertGen.Encodings.UTF8;

//DynamicConfiguration.CA_CONFIG.

            EasySslStream.CertGenerationClasses.OpensslCertGeneration opensslCertGeneration = new EasySslStream.CertGenerationClasses.OpensslCertGeneration();
            opensslCertGeneration.GenerateCA();

            



        }
    }
} 