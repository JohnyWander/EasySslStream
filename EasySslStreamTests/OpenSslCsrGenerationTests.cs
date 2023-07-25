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

        [OneTimeSetUp] public void OneTimeSetUp()
        {
            if (Directory.Exists(Workspace))
            {
                Directory.SetCurrentDirectory(Workspace);
            }
            else
            {
                Directory.CreateDirectory(Workspace);
                Directory.SetCurrentDirectory(Workspace);
            }

        }

        [SetUp]
        public void Setup()
        {
            csrgen = new OpensslCertGeneration();

            InvalidCsrConf = new CSRConfiguration();
            ValidCsrConf = new CSRConfiguration();

            ValidCsrConf.HashAlgorithm = CSRConfiguration.HashAlgorithms.sha256;
            ValidCsrConf.KeyLength = CSRConfiguration.KeyLengths.RSA_2048;
            
        }

        [Test]
        public void GenerateCSRsyncCorrectConfig()
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




    }
}
