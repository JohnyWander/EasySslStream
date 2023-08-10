namespace EasySslStream.Exceptions
{
    internal class PFXConvertException : Exception
    {
        public PFXConvertException(string message) : base(message) { }

        public PFXConvertException() : base("Unknown error when converting certificate to pfx format") { }
    }
}
