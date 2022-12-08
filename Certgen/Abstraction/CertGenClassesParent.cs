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
         internal string CAState;
         internal string CALocation;
         internal string CAOrganisation;
         internal string CACommonName;

        public abstract void GenerateCA(string outputpath);
        
        public CertGenClassesParent()
        {
            LoadCAconfig();
            //Console.WriteLine("parent ctor");
  
        }

        internal abstract void LoadCAconfig();
        









    }
}
