
using EasySslStream.CertGenerationClasses.GenerationConfigs;
using EasySslStream.Exceptions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace EasySslStream.CertGenerationClasses
{


    public partial class OpensslCertGeneration
    {
        public string _OpenSSLPath;


        public OpensslCertGeneration()
        {
            _OpenSSLPath = TryToFindOpenSSL();
            if (_OpenSSLPath is null || _OpenSSLPath == "")
            {
                throw new Exception("Could not found Openssl automatically. Please use specify path to openssl using constructor with string argument. ");
            }
        }

        public OpensslCertGeneration(string OpenSSLPath)
        {
            if (File.Exists(OpenSSLPath))
            {
                _OpenSSLPath = OpenSSLPath;
            }
            else
            {
                throw new Exception($"Provided path is not a valid OpenSSL path");
            }
        }
        #region Finding Related
        string TryToFindOpenSSL()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetOpenSSLLinux();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetOpenSSlWindows();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        string GetOpenSSLLinux()
        {
            using (Process which = new Process())
            {
                which.StartInfo.FileName = "which";
                which.StartInfo.UseShellExecute = false;
                which.StartInfo.RedirectStandardOutput = true;
                which.StartInfo.RedirectStandardError = true;

                which.Start();
                which.WaitForExit();

                if (which.ExitCode == 0)
                {
                    string output = which.StandardOutput.ReadToEnd();
                    return output;
                }
                else
                {
                    throw new Exception("Could not run which");
                }

            }
        }

        string GetOpenSSlWindows()
        {
            if (File.Exists(@"C:\Program Files\OpenSSL\bin\openssl.exe"))
            {
                return @"C:\Program Files\OpenSSL\bin\openssl.exe";
            }
            else if (File.Exists(@"C:\Program Files (x86)\OpenSSL\bin\openssl.exe"))
            {
                return @"C:\Program Files (x86)\OpenSSL\bin\openssl.exe";
            }
            else if (File.Exists(@"C:\Program Files\Git\usr\bin\openssl.exe"))
            {
                return @"C:\Program Files\Git\usr\bin\openssl.exe";
            }
            else if (File.Exists(@"C:\Program Files (x86)\Git\usr\bin\openssl.exe"))
            {
                return @"C:\Program Files (x86)\Git\usr\bin\openssl.exe";
            }
            else
            {
                return "";
            }
        }


        #endregion
        #region Generation Methods




        /// <summary>
        /// Asynchronously Creates x509 CA certificate, based on ca configuration provided.
        /// </summary>
        /// <param name="conf">Configuration for certificate generation</param>
        /// <param name="SaveDir">Path where files should be saved</param>
        /// <param name="CertFileName">File name for output certificate</param>
        /// <param name="KeyFileName">File name for output private key file</param>
        /// <returns></returns>
        public Task GenerateCaAsync(CaCertgenConfig conf, string SaveDir = "", string CertFileName = "CA.crt", string KeyFileName = "CA.key")
        {
            TaskCompletionSource<object> generation_completion = new TaskCompletionSource<object>();
            VerifyConfig(conf, generation_completion);
            if (SaveDir == "")
            {
                SaveDir = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                if (!Directory.Exists(SaveDir))
                {
                    Directory.CreateDirectory(SaveDir);
                }
            }

            string configFile = CreateOpensslCaConfig(conf);

            if (!configFile.IsNormalized(NormalizationForm.FormD) && conf.Encoding.ToString() != "UTF8")
            {
                generation_completion.TrySetException(new Exceptions.ConfigurationException("Strins provided for CA generation contains diacretics, please set encoding to utf-8 in Configuration class"));

            }
            File.WriteAllText(SaveDir != AppDomain.CurrentDomain.BaseDirectory ? $"{SaveDir}\\genconf.txt" : "genconf.txt", configFile);
            string cmdargs = $"req -new -x509 -{conf.HashAlgorithm} -nodes -newkey rsa:{conf.KeyLengthAsNumber} -days {conf.Days} {conf.EncodingAsString} -keyout {KeyFileName} -out {CertFileName} -config genconf.txt";
            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = this._OpenSSLPath;
                openssl.StartInfo.CreateNoWindow = true;
                openssl.StartInfo.UseShellExecute = false;
                openssl.StartInfo.Arguments = cmdargs;
                openssl.EnableRaisingEvents = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.StartInfo.WorkingDirectory = SaveDir;
                openssl.Exited += (sender, args) =>
                {
                    if (openssl.ExitCode != 0)
                    {
                        string err = openssl.StandardError.ReadToEnd();
                        generation_completion.TrySetException(new Exceptions.CACertgenFailedException($"Generation Failed with error:{err} "));
                    }
                    else
                    {
                        generation_completion.SetResult(new object());
                    }
                };
                openssl.Start();
                openssl.WaitForExit();
                File.Delete(SaveDir != AppDomain.CurrentDomain.BaseDirectory ? $"{SaveDir}\\genconf.txt" : "genconf.txt");
            }
            return generation_completion.Task;
        }



        /// <summary>
        /// Creates x509 CA certificate, based on ca configuration from DynamicConfiguration Class
        /// </summary>
        /// <param name="conf">Configuration for certificate generation</param>
        /// <param name="SaveDir">Path where files should be saved</param>
        /// <param name="CertFileName">File name for output certificate</param>
        /// <param name="KeyFileName">File name for output private key file</param>
        public void GenerateCA(CaCertgenConfig conf, string SaveDir = "", string CertFileName = "CA.crt", string KeyFileName = "CA.key")
        {
            VerifyConfig(conf);
            if (SaveDir == "")
            {
                SaveDir = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                if (!Directory.Exists(SaveDir))
                {
                    Directory.CreateDirectory(SaveDir);
                }
            }

            string configFile = CreateOpensslCaConfig(conf);

            if (!configFile.IsNormalized(NormalizationForm.FormD) && conf.Encoding.ToString() != "UTF8")
            {
                throw new Exceptions.ConfigurationException("Strins provided for CA generation contains diacretics, please set encoding to utf-8 in Configuration class");
            }
            File.WriteAllText(SaveDir != AppDomain.CurrentDomain.BaseDirectory ? $"{SaveDir}\\genconf.txt" : "genconf.txt", configFile);
            string cmdargs = $"req -new -x509 -{conf.HashAlgorithm} -nodes -newkey rsa:{conf.KeyLengthAsNumber} -days {conf.Days} {conf.EncodingAsString} -keyout {KeyFileName} -out {CertFileName} -config genconf.txt";
            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = this._OpenSSLPath;
                openssl.StartInfo.CreateNoWindow = true;
                openssl.StartInfo.UseShellExecute = false;
                openssl.StartInfo.Arguments = cmdargs;
                openssl.EnableRaisingEvents = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.StartInfo.WorkingDirectory = SaveDir;
                openssl.Exited += (sender, args) =>
                {
                    if (openssl.ExitCode != 0)
                    {
                        string err = openssl.StandardError.ReadToEnd();
                        throw new Exceptions.CACertgenFailedException($"Generation Failed with error:{err} ");
                    }
                };
                openssl.Start();
                openssl.WaitForExit();
                File.Delete(SaveDir != AppDomain.CurrentDomain.BaseDirectory ? $"{SaveDir}\\genconf.txt" : "genconf.txt");
            }
        }



        /// <summary>
        /// Generates csr based on settings from CSRConfiguration class
        /// </summary>
        /// <param name="config">Instance of CSRConfiguration class that contains configuration</param>
        /// <param name="SaveDir">Path where created csr and private key file should be saved</param>
        /// <param name="CSRFileName">Output csr file name</param>
        /// <param name="KeyFileName">Output private key file name</param>
        public void GenerateCSR(CSRConfiguration config, string SaveDir = "", string CSRFileName = "CSR.csr", string KeyFileName = "CSR.key")
        {
            VerifyConfig(config);
            if (SaveDir == "")
            {
                SaveDir = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                if (!Directory.Exists(SaveDir))
                {
                    Directory.CreateDirectory(SaveDir);
                }
            }

            File.WriteAllText(SaveDir != AppDomain.CurrentDomain.BaseDirectory ? $"{SaveDir}\\genconfcsr.txt" : "genconfcsr.txt", CreateOpensslCSRConfig(config));
            string cmdargs = $"req -new -{config.HashAlgorithm.ToString()} -nodes -newkey rsa:{config.KeyLength.ToString().Split('_')[1]} {config.EncodingAsString} -keyout {KeyFileName} -out {CSRFileName} -config genconfcsr.txt";
            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = this._OpenSSLPath;
                openssl.StartInfo.CreateNoWindow = true;
                //  openssl.StartInfo.UseShellExecute = false;
                openssl.StartInfo.Arguments = cmdargs;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.StartInfo.WorkingDirectory = SaveDir;
                openssl.Start();
                openssl.WaitForExit();

                if (openssl.ExitCode != 0)
                {
                    throw new CSRgenFailedException("CSR generation failed with error: " + openssl.StandardError.ReadToEnd());

                }
                File.Delete(SaveDir != AppDomain.CurrentDomain.BaseDirectory ? $"{SaveDir}\\genconfcsr.txt" : "genconfcsr.txt");
            }
        }

        /// <summary>
        /// Asynchronously generates csr based on settings from CSRConfiguration class
        /// </summary>
        /// <param name="config">Instance of CSRConfiguration class that contains configuration</param>
        /// <param name="SaveDir">Path where created csr and private key file should be saved</param>
        /// <param name="CSRFileName">Output csr file name</param>
        /// <param name="KeyFileName">Output private key file name</param>
        /// <returns>Task object that indicates task completion</returns>
        public Task GenerateCSRAsync(CSRConfiguration config, string SaveDir = "", string CSRFileName = "CSRasync.csr", string KeyFileName = "CSRasync.key")
        {
            TaskCompletionSource<object> CSRgenCompletion = new TaskCompletionSource<object>();
            VerifyConfig(config, CSRgenCompletion);
            if (SaveDir == "")
            {
                SaveDir = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                if (!Directory.Exists(SaveDir))
                {
                    Directory.CreateDirectory(SaveDir);
                }
            }


            File.WriteAllTextAsync(SaveDir != AppDomain.CurrentDomain.BaseDirectory ? $"{SaveDir}\\genconfcsr.txt" : "genconfcsr.txt", CreateOpensslCSRConfig(config)).Wait();
            string cmdargs = $"req -new -{config.HashAlgorithm.ToString()} -nodes -newkey rsa:{config.KeyLengthAsNumber} {config.EncodingAsString} -keyout {KeyFileName} -out {CSRFileName} -config genconfcsr.txt";

            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = this._OpenSSLPath;
                openssl.StartInfo.CreateNoWindow = true;
                openssl.StartInfo.Arguments = cmdargs;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.EnableRaisingEvents = true;
                openssl.StartInfo.WorkingDirectory = SaveDir;

                openssl.Exited += (sender, args) =>
                {
                    if (openssl.ExitCode != 0)
                    {
                        string err = openssl.StandardError.ReadToEnd();

                        CSRgenCompletion.TrySetException(new Exceptions.CSRgenFailedException($"Generation Failed with error:{err} "));

                    }
                    else
                    {
                        CSRgenCompletion.SetResult(new object());
                    }
                };
                openssl.Start();
                openssl.WaitForExit();
                File.Delete(SaveDir != AppDomain.CurrentDomain.BaseDirectory ? $"{SaveDir}\\genconfcsr.txt" : "genconfcsr.txt");

            }

            return CSRgenCompletion.Task;

        }



        /// <summary>
        /// Signs certificate signing request
        /// </summary>
        /// <param name="config"></param>
        /// <param name="CSRpath"></param>
        /// <param name="CAPath"></param>
        /// <param name="CAKeyPath"></param>
        /// <param name="CertFileName"></param>
        /// <param name="SaveDir"></param>
        public void SignCSR(SignCSRConfig config, string CSRpath, string CAPath, string CAKeyPath, string CertFileName, string SaveDir = "")
        {
            if (SaveDir == "")
            {
                SaveDir = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                if (!Directory.Exists(SaveDir))
                {
                    Directory.CreateDirectory(SaveDir);
                }
            }

            string copyExtensions = "";
            if (config.copyallextensions == true) { copyExtensions = "-copy_extensions copyall"; }


            File.WriteAllText(SaveDir != AppDomain.CurrentDomain.BaseDirectory ? $"{SaveDir}\\signconf.txt" : "signconf.txt", config.BuildConfFile());
            string command = @$"req -x509 -in {CSRpath} -CA {CAPath} -CAkey {CAKeyPath} -out {CertFileName} -days {config.days} {copyExtensions} -config signconf.txt";
            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = this._OpenSSLPath;
                openssl.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                openssl.StartInfo.CreateNoWindow = true;
                openssl.StartInfo.Arguments = command;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.StartInfo.WorkingDirectory = SaveDir;
                openssl.Start();
                openssl.WaitForExit();

                if (openssl.ExitCode != 0)
                {
                    throw new SignCsrException($"Signing csr failed with Exception: {openssl.StandardError.ReadToEnd()}");
                }
            }
            File.Delete(SaveDir != AppDomain.CurrentDomain.BaseDirectory ? $"{SaveDir}\\signconf.txt" : "signconf.txt");
        }


        /// <summary>
        /// Asynchrounously signs certificate signing request
        /// </summary>
        /// <param name="config"></param>
        /// <param name="CSRpath"></param>
        /// <param name="CAPath"></param>
        /// <param name="CAKeyPath"></param>
        /// <param name="CertFileName"></param>
        /// <param name="SaveDir"></param>
        /// <returns></returns>
        public Task SignCSRAsync(SignCSRConfig config, string CSRpath, string CAPath, string CAKeyPath, string CertFileName, string SaveDir = "")
        {

            TaskCompletionSource<object> SignCompletion = new TaskCompletionSource<object>();

            if (SaveDir == "")
            {
                SaveDir = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                if (!Directory.Exists(SaveDir))
                {
                    Directory.CreateDirectory(SaveDir);
                }
            }

            string copyExtensions = "";
            if (config.copyallextensions == true) { copyExtensions = "-copy_extensions copyall"; }

            File.WriteAllText(SaveDir != AppDomain.CurrentDomain.BaseDirectory ? $"{SaveDir}\\signconf.txt" : "signconf.txt", config.BuildConfFile());
            string command = @$"req -x509 -in {CSRpath} -CA {CAPath} -CAkey {CAKeyPath} -out {CertFileName} -days {config.days} {copyExtensions} -config signconf.txt";


            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = this._OpenSSLPath;
                openssl.StartInfo.CreateNoWindow = true;
                openssl.StartInfo.WorkingDirectory = SaveDir;
                openssl.StartInfo.Arguments = command;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.EnableRaisingEvents = true;

                openssl.Exited += (sender, args) =>
                {
                    if (openssl.ExitCode != 0)
                    {
                        string err = openssl.StandardError.ReadToEnd();
                        SignCompletion.TrySetException(new Exceptions.CACertgenFailedException($"Generation Failed with error:{err} "));
                    }
                    else
                    {
                        SignCompletion.TrySetResult(new object());
                    }
                };
                openssl.Start();
                openssl.WaitForExit();


                File.Delete(SaveDir != AppDomain.CurrentDomain.BaseDirectory ? $"{SaveDir}\\signconf.txt" : "signconf.txt");
            }
            return SignCompletion.Task;

        }




        public void ConvertX509ToPfx(string Certpath, string KeyPath, string Certname, string Password, string SaveDir = "")
        {
            if (SaveDir == "")
            {
                SaveDir = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                if (!Directory.Exists(SaveDir))
                {
                    Directory.CreateDirectory(SaveDir);
                }
            }
            string command = $"pkcs12 -export -out {Certname} -inkey {KeyPath} -in {Certpath} -passout pass:{Password}";
            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = this._OpenSSLPath;
                openssl.StartInfo.CreateNoWindow = true;
                openssl.StartInfo.Arguments = command;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.StartInfo.WorkingDirectory = SaveDir;
                openssl.Start();
                openssl.WaitForExit();
                if (openssl.ExitCode != 0)
                {
                    throw new PFXConvertException($"Converting certificate to pfx failed with openssl error : {openssl.StandardError.ReadToEnd()}");
                }
            }
        }


        public Task ConvertX509ToPfxAsync(string Certpath, string KeyPath, string Certname, string Password, string SaveDir = "")
        {
            TaskCompletionSource<object> convertcompletion = new TaskCompletionSource<object>();

            if (SaveDir == "")
            {
                SaveDir = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                if (!Directory.Exists(SaveDir))
                {
                    Directory.CreateDirectory(SaveDir);
                }
            }

            string command = $"pkcs12 -export -out {Certname} -inkey {KeyPath} -in {Certpath} -passout pass:{Password}";
            using (Process openssl = new Process())
            {
                openssl.StartInfo.FileName = this._OpenSSLPath;
                openssl.StartInfo.CreateNoWindow = true;
                openssl.StartInfo.Arguments = command;
                openssl.StartInfo.RedirectStandardOutput = true;
                openssl.StartInfo.RedirectStandardError = true;
                openssl.EnableRaisingEvents = true;
                openssl.StartInfo.WorkingDirectory = SaveDir;
                openssl.Exited += (sender, args) =>
                {
                    if (openssl.ExitCode != 0)
                    {
                        string err = openssl.StandardError.ReadToEnd();
                        convertcompletion.TrySetException(new PFXConvertException($"Converting certificate to pfx failed with openssl error : {err}"));
                    }
                    else
                    {
                        convertcompletion.TrySetResult(new object());
                    }
                };
                openssl.Start();
                openssl.WaitForExit();
                return convertcompletion.Task;
            }
        }




        #endregion

        #region Config Builders

        string CreateOpensslCaConfig(CaCertgenConfig conf)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(@$"[req]
default_bits= {conf.KeyLengthAsNumber}
prompt = no
default_md = {conf.HashAlgorithm.ToString()}
distinguished_name = dn
[dn]" + "\n");

            if (conf.CountryCode is not null) { builder.Append($"C={conf.CountryCode}\n"); }
            if (conf.CountryState is not null) { builder.Append($"ST={conf.CountryState}\n"); }
            if (conf.Location is not null) { builder.Append($"L={conf.Location}\n"); }
            if (conf.Organization is not null) { builder.Append($"O={conf.Organization}\n"); }
            if (conf.CommonName is not null) { builder.Append($"CN={conf.CommonName}\n"); }

            return builder.ToString();
        }

        string CreateOpensslCSRConfig(CSRConfiguration config)
        {
            string confile = $@"[req]
default_bits={config.KeyLengthAsNumber}
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

            return confile;
        }

        #endregion

        #region Checkers

        private void VerifyConfig(Config conf, TaskCompletionSource<object> tcs)
        {
            if (conf.HashAlgorithm is null)
            {
                tcs.TrySetException(new Exceptions.ConfigurationException("Hash algorithm is not set propertly in configuration class"));
                return;
            }

            if (conf.KeyLength is null || conf.KeyLengthAsNumber is null)
            {
                tcs.TrySetException(new Exceptions.ConfigurationException("Key length is not set correctly in configuration class"));
                return;
            }
        }

        private void VerifyConfig(Config conf)
        {
            if (conf.HashAlgorithm is null)
            {
                throw new Exceptions.ConfigurationException("Hash algorithm is not set propertly in configuration class");
                return;
            }

            if (conf.KeyLength is null || conf.KeyLengthAsNumber is null)
            {
                throw new Exceptions.ConfigurationException("Key length is not set correctly in configuration class");
                return;
            }
        }
        #endregion

    }
}
