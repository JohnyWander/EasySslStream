namespace EasySslStream.Exceptions
{
    [Serializable]
    public class CountryCodeInvalidException : Exception
    {
        public CountryCodeInvalidException(string message) : base(message)
        {

        }

        public CountryCodeInvalidException(int Length)
          : base($"Provided Country code is invalid it's length must be 2 - Length: {Length}")
        {


        }
    }
}
