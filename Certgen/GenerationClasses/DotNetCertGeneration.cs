﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream
{
    internal class DotNetCertGeneration : Abstraction.CertGenClassesParent
    {
        public override void GenerateCA(string outputpath)
        {
            throw new NotImplementedException();
        }

        internal override void LoadCAconfig()
        {
            throw new NotImplementedException();
        }

        public override Task GenerateCA_Async(string outpath)
        {
            throw new NotImplementedException();
        }

    }
}
