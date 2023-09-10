namespace EasySslStream.Exceptions
{
    [Serializable]
    internal class CSRConfigurationException : Exception
    {

        public CSRConfigurationException() : base("At least one of CSR's required values are NOT set")
        {

        }

        public CSRConfigurationException(string Message) : base(Message)
        {

        }

    }
}
