﻿using EasySslStream.CertGenerationClasses;
using EasySslStream.Connection.Full;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream
{
    
      public static class Program

      {

          static void Main()
          {

              DynamicConfiguration.EnableDebugMode(DynamicConfiguration.DEBUG_MODE.Console);
            
              DynamicConfiguration.CA_CONFIG.Encoding = CA_CertGen.Encodings.UTF8;
              DynamicConfiguration.CA_CONFIG.KeyLength = CA_CertGen.KeyLengths.RSA_2048;
              DynamicConfiguration.CA_CONFIG.HashAlgorithm = CA_CertGen.HashAlgorithms.sha256;
            //  DynamicConfiguration.CA_CONFIG.Location = "New York";
             // DynamicConfiguration.CA_CONFIG.Organisation = "White Hause";
              DynamicConfiguration.CA_CONFIG.CommonName = "gfvąąąąv.com";
            //  DynamicConfiguration.CA_CONFIG.CountryCode = "US";
             // DynamicConfiguration.CA_CONFIG.CountryState = "Florida";
            //  DynamicConfiguration.CA_CONFIG.Days = 365;
            
              CertGenerationClasses.OpensslCertGeneration gen = new OpensslCertGeneration();

             //  gen.GenerateCA("CA");
      //      Task.Run(async () =>
           // {
           //     await gen.GenerateCA_Async("async2");
           // }).Wait();
               
                CSRConfiguration conf = new CSRConfiguration();
               // conf.Organization = "ME";
                conf.KeyLength = CSRConfiguration.KeyLengths.RSA_2048;
                conf.CommonName = "mypc.com";
                conf.alt_names.Add("mypc.com");
                conf.alt_names.Add("xd.mypc.com");
                conf.State = "Florida";
                conf.City = "Texas";
                conf.CountryCode = "US";
                conf.Encoding = CSRConfiguration.Encodings.UTF8;
                conf.HashAlgorithm = CSRConfiguration.HashAlgorithms.sha256;


            //  gen.GenerateCSR(conf,"csr");
            //gen.Generate_Async(conf, "csrAsync");
            // gen.GenerateCSRAsync(conf, "csrAsync");


                SignCSRConfig signconf = new SignCSRConfig();
            
                signconf.copyallextensions = true;
                signconf.SetAuthorityKeyIdentifiers(SignCSRConfig.authorityKeyIdentifiers.keyid_and_issuer);
                signconf.SetBasicConstrainsList(SignCSRConfig.basicConstrains.CAFalse);
                signconf.SetExtendedKeyUsage(new SignCSRConfig.ExtendedKeyUsage[] {
                SignCSRConfig.ExtendedKeyUsage.clientAuth,
                SignCSRConfig.ExtendedKeyUsage.serverAuth
                });
                signconf.days = 365;


            //gen.SignCSR(signconf,"csr/certificate.csr","CA/CA.crt","CA/CA.key","certificate","crt"); // ok
           // gen.SignCSRAsync(signconf, "csr/certificate.csr", "CA/CA.crt", "CA/CA.key", "certificateByAsync", "crt").Wait();
           
            
            
                         //   gen.ConvertX509ToPfx("crt\\certificate.crt", "csr//certificate.key", "cert.pfx", "123"); // ok
                          //  gen.ConvertX509ToPfxAsync("crt\\certificate.crt", "csr//certificate.key", "cert.pfx", "123"); // ok
           

                                        //DynamicConfiguration.TransportBufferSize = 4096; // WORKS FINE
                                        DynamicConfiguration.TransportBufferSize = 8192;// Works fine, better transfer speed very rarely
                                                                                        //  DynamicConfiguration.TransportBufferSize = 16384;// :/ sometimes crashes

                                        Server server = new Server();
                                        server.CertificateCheckSettings.VerifyCertificateName = false;
                                        server.CertificateCheckSettings.VerifyCertificateChain = false;



                                        server.StartServer(IPAddress.Any, 10000, "cert.pfx", "123", false);


                                        Thread.Sleep(10000);


            foreach (SSLClient cl in server.ConnectedClients)
            {
                /*
                    IDirectoryReceiveEventAndStats dreas = cl.DirectoryReceiveEventAndStats;
                    dreas.AutoStartDirectoryReceiveSpeedCheck = true;
                    dreas.DefaultDirectoryReceiveUnit = ConnectionCommons.Unit.MBs;
                    dreas.DirectoryReceiveCheckInterval = 1000;
                    dreas.OnDirectoryReceiveSpeedChecked += (object sender, EventArgs e) =>
                    {             
                        Console.WriteLine(dreas.CurrentReceiveFile);
                    };
                */
                IDirectorySendEventAndStats seas = cl.DirectorySendEventAndStats;
                seas.AutoStartDirectorySendSpeedCheck = true;
                seas.DefaultDirectorySendUnit = ConnectionCommons.Unit.MBs;
                seas.DirectorySendCheckInterval = 1000;
                seas.OnDirectorySendSpeedChecked += (object sender, EventArgs e) =>
                {
                    Console.Clear();
                    Console.WriteLine(seas.stringDirectorySendSpeed);
                    
                };

                Thread.Sleep(3000);

                cl.SendDirectory("C:\\TEST");
            }

            
          
       
        }



    }

    }
