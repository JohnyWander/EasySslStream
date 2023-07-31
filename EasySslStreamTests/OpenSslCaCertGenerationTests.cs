using EasySslStream.Certgen.GenerationClasses.GenerationConfigs;
using EasySslStream.CertGenerationClasses;
using EasySslStream.Exceptions;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EasySslStreamTests
{
    public class OpenSslCaCertGenerationTests
    {
        private OpensslCertGeneration OpensslCertGen;
        private CaCertgenConfig CorrectCaCertgenConfig;
        private CaCertgenConfig InvalidCaCertgenConfig;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
          
        }


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

        [OneTimeTearDown] public void TearDown()
        {
            DirectoryInfo TestDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            
            foreach(FileInfo file in TestDir.EnumerateFiles("*")
                .Where(x => x.Name.Contains(".csr") || x.Name.Contains(".crt") || x.Name.Contains(".key")))
            {
               file.Delete();
            }
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
            ConfigurationException CACe = Assert.ThrowsAsync<ConfigurationException>
            (
                async () => { await OpensslCertGen.GenerateCaAsync(InvalidCaCertgenConfig); }
            );
            Assert.That(CACe.Message.Equals("Hash algorithm is not set propertly in configuration class"));
            InvalidCaCertgenConfig.HashAlgorithm = CaCertgenConfig.HashAlgorithms.sha256;

            CACe = Assert.ThrowsAsync<ConfigurationException>
            (
                async () => { await OpensslCertGen.GenerateCaAsync(InvalidCaCertgenConfig); }
            );
            Assert.That(CACe.Message.Equals("Key length is not set correctly in configuration class"));
            InvalidCaCertgenConfig.KeyLength = CaCertgenConfig.KeyLengths.RSA_2048;

            await OpensslCertGen.GenerateCaAsync(InvalidCaCertgenConfig, "", "CAFromInvalid.crt", "CAFromInvalid.key");
            Assert.That(() => File.Exists("CAFromInvalid.crt"));
            Assert.That(() => File.Exists("CAFromInvalid.key"));
            });
                

        }

        [Test,Order(5)]
        public void TestCaGenerationWithCorrectConfig()
        {
            OpensslCertGen.GenerateCA(this.CorrectCaCertgenConfig);
            MethodInfo genInfo = typeof(OpensslCertGeneration).GetMethod("GenerateCaAsync");
            ParameterInfo[] param = genInfo.GetParameters();
            foreach (ParameterInfo p in param)
            {
                if (p.IsOptional)
                {
                    if (p.Position != 1)
                    {
                        Assert.That(File.Exists((string)p.DefaultValue));
                    }
                }
            }

        }

        [Test,Order(6)]
        public void TestCaGenerationWithInvalidConfig()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<CountryCodeInvalidException>
                (
                    () => { OpensslCertGen.GenerateCaAsync(InvalidCaCertgenConfig); }
                );
                InvalidCaCertgenConfig.CountryCode = "US";
                ConfigurationException CACe = Assert.Throws<ConfigurationException>
                (
                    () => { OpensslCertGen.GenerateCA(InvalidCaCertgenConfig); }
                );
                Assert.That(CACe.Message.Equals("Hash algorithm is not set propertly in configuration class"));
                InvalidCaCertgenConfig.HashAlgorithm = CaCertgenConfig.HashAlgorithms.sha256;

                CACe = Assert.Throws<ConfigurationException>
                (
                    () => { OpensslCertGen.GenerateCA(InvalidCaCertgenConfig); }
                );
                Assert.That(CACe.Message.Equals("Key length is not set correctly in configuration class"));
                InvalidCaCertgenConfig.KeyLength = CaCertgenConfig.KeyLengths.RSA_2048;

                OpensslCertGen.GenerateCA(InvalidCaCertgenConfig, "", "CAFromInvalid.crt", "CAFromInvalid.key");
                Assert.That(() => File.Exists("CAFromInvalid.crt"));
                Assert.That(() => File.Exists("CAFromInvalid.key"));
            });

        }

        [Test, Order(7)]
        public void TestGenerationWithCustomPaths()
        {
            // Relative
            OpensslCertGen.GenerateCA(this.CorrectCaCertgenConfig, "CustomDirCA\\Sync", "CAcustomName.crt", "CAKeycustomName.key");
            Task.Run(async () =>{await OpensslCertGen.GenerateCaAsync(this.CorrectCaCertgenConfig, "CustomDirCA\\Async", "CAcustomName.crt", "CAKeycustomName.key"); });

            Assert.That(File.Exists("CustomDirCA\\Sync\\CAcustomName.crt"));
            Assert.That(File.Exists("CustomDirCA\\Sync\\CAKeycustomName.key"));

            Assert.That(File.Exists("CustomDirCA\\Async\\CAcustomName.crt"));
            Assert.That(File.Exists("CustomDirCA\\Sync\\CAKeycustomName.key"));

        }




    }
}