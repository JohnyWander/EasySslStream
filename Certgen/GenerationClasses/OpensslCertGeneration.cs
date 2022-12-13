﻿using EasySslStream.Abstraction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.CertGenerationClasses
{
    public class OpensslCertGeneration : Abstraction.CertGenClassesParent
    {
          public override Task GenerateCA_Async(string OutputPath="default")
          {
            TaskCompletionSource generation_completion = new TaskCompletionSource();
            if(OutputPath != "default")
            {
                Directory.SetCurrentDirectory(OutputPath);
            }

          }
          public override void GenerateCA(string OutputPath="default")
          {




            if (OutputPath != "default")
            {
                Directory.SetCurrentDirectory(OutputPath);
            }

            string configFile = @$"[req]
default_bits= {base.CAKeyLength}
prompt = no
default_md = {base.CAHashAlgo}
distinguished_name = dn
[dn]
C={base.CACountry}
ST={base.CAState}
L={base.CALocation}
O={base.CAOrganisation}
CN={base.CACommonName}";

            File.WriteAllText("genconf.txt", configFile);
            string cmdargs = $"req -new -x509 -{base.CAHashAlgo} -nodes -newkey rsa:{base.CAKeyLength} -days {base.CAdays} {base.CAGenerationEncoding} -keyout CA.key -out CA.crt -config genconf.txt";


            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = DynamicConfiguration.OpenSSl_config.OpenSSL_PATH+"\\" + "openssl.exe";
                openssl.StartInfo.CreateNoWindow = true;
              //  openssl.StartInfo.UseShellExecute = false;
                openssl.StartInfo.Arguments = cmdargs;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.Start();
               // openssl.BeginErrorReadLine();
              //  openssl.BeginOutputReadLine();

               // openssl.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
               // {
                    
                //};

                openssl.WaitForExit();
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);


                if (openssl.ExitCode != 0)
                {
                    DynamicConfiguration.RaiseMessage?.Invoke(openssl.StandardError.ReadToEnd(), "Openssl Error");
                }
               // Console.WriteLine(openssl.StandardError.ReadToEnd());

                
            }


        }

        internal override void LoadCAconfig()
        {
            
                CAHashAlgo = DynamicConfiguration.CA_CONFIG.HashAlgorithm.ToString();
                CAKeyLength = DynamicConfiguration.CA_CONFIG.KeyLength.ToString().Split('_')[1];
                CAdays = Convert.ToString(DynamicConfiguration.CA_CONFIG.Days);
                CACountry = DynamicConfiguration.CA_CONFIG.CountryCodeString;
                CAState = DynamicConfiguration.CA_CONFIG.CountryState;
                CALocation = DynamicConfiguration.CA_CONFIG.Location;
                CAOrganisation = DynamicConfiguration.CA_CONFIG.Organisation;
                CACommonName = DynamicConfiguration.CA_CONFIG.CommonName;
                if(DynamicConfiguration.CA_CONFIG.Encoding == CA_CertGen.Encodings.UTF8)
                {
                CAGenerationEncoding = "-utf8";
                }
                else
                {
                CAGenerationEncoding = string.Empty;
                }


            if (CAHashAlgo is null || CAKeyLength is null || CAdays is null || CACountry is null || CAState is null || CALocation is null || CACommonName is null)
            {
                throw new Exceptions.CAconfiguratonException("At least one of the required parameters for CA certificate generation is NOT set");
            }
            
        }




    }
}
