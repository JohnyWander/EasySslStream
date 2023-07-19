using EasySslStream.Certgen.GenerationClasses.GenerationConfigs;
using EasySslStream.CertGenerationClasses;
using EasySslStream.Exceptions;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EasySslStreamTests
{
    public class OpenSslCetGenerationTests
    {
        private OpensslCertGeneration OpensslCertGen;
        private CaCertgenConfig CorrectCaCertgenConfig;
        private CaCertgenConfig InvalidCaCertgenConfig;

    


        [SetUp]
        public void Setup()
        {
            OpensslCertGen = new OpensslCertGeneration();
            CorrectCaCertgenConfig = new CaCertgenConfig();
            InvalidCaCertgenConfig = new CaCertgenConfig();

            CorrectCaCertgenConfig.KeyLength = CaCertgenConfig.KeyLengths.RSA_2048;
            CorrectCaCertgenConfig.HashAlgorithm = CaCertgenConfig.HashAlgorithms.sha256;
            CorrectCaCertgenConfig.CountryCode = "US";

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
            MethodInfo genInfo = typeof(OpensslCertGeneration).GetMethod("GenerateCaAsync");
            ParameterInfo[] param = genInfo.GetParameters();
            foreach (ParameterInfo p in param)
            {
                if (p.IsOptional)
                {
                    if(p.Position != 1)
                    {
                        Assert.That(File.Exists((string)p.DefaultValue));
                    }                                    
                }
            }
            
        }

        [Test, Order(4)]
        public async Task TestCaGenerationAsyncWithInvalidConfig()
        {
            Assert.Multiple(async () =>
            {
                Assert.ThrowsAsync<CountryCodeInvalidException>
                (
                    async () => { await OpensslCertGen.GenerateCaAsync(InvalidCaCertgenConfig); }
                );
                InvalidCaCertgenConfig.CountryCode = "US";
                CAconfigurationException CACe = Assert.ThrowsAsync<CAconfigurationException>
                (
                    async() => { await OpensslCertGen.GenerateCaAsync(InvalidCaCertgenConfig); }
                );
                Assert.That(CACe.Message.Equals("Hash algorithm is not set propertly in configuration class"));
                InvalidCaCertgenConfig.HashAlgorithm = CaCertgenConfig.HashAlgorithms.sha256;

                CACe = Assert.ThrowsAsync<CAconfigurationException>
                (
                    async () => { await OpensslCertGen.GenerateCaAsync(InvalidCaCertgenConfig); }
                );
                Assert.That(CACe.Message.Equals("Key length is not set correctly in configuration class"));
                InvalidCaCertgenConfig.KeyLength = CaCertgenConfig.KeyLengths.RSA_2048;
                
                await OpensslCertGen.GenerateCaAsync(InvalidCaCertgenConfig,"","CAFromInvalid.crt","CAFromInvalid.key");
            });
                

        }






    }
}