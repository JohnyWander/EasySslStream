using EasySslStream;
using EasySslStream.Certgen.GenerationClasses.GenerationConfigs;
using EasySslStream.CertGenerationClasses;
using EasySslStream.CertGenerationClasses.GenerationConfigs;
using EasySslStream.Exceptions;

namespace EasySslStreamTests.CertgenTests
{
    public class OpenSslSignCSRAndPFXTests
    {
        OpensslCertGeneration certgen = new OpensslCertGeneration();

        SignCSRConfig CorrectSingConfig = new SignCSRConfig();
        SignCSRConfig IncorrectSignConfig = new SignCSRConfig();

        string SignCsrWorkspace = "SignWorkspace";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Directory.CreateDirectory(SignCsrWorkspace);

            CaCertgenConfig caCertgenConfig = new CaCertgenConfig();
            caCertgenConfig.KeyLength = Config.KeyLengths.RSA_4096;
            caCertgenConfig.HashAlgorithm = Config.HashAlgorithms.sha256;
            caCertgenConfig.CommonName = "TestCA";
            caCertgenConfig.Days = 128;
            caCertgenConfig.Organization = "TESTCAORG";
            caCertgenConfig.State = "Iowa";
            caCertgenConfig.CountryCode = "JP";

            certgen.GenerateCA(caCertgenConfig, SignCsrWorkspace, "TestCA.crt", "TestCA.key");

            CSRConfiguration csrgenConfig = new CSRConfiguration();
            csrgenConfig.KeyLength = Config.KeyLengths.RSA_4096;
            csrgenConfig.HashAlgorithm = Config.HashAlgorithms.sha384;
            csrgenConfig.CommonName = "testdomain.com";
            csrgenConfig.City = "Vienna";
            csrgenConfig.CountryState = "Westchestershire";
            csrgenConfig.alt_names.Add("*.testdomain.com");
            csrgenConfig.CountryCode = "US";
            certgen.GenerateCSR(csrgenConfig, SignCsrWorkspace, "TestCSR.csr", "TestKEY.key");

        }

        [SetUp]
        public void SetUp()
        {
            certgen = new OpensslCertGeneration();
            CorrectSingConfig = new SignCSRConfig();
            IncorrectSignConfig = new SignCSRConfig();
            CorrectSingConfig.SetDefaultConfig(SignCSRConfig.DefaultConfigs.Server);
        }

        [Test, Order(1)]
        public void TestSignWithcorrectConfig()
        {
            SignCSRConfig conf = new SignCSRConfig();
            conf.SetDefaultConfig(SignCSRConfig.DefaultConfigs.Server);

            certgen.SignCSR(conf, "TestCSR.csr", "TestCA.crt", "TestCA.key", "TestCertificate.crt", SignCsrWorkspace);
            Assert.That(File.Exists($"{SignCsrWorkspace}\\TestCertificate.crt"));
        }

        [Test, Order(2)]
        public void TestSignWithIncorrectConfig()
        {
            Assert.Multiple(() =>
            {
                Assert.Throws<SignCsrException>(() => certgen.SignCSR(this.IncorrectSignConfig, "TestCSR.csr", "TestCA.crt", "TestCA.key", "TestCertificateFromIncorrect.crt", SignCsrWorkspace));
                IncorrectSignConfig.days = 365;

                certgen.SignCSR(IncorrectSignConfig, "TestCSR.csr", "TestCA.crt", "TestCA.key", "TestCertificateFromIncorrect.crt", SignCsrWorkspace);
                Assert.That(File.Exists($"{SignCsrWorkspace}\\TestCertificateFromIncorrect.crt"));
            });
        }

        [Test, Order(3)]
        public void TestConvertToPfx()
        {
            certgen.ConvertX509ToPfx("TestCertificate.crt", "TestKEY.key", "TestPFX.pfx", "", SignCsrWorkspace);
            Assert.That(File.Exists(SignCsrWorkspace + "\\TestPFX.pfx"));
        }
        // Async block

        [Test, Order(4)]
        public async Task TestSignAsyncWithCorrectConfig()
        {
            SignCSRConfig conf = new SignCSRConfig();
            conf.SetDefaultConfig(SignCSRConfig.DefaultConfigs.Server);

            await certgen.SignCSRAsync(conf, "TestCSR.csr", "TestCA.crt", "TestCA.key", "TestCertificateAsync.crt", SignCsrWorkspace);
            Assert.That(File.Exists($"{SignCsrWorkspace}\\TestCertificateAsync.crt"));
        }

        [Test, Order(5)]
        public async Task TestSignAsyncWithIncorrectConfig()
        {
            Assert.Multiple(async () =>
            {
                Assert.Throws<SignCsrException>(() => certgen.SignCSR(this.IncorrectSignConfig, "TestCSR.csr", "TestCA.crt", "TestCA.key", "TestCertificateFromIncorrect.crt", SignCsrWorkspace));
                IncorrectSignConfig.days = 365;

                await certgen.SignCSRAsync(IncorrectSignConfig, "TestCSR.csr", "TestCA.crt", "TestCA.key", "TestCertificateFromIncorrectAsync.crt", SignCsrWorkspace);
                Assert.That(File.Exists($"{SignCsrWorkspace}\\TestCertificateFromIncorrectAsync.crt"));
            });
        }

        [Test, Order(6)]
        public async Task TestConvertToPfxAsync()
        {
            await certgen.ConvertX509ToPfxAsync("TestCertificate.crt", "TestKEY.key", "TestPFXAsync.pfx", "", SignCsrWorkspace);
            Assert.That(File.Exists(SignCsrWorkspace + "\\TestPFXAsync.pfx"));
        }

    }
}
