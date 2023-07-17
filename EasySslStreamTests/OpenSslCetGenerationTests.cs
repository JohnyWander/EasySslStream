using EasySslStream.Certgen.GenerationClasses.GenerationConfigs;
using EasySslStream.CertGenerationClasses;
using EasySslStream.Exceptions;
using System.Diagnostics;

namespace EasySslStreamTests
{
    public class OpenSslCetGenerationTests
    {
        private OpensslCertGeneration OpensslCertGen;
        private CaCertgenConfig CorrectCaCertgenConfig;
        private CaCertgenConfig InvalidCaCertgenConfig;

        [OneTimeSetUp]
        public void Setup()
        {
            OpensslCertGen = new OpensslCertGeneration();
          
        }



        [Test,Order(1)]
        public void TestValidCaConfiguration()
        {
            CorrectCaCertgenConfig= new CaCertgenConfig();
            CorrectCaCertgenConfig.HashAlgorithm = CaCertgenConfig.HashAlgorithms.sha256;
            CorrectCaCertgenConfig.KeyLength = CaCertgenConfig.KeyLengths.RSA_2048;
            Assert.DoesNotThrow(() => { CorrectCaCertgenConfig.CountryCode = "US"; });
        }

        [Test,Order(2)]
        public void TestInvalidCaConfiguration()
        {
            Debug.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
            InvalidCaCertgenConfig = new CaCertgenConfig();            
            Assert.Throws<CountryCodeInvalidException>(() => InvalidCaCertgenConfig.CountryCode = "ewew");
            


        }

        [Test,Order(3)]
        public async Task TestCaGenerationAsyncWithCorrectConfig()
        {
            await OpensslCertGen.GenerateCaAsync(this.CorrectCaCertgenConfig);
        }




    }
}