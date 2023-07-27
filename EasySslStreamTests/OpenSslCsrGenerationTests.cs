using EasySslStream.CertGenerationClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySslStream.CertGenerationClasses.GenerationConfigs;
namespace EasySslStreamTests
{
    internal class OpenSslCsrGenerationTests
    {
        OpensslCertGeneration csrgen;
        CSRConfiguration ValidCsrConf;
        CSRConfiguration InvalidCsrConf;
        
        private string Workspace ="CSRworkspace";

        
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
                file.Delete();
            }
        }


        [Test]
        public void TestGenerateCSRCorrectConfig()
        {
            csrgen.GenerateCSR(ValidCsrConf);
            MethodInfo inf = csrgen.GetType().GetMethod("GenerateCSR");
            ParameterInfo[] parameters = inf.GetParameters();
            foreach(ParameterInfo param in parameters)
            {
                if (param.IsOptional)
                {
                    if(param.Position != 1)
                    {
                        Assert.That(File.Exists((string)param.DefaultValue),$"Not Found output file - {param.DefaultValue}");
                    }
                }
            }
        }

        [Test]
        public async Task TestGenerateCSRAsyncCorrectConfig()
        {
            await csrgen.GenerateCSRAsync(ValidCsrConf);
            MethodInfo inf = csrgen.GetType().GetMethod("GenerateCSR");
            ParameterInfo[] parameters = inf.GetParameters();
            foreach (ParameterInfo param in parameters)
            {
                if (param.IsOptional)
                {
                    if (param.Position != 1)
                    {
                        Assert.That(File.Exists((string)param.DefaultValue), $"Not Found output file - {param.DefaultValue}");
                    }
                }
            }
        }




    }
}
