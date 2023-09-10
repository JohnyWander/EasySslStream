namespace EasySslStream.Exceptions
{
    public class SignCsrException : Exception
    {
        public SignCsrException(string Message) : base(Message) { }

        public SignCsrException() : base("Unknown sign csr error") { }
    }
}
