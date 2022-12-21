using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.GenerationClasses
{
    internal class MakecertCertGeneration : Abstraction.CertGenClassesParent
    {
        public override Task GenerateCA_Async(string outpath)
        {
            throw new NotImplementedException();
        }
        public override void GenerateCA(string outputpath)
        {
            throw new NotImplementedException();
        }

        internal override void LoadCAconfig()
        {
            throw new NotImplementedException();
        }


        public override void GenerateCSR(CSRConfiguration config, string OutputPath)
        {
            throw new NotImplementedException();
        }
        public override Task GenerateCSRAsync(CSRConfiguration config, string OutputPath)
        {
            throw new NotImplementedException();
        }


        public override void SignCSR(SignCSRConfig config, string CSRpath, string CAPath, string CAKeyPath, string CertName, string Outputpath = "default")
        {
            throw new NotImplementedException();
        }

        public override Task SignCSRAsync(SignCSRConfig config, string CSRpath, string CAPath, string CAKeyPath, string CertName, string Outputpath = "default")
        {
            throw new NotImplementedException();
        }

        public override void ConvertX509ToPfx(string Certpath, string KeyPath, string Password, string Certname, string OutputPath)
        {
            throw new NotImplementedException();
        }

        public override Task ConvertX509ToPfxAsync(string Certpath, string KeyPath, string Password, string Certname, string OutputPath)
        {
            throw new NotImplementedException();
        }

    }
}
