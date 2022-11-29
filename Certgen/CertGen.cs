using EasySslStream.CertGenerationClasses;
using EasySslStream.GenerationClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream
{
    public class CertGen
    {

        private DotNetCertGeneration? DotNetCertGeneration;
        private MakecertCertGeneration? MakecertCertGeneration;
        private OpensslCertGeneration? OpensslCertGeneration;

        
       

        public CertGen()
        {

            switch (DynamicConfiguration.Certgen_Mode)
            {

                case DynamicConfiguration.SSL_Certgen_mode.OpenSSL:
                {
                        DotNetCertGeneration = new DotNetCertGeneration();
                        


                        break;
                }


            }






        }




    }
}
