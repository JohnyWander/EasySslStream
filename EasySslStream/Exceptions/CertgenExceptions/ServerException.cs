namespace EasySslStream.Exceptions
{
    [Serializable]
    internal class ServerException : Exception
    {


        public ServerException(string message) : base("Server Exception:" + message)
        {


        }


        public ServerException() : base("Unknown server Exception")
        {


        }

    }

}
