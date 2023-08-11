namespace EasySslStream.Exceptions
{
    public class CSRgenFailedException : Exception
    {
        public CSRgenFailedException(string Message) : base(Message)
        {
        }


        public CSRgenFailedException() : base("CA certificate failed, unknown error")
        {

        }
    }
}
