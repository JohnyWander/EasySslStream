using EasySslStream.Abstraction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.CertGenerationClasses
{
    public class OpensslCertGeneration : Abstraction.CertGenClassesParent
    {
          
          public override void GenerateCA(string OutputPath)
          {
            string configFile = @$"[req]
default_bits= {base.CAKeyLength}
prompt = no
default_md = {base.CAHashAlgo}
distinguished_name = dn
[dn]
C={base.CACountry}
ST={base.CAState}
L={base.CALocation}
O={base.CAOrganisation}
CN={base.CACommonName}";

            File.WriteAllText("genconf.txt", configFile);

            string cmdargs = $"req -new -x509 -{base.CAHashAlgo} -nodes -newkey rsa:{base.CAKeyLength} -keyout CA.key -out CA.crt -config genconf.txt";


            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = DynamicConfiguration.OpenSSl_config.OpenSSL_PATH + "openssl.exe";

                openssl.StartInfo.Arguments = cmdargs;
                openssl.Start();
                openssl.WaitForExit();
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            }


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
