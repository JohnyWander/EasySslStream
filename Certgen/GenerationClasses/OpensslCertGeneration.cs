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
            Console.WriteLine(base.CAKeyLength);
          }





    }
}
