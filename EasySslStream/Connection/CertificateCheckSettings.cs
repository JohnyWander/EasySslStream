namespace EasySslStream.Connection
{
    /// <summary>
    /// Containts settings for certificate verification
    /// </summary>
    public class CertificateCheckSettings
    {
        public bool VerifyCertificateChain = true;
        public bool VerifyCertificateName = true;
    }
}
