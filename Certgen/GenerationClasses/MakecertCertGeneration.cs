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


        public override void GenerateCSR(ClientCSRConfiguration config, string OutputPath)
        {
            throw new NotImplementedException();
        }

        public override Task GenerateCSRAsync(ClientCSRConfiguration config, string OutputPath)
        {
            throw new NotImplementedException();
        }
    }
}
