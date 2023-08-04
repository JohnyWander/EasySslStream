using EasySslStream.Certgen.GenerationClasses.GenerationConfigs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EasySslStream.SignCSRConfig;

namespace EasySslStream
{
    /// <summary>
    /// Class that contains configuration for CSR signing with OpenSSL
    /// </summary>
    public class SignCSRConfig : Config
    {
        public enum authorityKeyIdentifiers
        {
            keyid,
            issuer,
            keyid_and_issuer
        }

        public enum basicConstrains{
            CATrue,
            CAFalse,
            pathlen
            
        }

        public enum KeyUsage
        {
            digitalSignature,
            nonRepudiation,
            keyEncipherment,
            dataEncipherment,
            keyAgreement,
            keyCertSign,
            cRLSign,
            encipherOnly,
            decipherOnly
        }

         public enum ExtendedKeyUsage
        {
            serverAuth,
            clientAuth,
            codeSigning,
            emailProtection,
            timeStamping,
            msCodeInd,
            msCodeCom,
            msCTLSign,
            msSGC,
            msEFS,
            nsSGC
        }

        public enum AltNames
        {
            DNS,
            IP,
            email,
            URI,
            RID,
            otherName

        }

        public enum DefaultConfigs
        {
            Enduser,
            Server
        }

        public string Filename;

        public int days;
        public bool copyallextensions = false;


        public List<string> authorityKeyIdentifiersList { private set; get; } = new List<string>();
        public List<string> basicConstrainsList { private set; get; } = new List<string>();

        public List<string> keyUsageList { private set; get; } = new List<string>();

        public List<string> ExtendedkeyUsageList { private set; get; } = new List<string>();

        public List<KeyValuePair<AltNames,string>> subjectAltNamesList = new List<KeyValuePair<AltNames,string>>();

        public void SetAuthorityKeyIdentifiers(authorityKeyIdentifiers AuthoritykeyIdentifier)
        {
            if (AuthoritykeyIdentifier == authorityKeyIdentifiers.keyid || AuthoritykeyIdentifier == authorityKeyIdentifiers.issuer)
            {
                authorityKeyIdentifiersList.Add(AuthoritykeyIdentifier.ToString());
            }
            else
            {
                authorityKeyIdentifiersList.Add(authorityKeyIdentifiers.keyid.ToString()); authorityKeyIdentifiersList.Add(authorityKeyIdentifiers.issuer.ToString());
            }
        }
        public void SetAuthorityKeyIdentifiers(authorityKeyIdentifiers[] AuthorityKeyIdentifiers)
        {
            foreach(authorityKeyIdentifiers aki in AuthorityKeyIdentifiers)
            {
                if(aki != authorityKeyIdentifiers.keyid_and_issuer)
                {
                    authorityKeyIdentifiersList.Add(aki.ToString());
                }
                else
                {
                    authorityKeyIdentifiersList.Add(authorityKeyIdentifiers.keyid.ToString()); authorityKeyIdentifiersList.Add(authorityKeyIdentifiers.issuer.ToString());
                }


            }
        }

        public void SetBasicConstrainsList(basicConstrains[] basicConstrains_)
        {
            foreach(basicConstrains bc in basicConstrains_)
            {
                if (bc != basicConstrains.pathlen)
                {
                    basicConstrainsList.Add(bc.ToString());
                }
                else if(bc== basicConstrains.pathlen)
                {
                    throw new Exceptions.SignCSRConfigurationException("Can't add pathlen without int argument, use overload with int argument");
                }
            }
        }
        public void SetBasicConstrainsList(basicConstrains basicConstrains_)
        {

            if (basicConstrains_ != basicConstrains.pathlen)
            {
                if (basicConstrains_ == basicConstrains.CATrue)
                {
                    basicConstrainsList.Add("CA:TRUE");
                }
                else
                {
                    basicConstrainsList.Add("CA:FALSE");
                }

            }
            else
            {
                throw new Exceptions.SignCSRConfigurationException("Can't add pathlen without int argument, use overload with int argument");
            }


        }

        public void SetBasicConstrainsList(basicConstrains basicConstrains_,int len)
        {

            if (basicConstrains_ != basicConstrains.pathlen)
            {
                if (basicConstrains_ == basicConstrains.CATrue)
                {
                    basicConstrainsList.Add("CA:TRUE");
                }
                else
                {
                    basicConstrainsList.Add("CA:FALSE");
                }

            }
            else
            {
                basicConstrainsList.Add("pathlen:" + len);
            }


        }

        public void SetBasicConstrainsList(basicConstrains[] basicConstrains_, int len)
        {
            foreach (basicConstrains bc in basicConstrains_)
            {
                if (bc != basicConstrains.pathlen)
                {
                    if (bc == basicConstrains.CATrue)
                    {
                        basicConstrainsList.Add("CA:TRUE");
                    }
                    else
                    {
                        basicConstrainsList.Add("CA:FALSE");
                    }

                }
                else if (bc == basicConstrains.pathlen)
                {
                    basicConstrainsList.Add("pathlen:" + len);
                }
            }
        }


        public void SetKeyUsageList(KeyUsage[] keyusage)
        {
            foreach(KeyUsage ku in keyusage)
            {
                keyUsageList.Add(ku.ToString());
            }
        }

        public void SetExtendedKeyUsage(ExtendedKeyUsage[] extendedkeyusage)
        {
            foreach(ExtendedKeyUsage eku in extendedkeyusage)
            {
                keyUsageList.Add(eku.ToString());
            }

        }

        public void AddAltName(AltNames altname,string name)
        {
            if(altname == AltNames.DNS)
            {
                subjectAltNamesList.Add(new KeyValuePair<AltNames,string>(altname,name));
            }
            else
            {
                throw new NotImplementedException();
            }


        }

        /// <summary>
        /// Sets default configuration values of CSR signing to specified type.
        /// </summary>
        /// <param name="conf"></param>
        public void SetDefaultConfig(DefaultConfigs conf)
        {
            switch (conf)
            {
                case DefaultConfigs.Enduser:
                    SetAuthorityKeyIdentifiers(authorityKeyIdentifiers.keyid_and_issuer);
                    SetBasicConstrainsList(basicConstrains.CAFalse);
                    SetKeyUsageList(new KeyUsage[] {
                        KeyUsage.digitalSignature,
                        KeyUsage.nonRepudiation,
                        KeyUsage.keyEncipherment,
                        KeyUsage.dataEncipherment
                    });
                    days = 365;
                    copyallextensions = true;
                                       
               break;
                


            }
        }





        internal string BuildConfFile()
        {
            StringBuilder confile = new StringBuilder();
            if (authorityKeyIdentifiersList.Count > 0)
            {
                confile.Append("authorityKeyIdentifier = ");
                string aki_string = "";
                foreach (var aki in authorityKeyIdentifiersList)
                {
                    aki_string += aki + ",";
                }
                aki_string = aki_string.Trim(',');
                confile.Append(aki_string+"\n");
            }
            ////////////////////////////////////////////
            if (basicConstrainsList.Count > 0)
            {
                confile.Append("basicConstraints = ");
                string constrains_string = "";
                foreach(var basicConstrains in basicConstrainsList)
                {
                    constrains_string += basicConstrains + ",";
                }
                constrains_string = constrains_string.Trim(',');
                confile.Append(constrains_string+"\n");
            }
            ////////////////////////////////////////////
            if(keyUsageList.Count > 0)
            {
                confile.Append("keyUsage = ");
                string keyusage_string = "";
                foreach(string keyusage in keyUsageList)
                {
                    keyusage_string += keyusage + ",";
                }
                keyusage_string = keyusage_string.Trim(',');
                confile.Append(keyusage_string+'\n');
            }
            ///////////////////////////////////////////
            if(ExtendedkeyUsageList.Count > 0)
            {
                confile.Append("extendedKeyUsage");
                string extendedkeyusage = "";
                foreach(string ekeysuage in ExtendedkeyUsageList)
                {
                    extendedkeyusage += ekeysuage + ",";
                }
                extendedkeyusage=extendedkeyusage.Trim(',');
                confile.Append(extendedkeyusage+'\n');
            }
            ///////////////////////////////////////////
            if(subjectAltNamesList.Count > 0)
            {
                foreach (KeyValuePair<AltNames, string> altn in subjectAltNamesList)
                {
                    string alttype = altn.Key.ToString();
                    string altname = altn.Value;

                    confile.AppendLine($"subjectAltName={alttype}:{altname}");
;




                }
               
            }


            return confile.ToString();


        }


    }
}
