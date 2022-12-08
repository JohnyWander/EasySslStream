namespace CertGenTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            EasySslStream.DynamicConfiguration.CA_CONFIG.CountryCode = "US";

            EasySslStream.CertGenerationClasses.OpensslCertGeneration opensslCertGeneration = new EasySslStream.CertGenerationClasses.OpensslCertGeneration();
            opensslCertGeneration.GenerateCA("xd.txt");

            



        }
    }
} 