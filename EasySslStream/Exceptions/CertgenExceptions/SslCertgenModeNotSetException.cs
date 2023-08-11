namespace EasySslStream.Exceptions
{
    [Serializable]
    internal class SslCertgenModeNotSetException : Exception
    {

        public SslCertgenModeNotSetException()
            : base("Certgen mode is not set! Please set it using SelectCertGenMode function in dynamic configuration")
        {


        }

    }
}
