using System;
using EasySslStream.Exceptions;


namespace EasySslStream.Certgen.GenerationClasses.GenerationConfigs
{
    public class CaCertgenConfig
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
        public KeyLengths? KeyLength;
        public Encodings Encoding = Encodings.Default;


        public int Days { internal get; set; } = 365;// ex. 356

        internal string? CountryCodeString;
        public string? CountryCode
        {
            internal get
            {
                if (CountryCode is not null)
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
        public string? Organisation { internal get; set; }
        public string? CommonName { internal get; set; }

        private bool VerifyCountryCode(string CountryCode, out int length)
        {
            length = CountryCode.Length;
            return CountryCode.Length == 2 ? true : false;
        }
    }

}