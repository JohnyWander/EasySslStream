namespace EasySslStream.Exceptions
{
    [Serializable]
    internal class SignCSRConfigurationException : Exception
    {
        public SignCSRConfigurationException(string message) : base(message)
        {

        }

        public SignCSRConfigurationException() : base("Unknown configuration error")
        {

        }




    }
}
