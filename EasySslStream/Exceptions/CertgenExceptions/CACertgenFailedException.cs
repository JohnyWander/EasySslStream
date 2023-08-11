namespace EasySslStream.Exceptions
{
    [Serializable]
    internal class CACertgenFailedException : Exception
    {
        string defaultMessage = "Certificate signing failed, Unknown error";
        public CACertgenFailedException(string message) : base(message)
        {

        }

        public CACertgenFailedException() : base("CA certificate failed, unknown error")
        {

        }


    }
}
