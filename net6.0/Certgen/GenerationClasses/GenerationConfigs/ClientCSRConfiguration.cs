using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream
{
      public class CSRConfiguration
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
        public string CSRFileName="certificate.csr";

     
        
        





        public HashAlgorithms? HashAlgorithm;
        public KeyLengths? KeyLength;
        public Encodings? Encoding = Encodings.Default;

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
                    throw new Exceptions.CountryCodeInvalidException("CountryCode is NULL");
                }
            }
            set
            {
                int length;
                if (value is null) { throw new Exceptions.CountryCodeInvalidException("Passed value is NULL"); }
                if (VerifyCountryCode(value, out length)) { CountryCodeString = value?.ToUpper(); }
                else { throw new Exceptions.CountryCodeInvalidException(length); }

            }
        }
        public string? State { internal get; set; }
        public string? City { internal get; set; }
        public string? Organization { internal get; set; }
        public string? CommonName { internal get; set; }
        public List<string> alt_names {  get; set; } = new List<string>();


        private bool VerifyCountryCode(string CountryCode, out int length)
        {
            length = CountryCode.Length;
            return CountryCode.Length == 2 ? true : false;
        }

        internal void VerifyConfiguration()
        {
            if (CSRFileName.Contains(".csr"))
            {
                CSRFileName = CSRFileName.Replace(".csr", "");
            }
    

            if(HashAlgorithm is null) { throw new Exceptions.CSRConfigurationException("Hash algorithm is null or is not set propertly"); }
            if(KeyLength is null) { throw new Exceptions.CSRConfigurationException("RSA keylength is null or is not set propertly"); }
            if(CommonName is null) { throw new Exceptions.CSRConfigurationException("Common name for csr is null or not set propertly"); }
            


        }

}
}
