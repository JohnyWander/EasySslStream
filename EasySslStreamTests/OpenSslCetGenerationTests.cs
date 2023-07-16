using EasySslStream.CertGenerationClasses;

namespace EasySslStreamTests
{
    public class OpenSslCetGenerationTests
    {
        private OpensslCertGeneration OpensslCertGen;
        

        [OneTimeSetUp]
        public void Setup()
        {
            OpensslCertGen = new OpensslCertGeneration();
          
        }



        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}