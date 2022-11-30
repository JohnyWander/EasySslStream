using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Abstraction
{
    public abstract class CertGenClassesParent
    {
         internal string CAHashAlgo;
         internal string CAKeyLength;
         internal string CAdays;
        internal string CACountry;

        public abstract void GenerateCA(string outputpath);
        
        public CertGenClassesParent()
        {
           
  
        }
       
        internal virtual void LoadCAconfig()
        {
            CAHashAlgo = DynamicConfiguration.CA_CONFIG.HashAlgorithm.ToString();
            CAKeyLength = DynamicConfiguration.CA_CONFIG.KeyLength.ToString();
            CAdays = Convert.ToString(DynamicConfiguration.CA_CONFIG.Days);

        }









    }
}
