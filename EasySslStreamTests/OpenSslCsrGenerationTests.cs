using EasySslStream.CertGenerationClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySslStream.CertGenerationClasses.GenerationConfigs;
using System.Runtime.CompilerServices;

namespace EasySslStreamTests
{
    internal class OpenSslCsrGenerationTests
    {
        OpensslCertGeneration csrgen;
        CSRConfiguration ValidCsrConf;
        CSRConfiguration InvalidCsrConf;

        private string Workspace = "CSRworkspace";


        private string DefaultCSRPath;
        private string DefaultKeyPath;

        private string DefaultAsyncCSRPath;
        private string DefaultAsyncKeyPath;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Type type = typeof(OpensslCertGeneration);
            MethodInfo SyncGenMethod = type.GetMethod("GenerateCSR");
            MethodInfo AsyncGenMethod = type.GetMethod("GenerateCSRAsync");

            ParameterInfo[] SyncParams = SyncGenMethod.GetParameters();
            ParameterInfo[] AsyncParams = AsyncGenMethod.GetParameters();

            DefaultCSRPath = SyncParams[2].DefaultValue.ToString();
            DefaultKeyPath = SyncParams[3].DefaultValue.ToString();

            DefaultAsyncCSRPath = AsyncParams[2].DefaultValue.ToString();
            DefaultAsyncKeyPath = AsyncParams[3].DefaultValue.ToString();
        }

        

        
        [SetUp]
        public void Setup()
        {
            csrgen = new OpensslCertGeneration();

            InvalidCsrConf = new CSRConfiguration();
            ValidCsrConf = new CSRConfiguration();

            ValidCsrConf.HashAlgorithm = CSRConfiguration.HashAlgorithms.sha256;
            ValidCsrConf.KeyLength = CSRConfiguration.KeyLengths.RSA_2048;
            ValidCsrConf.CountryCode = "US";
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            DirectoryInfo TestDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            foreach (FileInfo file in TestDir.EnumerateFiles("*")
                .Where(x => x.Name.Contains(".csr") || x.Name.Contains(".crt") || x.Name.Contains(".key")))
            {
                //file.Delete();
            }
        }


        [Test]
        public void TestGenerateCSRCorrectConfig()
        {
            csrgen.GenerateCSR(ValidCsrConf);
            Assert.That(File.Exists(DefaultCSRPath));      
            Assert.That(File.Exists(DefaultKeyPath));

        }

        [Test]
        public async Task TestGenerateCSRAsyncCorrectConfig()
        {
            await csrgen.GenerateCSRAsync(ValidCsrConf);
            Assert.That(File.Exists(DefaultAsyncCSRPath),$"Not found file {DefaultAsyncCSRPath}");
            Assert.That(File.Exists(DefaultAsyncKeyPath),$"Not found file {DefaultAsyncKeyPath}");
        }




    }
}
