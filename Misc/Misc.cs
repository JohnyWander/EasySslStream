﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Misc
{
    public class Misc
    {

        public void TestRaiseMessage()
        {

            DynamicConfiguration.RaiseMessage?.Invoke("Test Message","test title");
        }


    }
}
