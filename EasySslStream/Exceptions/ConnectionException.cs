namespace EasySslStream.Exceptions
{
    [Serializable]
    internal class ConnectionException : Exception
    {


        public ConnectionException(string message) : base("Connection:" + message)
        {


        }


        public ConnectionException() : base("Unknown Connection Exception")
        {


        }

    }
}
