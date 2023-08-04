﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySslStream;
using EasySslStream.CertGenerationClasses;

namespace EasySslStreamTests.CertgenTests
{
    public class OpenSslSignCSRTests
    {
        OpensslCertGeneration certgen = new OpensslCertGeneration();

        SignCSRConfig CorrectConfig = new SignCSRConfig();
        SignCSRConfig IncorrectCSRConfig = new SignCSRConfig();


        [SetUp]
        public void SetUp()
        {

            certgen = new OpensslCertGeneration();
            CorrectConfig = new SignCSRConfig();
            IncorrectCSRConfig = new SignCSRConfig();




        }


    }
}
