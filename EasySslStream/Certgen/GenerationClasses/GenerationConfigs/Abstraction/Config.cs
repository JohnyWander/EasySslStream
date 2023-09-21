using EasySslStream.Exceptions;

namespace EasySslStream.CertGenerationClasses.GenerationConfigs
{
    public abstract class Config
    {
        public enum HashAlgorithms
        {
            sha256,
            sha384
        }

        public enum KeyLengths
        {
            RSA_1024,
            RSA_2048,
            RSA_4096
        }

        public enum Encodings
        {
            Default,
            UTF8
        }


        public HashAlgorithms? HashAlgorithm;

        public string KeyLengthAsNumber;
        private KeyLengths? _KeyLength;
        public KeyLengths? KeyLength
        {
            get
            {
                return _KeyLength;
            }
            set
            {
                _KeyLength = value;
                KeyLengthAsNumber = _KeyLength.ToString().Split("_")[1];
            }
        }

        internal string EncodingAsString;
        private Encodings _Encoding;
        public Encodings Encoding
        {
            get { return _Encoding; }
            set
            {
                _Encoding = value;
                EncodingAsString = $"-{_Encoding.ToString()}";
            }

        }

        public int Days { internal get; set; } = 365;// ex. 356


        internal string? CountryCodeString;
        public string? CountryCode
        {
            get
            {
                if (CountryCodeString is not null)
                {
                    return CountryCodeString;
                }
                else
                {
                    throw new CountryCodeInvalidException("CountryCode is NULL");
                }
            }
            set
            {
                int length;
                if (value is null) { throw new CountryCodeInvalidException("Passed value is NULL"); }
                if (VerifyCountryCode(value, out length)) { CountryCodeString = value?.ToUpper(); }
                else { throw new CountryCodeInvalidException(length); }

            }
        }
        public string? CountryState { internal get; set; }
        public string? Location { internal get; set; }
        public string? Organization { internal get; set; }
        public string? CommonName { internal get; set; }

        public string? State { internal get; set; }
        public string? City { internal get; set; }
        public List<string> alt_names { get; set; } = new List<string>();
        private protected bool VerifyCountryCode(string CountryCode, out int length)
        {
            length = CountryCode.Length;
            return CountryCode.Length == 2 ? true : false;
        }


    }
}
