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
          public abstract void GenerateCA(string outputpath);
        public abstract Task GenerateCA_Async(string OutputPath);
        public CertGenClassesParent()
          {
            LoadCAconfig();
            //Console.WriteLine("parent ctor");
  
          }

        internal abstract void LoadCAconfig();

       








    }
}
