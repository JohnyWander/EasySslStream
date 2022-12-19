using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Abstraction
{
    public abstract class CertGenClassesParent
    {
          protected string CAHashAlgo;
          protected string CAKeyLength;
          protected string CAdays;
          protected string CACountry;
          protected string CAState;
          protected string CALocation;
          protected string CAOrganisation;
          protected string CACommonName;
          protected string CAGenerationEncoding;
        public CertGenClassesParent()
        {
            LoadCAconfig();
            //Console.WriteLine("parent ctor");

        }
        internal abstract void LoadCAconfig();
      

        //CA
        public abstract void GenerateCA(string outputpath);
        public abstract Task GenerateCA_Async(string OutputPath);
      
        // CSR
        public abstract void GenerateCSR(ClientCSRConfiguration config, string OutputPath);
        public abstract Task GenerateCSRAsync(ClientCSRConfiguration config, string OutputPath);




        public abstract void SignCSR(SignCSRConfig config, string CSRpath, string CAPath, string CAKeyPath, string CertName,string Outputpath = "default");
       
        public abstract Task SignCSRAsync(SignCSRConfig config, string CSRpath, string CAPath, string CAKeyPath, string CertName, string Outputpath = "default");





    }
}
