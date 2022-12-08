using EasySslStream.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.CertGenerationClasses
{
    public class OpensslCertGeneration : Abstraction.CertGenClassesParent
    {
          
          public override void GenerateCA(string OutputPath)
          {
            Console.WriteLine(base.CACountry);
          }

        internal override void LoadCAconfig()
        {
            
                CAHashAlgo = DynamicConfiguration.CA_CONFIG.HashAlgorithm.ToString();
                CAKeyLength = DynamicConfiguration.CA_CONFIG.KeyLength.ToString();
                CAdays = Convert.ToString(DynamicConfiguration.CA_CONFIG.Days);
                CACountry = DynamicConfiguration.CA_CONFIG.CountryCodeString;
                CAState = DynamicConfiguration.CA_CONFIG.CountryState;
                CALocation = DynamicConfiguration.CA_CONFIG.Location;
                CACommonName = DynamicConfiguration.CA_CONFIG.CommonName;

                
              
            
        }




    }
}
