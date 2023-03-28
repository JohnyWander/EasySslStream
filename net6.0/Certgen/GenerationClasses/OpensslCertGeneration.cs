﻿using EasySslStream.Abstraction;
using EasySslStream.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace EasySslStream.CertGenerationClasses
{
 

    public partial class OpensslCertGeneration : Abstraction.CertGenClassesParent
    {


        /// <summary>
        /// Asynchronously Creates x509 CA certificate, based on ca configuration from DynamicConfiguration Class
        /// </summary>
        /// <param name="OutputPath">Path where CA.crt, CA.key should appear</param>
        /// <returns></returns>
        public override Task GenerateCA_Async(string OutputPath="default")
          {
            LoadCAconfig();
            TaskCompletionSource<object> generation_completion = new TaskCompletionSource<object>();
           // generation_completion.Task.ConfigureAwait(false);
            if (OutputPath != "default")
            {
                try { Directory.SetCurrentDirectory(OutputPath); }
                catch { Directory.CreateDirectory(OutputPath); Directory.SetCurrentDirectory(OutputPath); }
            }




            string configFile = @$"[req]
default_bits= {base.CAKeyLength}
prompt = no
default_md = {base.CAHashAlgo}
distinguished_name = dn
[dn]" + "\n";
            if (base.CACountry is not null) { configFile += $"C={base.CACountry}\n"; }
            if (base.CAState is not null) { configFile += $"ST={base.CAState}\n"; }
            if (base.CALocation is not null) { configFile += $"L={base.CALocation}\n"; }
            if (base.CAOrganisation is not null) { configFile += $"O={base.CAOrganisation}\n"; }
            if (base.CACommonName is not null) { configFile += $"CN={base.CACommonName}\n"; }

            if (base.CAHashAlgo == "" || base.CAHashAlgo is null) { throw new Exceptions.CAconfigurationException("Hash alghorithm is not set in DynamicConfiguration"); }

            if (!configFile.IsNormalized(NormalizationForm.FormD) && base.CAGenerationEncoding != "-utf8")
            {
                throw new Exceptions.CAconfigurationException("Strins provided for CA generation contains diacretics, please set encoding to utf-8 in DynamicConfiguration");
            }



            File.WriteAllText("genconf.txt", configFile);
            string cmdargs = $"req -new -x509 -{base.CAHashAlgo} -nodes -newkey rsa:{base.CAKeyLength} -days {base.CAdays} {base.CAGenerationEncoding} -keyout CA.key -out CA.crt -config genconf.txt";
            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = DynamicConfiguration.OpenSSl_config.OpenSSL_PATH + "\\" + "openssl.exe";
                openssl.StartInfo.CreateNoWindow = true;
                openssl.StartInfo.UseShellExecute = false;
                openssl.StartInfo.Arguments = cmdargs;
                openssl.EnableRaisingEvents = true;
                openssl.StartInfo.RedirectStandardError = true;
                if (OutputPath != "default")
               // {
               //     openssl.StartInfo.WorkingDirectory = OutputPath;
               // }
                openssl.Exited += (sender, args) =>
                {
                    if (openssl.ExitCode != 0)
                    {
                        string err = openssl.StandardError.ReadToEnd();                     
                        generation_completion.SetException(new Exceptions.CACertgenFailedException($"Generation Failed with error:{err} "));                        
                    }
                    else
                    {
                        generation_completion.SetResult(null); 
                    }
                };

                
                openssl.Start();
                openssl.WaitForExit();
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            //    Task.Run(() =>
               // {
                    File.Delete(OutputPath =="default"? "genconf.txt":$"{OutputPath}" + "/genconf.txt");
              //  }).Wait();
                
                return generation_completion.Task;
            }


        }



        /// <summary>
        /// Creates x509 CA certificate, based on ca configuration from DynamicConfiguration Class
        /// </summary>
        /// <param name="OutputPath">Path where CA.crt, CA.key should appear</param>
        public override void GenerateCA(string OutputPath = "default")
        {
            LoadCAconfig();
            if (OutputPath != "default")
            {
                try { Directory.SetCurrentDirectory(OutputPath); }
                catch { Directory.CreateDirectory(OutputPath); Directory.SetCurrentDirectory(OutputPath); }
            }

            string configFile = @$"[req]
default_bits= {base.CAKeyLength}
prompt = no
default_md = {base.CAHashAlgo}
distinguished_name = dn
[dn]" + "\n";
            if (base.CACountry is not null) { configFile += $"C={base.CACountry}\n"; }
            if (base.CAState is not null) { configFile += $"ST={base.CAState}\n"; }
            if (base.CALocation is not null) { configFile += $"L={base.CALocation}\n"; }
            if (base.CAOrganisation is not null) { configFile += $"O={base.CAOrganisation}\n"; }
            if (base.CACommonName is not null) { configFile += $"CN={base.CACommonName}\n"; }

            if(base.CAHashAlgo=="" || base.CAHashAlgo is null) { throw new Exceptions.CAconfigurationException("Hash alghorithm is not set in DynamicConfiguration"); }

            if (!configFile.IsNormalized(NormalizationForm.FormD) && base.CAGenerationEncoding!="-utf8")
            {
                throw new Exceptions.CAconfigurationException("Strins provided for CA generation contains diacretics, please set encoding to utf-8 in DynamicConfiguration");
            }
        

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
                if (OutputPath != "default")
                {
                    openssl.StartInfo.WorkingDirectory = OutputPath;
                }
                // openssl.BeginErrorReadLine();
                //  openssl.BeginOutputReadLine();

                // openssl.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                // {

                //};
                
                openssl.WaitForExit();
                


                if (openssl.ExitCode != 0)
                {
                    DynamicConfiguration.RaiseMessage?.Invoke(openssl.StandardError.ReadToEnd(), "Openssl Error");
                }
               // Console.WriteLine(openssl.StandardError.ReadToEnd());

                
            }

             File.Delete("genconf.txt");

         
            

            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        }


        
        /// <summary>
        /// Generates csr based on settings from CSRConfiguration class
        /// </summary>
        /// <param name="config">Instance of CSRConfiguration class that contains configuration</param>
        /// <param name="OutputPath">Output path</param>
        public override void GenerateCSR(CSRConfiguration config,string OutputPath= "default")
        {
            config.VerifyConfiguration();
            if (OutputPath != "default")
            {
                try { Directory.SetCurrentDirectory(OutputPath); }
                catch { Directory.CreateDirectory(OutputPath); Directory.SetCurrentDirectory(OutputPath); }
            }
      


            string confile = $@"[req]
default_bits={config.KeyLength.ToString().Split('_')[1]}
prompt=no
default_md={config.HashAlgorithm.ToString()}
";
            confile += config.alt_names.Count > 0 ? "req_extensions = req_ext\n" : "";

            confile += @$"distinguished_name = dn

[ dn ]"+"\n";
            if (config.CountryCodeString is not null) { confile += $"C={config.CountryCodeString}\n"; }
            if (config.State is not null) { confile += $"ST={config.State}\n"; }
            if(config.City is not null) { confile += $"L={config.City}\n";}
            if (config.Organization is not null) { confile += $"O={config.Organization}\n"; }
            if (config.CommonName is not null) { confile += $"CN={config.CommonName}\n"; }


            if (config.alt_names.Count != 0)
            {
                confile += $@"[req_ext]
subjectAltName = @alt_names
[alt_names]
";
                int alt_count = 1;
                foreach (string altName in config.alt_names)
                {
                    confile += "DNS." + Convert.ToString(alt_count) + "= " + altName + "\n";
                    alt_count++;
                }

            }


            File.WriteAllText("genconfcsr.txt", confile);

            string encoding="";

            if(config.Encoding == CSRConfiguration.Encodings.UTF8)
            {
                encoding = "-utf8";
            }


            string cmdargs = $"req -new -{config.HashAlgorithm.ToString()} -nodes -newkey rsa:{config.KeyLength.ToString().Split('_')[1]} {encoding} -keyout {config.CSRFileName}.key -out {config.CSRFileName}.csr -config genconfcsr.txt";
            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = DynamicConfiguration.OpenSSl_config.OpenSSL_PATH + "\\" + "openssl.exe";
                openssl.StartInfo.CreateNoWindow = true;
                //  openssl.StartInfo.UseShellExecute = false;
                openssl.StartInfo.Arguments = cmdargs;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
               
           
                openssl.Start();
                if (OutputPath != "default")
                {
                    openssl.StartInfo.WorkingDirectory = OutputPath;
                }
                openssl.WaitForExit();
              


                if (openssl.ExitCode != 0)
                {
                    DynamicConfiguration.RaiseMessage?.Invoke(openssl.StandardError.ReadToEnd(), "Openssl Error");
             
                }
                File.Delete("genconfcsr.txt");
                // Console.WriteLine(openssl.StandardError.ReadToEnd());
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            }


        }

        /// <summary>
        /// Asynchronously generates csr based on settings from CSRConfiguration class
        /// </summary>
        /// <param name="config">Instance of CSRConfiguration class that contains configuration</param>
        /// <param name="OutputPath">Output path</param>
        /// <returns>Task object that indicates task completion</returns>
        public override Task GenerateCSRAsync(CSRConfiguration config, string OutputPath = "default")
        {
            TaskCompletionSource<object> CSRgenCompletion = new TaskCompletionSource<object>();
            try
            {
                config.VerifyConfiguration();
            }
            catch(Exception e)
            {
                CSRgenCompletion.SetException(e);
            }
            if (OutputPath != "default")
            {
                try { Directory.SetCurrentDirectory(OutputPath); }
                catch { Directory.CreateDirectory(OutputPath); Directory.SetCurrentDirectory(OutputPath); }
            }



            string confile = $@"[req]
default_bits={config.KeyLength.ToString().Split('_')[1]}
prompt=no
default_md={config.HashAlgorithm.ToString()}
";
            confile += config.alt_names.Count > 0 ? "req_extensions = req_ext\n" : "";

            confile += @$"distinguished_name = dn

[ dn ]" + "\n";
            if (config.CountryCodeString is not null) { confile += $"C={config.CountryCodeString}\n"; }
            if (config.State is not null) { confile += $"ST={config.State}\n"; }
            if (config.City is not null) { confile += $"L={config.City}\n"; }
            if (config.Organization is not null) { confile += $"O={config.Organization}\n"; }
            if (config.CommonName is not null) { confile += $"CN={config.CommonName}\n"; }
            if (config.alt_names.Count != 0)
            {
                confile += $@"[req_ext]
subjectAltName = @alt_names
[alt_names]
";
                int alt_count = 1;
                foreach (string altName in config.alt_names)
                {
                    confile += "DNS." + Convert.ToString(alt_count) + "= " + altName + "\n";
                    alt_count++;
                }

            }



            File.WriteAllText("genconfcsr.txt", confile);

            string encoding = "";

            if (config.Encoding == CSRConfiguration.Encodings.UTF8)
            {
                encoding = "-utf8";
            }

            string cmdargs = $"req -new -{config.HashAlgorithm.ToString()} -nodes -newkey rsa:{config.KeyLength.ToString().Split('_')[1]} {encoding} -keyout {config.CSRFileName}.key -out {config.CSRFileName}.csr -config genconfcsr.txt";

            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = DynamicConfiguration.OpenSSl_config.OpenSSL_PATH + "\\" + "openssl.exe";
                openssl.StartInfo.CreateNoWindow = true;
                //  openssl.StartInfo.UseShellExecute = false;
                openssl.StartInfo.Arguments = cmdargs;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.EnableRaisingEvents = true;

                openssl.Exited += (sender, args) =>
                {

                    if (openssl.ExitCode != 0)
                    {
                        string err = openssl.StandardError.ReadToEnd();
                        Console.WriteLine(err);
                        CSRgenCompletion.SetException(new Exceptions.CACertgenFailedException($"Generation Failed with error:{err} "));
                        //Console.WriteLine("EVENT");
                    }
                    else
                    {
                        CSRgenCompletion.SetResult(null);
                        // Console.WriteLine("EVENT");

                    }
                };

            
                openssl.Start();
                if (OutputPath != "default")
                {
                    openssl.StartInfo.WorkingDirectory = OutputPath;
                }
                openssl.WaitForExit();
                


                if (openssl.ExitCode != 0)
                {
                    DynamicConfiguration.RaiseMessage?.Invoke(openssl.StandardError.ReadToEnd(), "Openssl Error");

                }

               // Task.Run(() =>
            //    {
                    File.Delete("genconfcsr.txt");

                //}).Wait() ;
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
                // Console.WriteLine(openssl.StandardError.ReadToEnd());
                return CSRgenCompletion.Task;
                
            }


            
        }



        /// <summary>
        /// Signs certificate signing request
        /// </summary>
        /// <param name="config"></param>
        /// <param name="CSRpath"></param>
        /// <param name="CAPath"></param>
        /// <param name="CAKeyPath"></param>
        /// <param name="CertName"></param>
        /// <param name="OutputPath"></param>
        public override void SignCSR(SignCSRConfig config, string CSRpath, string CAPath, string CAKeyPath,string CertName, string OutputPath = "default")
        {
            if (OutputPath != "default")
            {
                try { Directory.SetCurrentDirectory(OutputPath); }
                catch { Directory.CreateDirectory(OutputPath); Directory.SetCurrentDirectory(OutputPath); }
            }

           
            if (!CertName.Contains(".crt")) { CertName += ".crt"; }
            string copyExtensions = "";
            if (config.copyallextensions == true) { copyExtensions = "-copy_extensions copyall"; }

            File.WriteAllText("signconf.txt",config.BuildConfFile());
            string command = @$"req -x509 -in {CSRpath} -CA {CAPath} -CAkey {CAKeyPath} -out {CertName} -days {config.days} -copy_extensions copyall -config signconf.txt";


            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = DynamicConfiguration.OpenSSl_config.OpenSSL_PATH + "\\" + "openssl.exe";
                openssl.StartInfo.CreateNoWindow = true;
                openssl.StartInfo.Arguments = command;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.Start();
                if (OutputPath != "default")
                {
                    openssl.StartInfo.WorkingDirectory = OutputPath;
                }
                openssl.WaitForExit();
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
                if (openssl.ExitCode != 0)
                {
                    DynamicConfiguration.RaiseMessage?.Invoke(openssl.StandardError.ReadToEnd(), "Openssl Error");

                }
            }
            }

        /// <summary>
        /// Asynchrounously signs certificate signing request
        /// </summary>
        /// <param name="config"></param>
        /// <param name="CSRpath"></param>
        /// <param name="CAPath"></param>
        /// <param name="CAKeyPath"></param>
        /// <param name="CertName"></param>
        /// <param name="OutputPath"></param>
        /// <returns></returns>
        public override Task SignCSRAsync(SignCSRConfig config, string CSRpath, string CAPath, string CAKeyPath, string CertName, string OutputPath = "default")
        {
            
            TaskCompletionSource<object> SignCompletion = new TaskCompletionSource<object>();

            if (OutputPath != "default")
            {
                try { Directory.SetCurrentDirectory(OutputPath); }
                catch { Directory.CreateDirectory(OutputPath); Directory.SetCurrentDirectory(OutputPath); }
            }
            if (!CertName.Contains(".crt")) { CertName += ".crt"; }
            string copyExtensions = "";
            if (config.copyallextensions == true) { copyExtensions = "-copy_extensions copyall"; }

            File.WriteAllText("signconf.txt", config.BuildConfFile());
            string command = @$"req -x509 -in {CSRpath} -CA {CAPath} -CAkey {CAKeyPath} -out {CertName} -days {config.days} -copy_extensions copyall -config signconf.txt";
            Console.WriteLine(command);

            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = DynamicConfiguration.OpenSSl_config.OpenSSL_PATH + "\\" + "openssl.exe";
                openssl.StartInfo.CreateNoWindow = true;
                //  openssl.StartInfo.UseShellExecute = false;
                openssl.StartInfo.Arguments = command;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.EnableRaisingEvents = true;

                openssl.Exited += (sender, args) =>
                {
                    if (openssl.ExitCode != 0)
                    {
                        string err = openssl.StandardError.ReadToEnd();
                        SignCompletion.SetException(new Exceptions.CACertgenFailedException($"Generation Failed with error:{err} "));
                    }
                    else
                    {
                        SignCompletion.SetResult(null);
                    }
                };
                openssl.Start();
                if (OutputPath != "default")
                {
                    openssl.StartInfo.WorkingDirectory = OutputPath;
                }
                openssl.WaitForExit();
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
                if (openssl.ExitCode != 0)
                {
                    DynamicConfiguration.RaiseMessage?.Invoke(openssl.StandardError.ReadToEnd(), "Openssl Error");
                }
                return SignCompletion.Task;

            }
        }



        public override void ConvertX509ToPfx(string Certpath, string KeyPath, string Certname, string Password, string OutputPath = "default")
        {
            if (OutputPath != "default")
            {
                try { Directory.SetCurrentDirectory(OutputPath); }
                catch { Directory.CreateDirectory(OutputPath); Directory.SetCurrentDirectory(OutputPath); }
            }
            if (Certname.Contains(".pfx"))
            {
                Certname += ".pfx";
            }
            string command = $"pkcs12 -export -out {Certname} -inkey {KeyPath} -in {Certpath} -passout pass:{Password}";
            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = DynamicConfiguration.OpenSSl_config.OpenSSL_PATH + "\\" + "openssl.exe";
                openssl.StartInfo.CreateNoWindow = true;
                openssl.StartInfo.Arguments = command;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.Start();
                if (OutputPath != "default")
                {
                    openssl.StartInfo.WorkingDirectory = OutputPath;
                }
                openssl.WaitForExit();
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
                if (openssl.ExitCode != 0)
                {
                    DynamicConfiguration.RaiseMessage?.Invoke(openssl.StandardError.ReadToEnd(), "Openssl Error");
                }
            }
        }


        public override Task ConvertX509ToPfxAsync(string Certpath, string KeyPath, string Password, string Certname, string OutputPath)
        {
            TaskCompletionSource<object> convertcompletion = new TaskCompletionSource<object>();
            if (OutputPath != "default")
            {
                try { Directory.SetCurrentDirectory(OutputPath); }
                catch { Directory.CreateDirectory(OutputPath); Directory.SetCurrentDirectory(OutputPath); }
            }
           
            if (Certname.Contains(".pfx"))
            {
                Certname += ".pfx";
            }
            string command = $"pkcs12 -export -out {Certname} -inkey {KeyPath} -in {Certpath} -passout pass:{Password}";

            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = DynamicConfiguration.OpenSSl_config.OpenSSL_PATH + "\\" + "openssl.exe";
                openssl.StartInfo.CreateNoWindow = true;                
                openssl.StartInfo.Arguments = command;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.EnableRaisingEvents = true;
                openssl.Exited += (sender, args) =>
                {
                    if (openssl.ExitCode != 0)
                    {
                        string err = openssl.StandardError.ReadToEnd();
                        convertcompletion.SetException(new Exceptions.CACertgenFailedException($"Generation Failed with error:{err} "));
                    }
                    else
                    {
                        convertcompletion.SetResult(null);
                    }
                };


                openssl.Start();
                if (OutputPath != "default")
                {
                    openssl.StartInfo.WorkingDirectory = OutputPath;
                }
                openssl.WaitForExit();
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
                if (openssl.ExitCode != 0)
                {
                    DynamicConfiguration.RaiseMessage?.Invoke(openssl.StandardError.ReadToEnd(), "Openssl Error");

                }
                return convertcompletion.Task;

            }
        }

        internal override void LoadCAconfig()
        {
            
                CAHashAlgo = DynamicConfiguration.CA_CONFIG.HashAlgorithm.ToString();
            try
            {
                CAKeyLength = DynamicConfiguration.CA_CONFIG.KeyLength.ToString().Split('_')[1];
            }
            catch (IndexOutOfRangeException)
            {
                 throw new Exceptions.CAconfigurationException("Key length not set correctly in DynamicConfiguration");
               
            }
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


           // if (CAHashAlgo is null || CAKeyLength is null || CAdays is null || CACountry is null || CAState is null || CALocation is null || CACommonName is null)
           // {
            //    throw new Exceptions.CAconfigurationException("At least one of the required parameters for CA certificate generation is NOT set");
            //}
            
            if(CAHashAlgo is null){ throw new Exceptions.CAconfigurationException("CA hash algorithm is not set in DynamicConfiguration"); }
            if(CAKeyLength is null) { throw new Exceptions.CAconfigurationException("RSA key lentgh for CA certificate is not set in DynamicConfiguration"); }
            if(CAdays is null) { throw new Exceptions.CAconfigurationException("Validity days of CA certificate are not set in DynamicConfiguration"); }
            if(CACommonName is null) { throw new Exceptions.CAconfigurationException("Common name for CA cetificate is not set in DynamicConfiguration"); }


        }




    }
}
