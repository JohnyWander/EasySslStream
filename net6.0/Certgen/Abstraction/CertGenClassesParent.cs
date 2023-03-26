using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Abstraction
{


    /// <summary>
    /// Parent class for certificate generation
    ///
    /// </summary>

    public abstract class CertGenClassesParent
    {
        
          protected string? CAHashAlgo;                     /// <summary>Contains name of hash algorithm for CA </summary>
          protected string? CAKeyLength;                    /// <summary>Contains length or RSA keys </summary>
          protected string? CAdays;                         /// <summary>amount of days from now for which the certificate will be valid </summary>
          protected string? CACountry;                      /// <summary>amount of days from now for which the certificate will be valid </summary>
          protected string? CAState;                        /// <summary>Region, State etc.</summary>
          protected string? CALocation;                     /// <summary>Locality, City etc</summary>
          protected string? CAOrganisation;                 /// <summary>Organisation of CA</summary>
          protected string? CACommonName;                   /// <summary>Common name of CA certificate</summary>
          protected string? CAGenerationEncoding;           /// <summary>Encoding of certificate fields</summary>
        public CertGenClassesParent()
        {
          //  LoadCAconfig();
            //Console.WriteLine("parent ctor");

        }
        internal abstract void LoadCAconfig();


        /// <summary>
        /// Base function of parent for generating CA cert
        /// </summary>
        /// <param name="OutputPath">Path for CA.crt,CA.key out</param>
        public abstract void GenerateCA(string OutputPath);

        /// <summary>
        /// Base function of parent for generating CA cert
        /// </summary>
        /// <param name="OutputPath">Path for CA.crt,CA.key out</param>
        /// <returns>Task object for await</returns>
        public abstract Task GenerateCA_Async(string OutputPath);
      
        /// <summary>
        /// Base function for generating csr
        /// </summary>
        /// <param name="config">CSRConfiguration class instance with configuration</param>
        /// <param name="OutputPath">path for .csr and .key</param>
        public abstract void GenerateCSR(CSRConfiguration config,string Filename, string OutputPath);

        /// <summary>
        /// Base function for generating csr
        /// </summary>
        /// <param name="config">CSRConfiguration class instance with configuration</param>
        /// <param name="OutputPath">path for .csr and .key</param>
        public abstract Task GenerateCSRAsync(CSRConfiguration config, string OutputPath);



        /// <summary>
        /// Base of SignCsr function
        /// </summary>
        /// <param name="config">Instance of config class</param>
        /// <param name="CSRpath">Path to csr file</param>
        /// <param name="CAPath">Path to CA certificate</param>
        /// <param name="CAKeyPath">Path to CA private key</param>
        /// <param name="CertName">Name of new Certificate</param>
        /// <param name="Outputpath">Output path for new certificate</param>
        public abstract void SignCSR(SignCSRConfig config, string CSRpath, string CAPath, string CAKeyPath, string CertName,string Outputpath = "default");

        /// <summary>
        /// Base of SignCsr function
        /// </summary>
        /// <param name="config">Instance of config class</param>
        /// <param name="CSRpath">Path to csr file</param>
        /// <param name="CAPath">Path to CA certificate</param>
        /// <param name="CAKeyPath">Path to CA private key</param>
        /// <param name="CertName">Name of new Certificate</param>
        /// <param name="Outputpath">Output path for new certificate</param>
        /// <returns>Task object that indicates task completion </returns>
        public abstract Task SignCSRAsync(SignCSRConfig config, string CSRpath, string CAPath, string CAKeyPath, string CertName, string Outputpath = "default");




        /// <summary>
        /// Base for converting x509 cert to pfx function
        /// </summary>
        /// <param name="Certpath"></param>
        /// <param name="KeyPath"></param>
        /// <param name="Password"></param>
        /// <param name="Certname"></param>
        /// <param name="OutputPath"></param>
        public abstract void ConvertX509ToPfx(string Certpath, string KeyPath, string Password, string Certname, string OutputPath);

        /// <summary>
        /// Base for converting x509 cert to pfx function
        /// </summary>
        /// <param name="Certpath"></param>
        /// <param name="KeyPath"></param>
        /// <param name="Password"></param>
        /// <param name="Certname"></param>
        /// <param name="OutputPath"></param>
        /// <returns> Task object that indicates task completion</returns>
        public abstract Task ConvertX509ToPfxAsync(string Certpath,string KeyPath,string Password,string Certname,string OutputPath);



        


    }
}
