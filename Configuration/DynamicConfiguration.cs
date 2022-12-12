using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using static EasySslStream.CA_CertGen;

namespace EasySslStream
{
    public delegate void DebugLocalVariableEventRaised();

    public static class DynamicConfiguration
    {
        public static CA_CertGen CA_CONFIG = new CA_CertGen();
        public static OpenSSLConfig_ OpenSSl_config = new OpenSSLConfig_();
     
        public enum DEBUG_MODE
        {
            Console,
            MessageBox,
            LocalVariable
        }

        public static DEBUG_MODE debug_mode;

        public enum SSL_Certgen_mode
        {
            OpenSSL,
            Makecer,
            DotNet

        }

        public static SSL_Certgen_mode Certgen_Mode { private set; get; }

        /// <summary>
        /// True - Lib will try to output debug Messages through specified 
        /// </summary>
        public static bool DEBUG = false;
     
        public static string? debug_message;
        public static string? debug_title;

        public static event DebugLocalVariableEventRaised? DebugLocalVariableEvent;

        private static void RaiseLocalVariableDebugMessage()
        {
            if (DEBUG)
                DebugLocalVariableEvent?.Invoke();
        }

        internal static void SetLocalDebugMsg(string Message, string Title)
        {

        }



        public delegate void RaiseMessageDelegate(string message, string title);
        public static RaiseMessageDelegate? RaiseMessage;


        /// <summary>
        /// Enables debug mode
        /// </summary>
        /// <param name="mode">Optional parameter - By default debug will show by Console.WriteLine, it can be changed here</param>
        public static void EnableDebugMode(DEBUG_MODE mode = DEBUG_MODE.Console)
        {
            DEBUG = true;

            if (mode == DEBUG_MODE.Console)
            {
                debug_mode = DEBUG_MODE.Console;
                RaiseMessage = void (string message, string Title) =>
                {
                    Console.WriteLine(Title + ":\n" + message);
                };
            }

            if (mode == DEBUG_MODE.MessageBox)
            {
                debug_mode |= DEBUG_MODE.MessageBox;
                RaiseMessage = void (string message, string Title) =>
                {
                    Interop.User32.MessageBox((IntPtr)0, message, Title, 0);
                };

            }

            if (mode == DEBUG_MODE.LocalVariable)
            {
                debug_mode = DEBUG_MODE.LocalVariable;
                RaiseMessage = void (string message, string Title) =>
                {
                    debug_message = message;
                    RaiseLocalVariableDebugMessage();
                };

            }
        }

        public static void DisableDebugMode()
        {
            DEBUG = false;
            RaiseMessage = null;
        }

       
        public static void SelectCertgenMode(SSL_Certgen_mode CertGenMode_)
        {
            Certgen_Mode = CertGenMode_;
        }
        


    }

    public class CA_CertGen
    {
        public enum HashAlgorithms
        {
          sha256,
          sha384
        }
        
        public enum KeyLengths
        {
            RSA_1024,
            RSA_2048,
            RSA_4096
        }

        public HashAlgorithms? HashAlgorithm;
        public KeyLengths? KeyLength;

        public int Days { internal get; set; } // ex. 356

        internal string? CountryCodeString;
        public string? CountryCode
        {
            internal get
            {
                if (CountryCode is not null)
                {
                    return CountryCodeString;
                }
                else
                {
                    throw new Exceptions.CountryCodeInvalidException("CountryCode is NULL");
                }
            }
            set
            {
                int length;
                if (value is null) { throw new Exceptions.CountryCodeInvalidException("Passed value is NULL"); }
                if (VerifyCountryCode(value, out length)) { CountryCodeString = value?.ToUpper(); }
                else { throw new Exceptions.CountryCodeInvalidException(length); }

            }
        }
        public string? CountryState { internal get; set; } 
        public string? Location { internal get; set; } 
        public string? Organisation { internal get; set; }
        public string? CommonName { internal get; set; }

        private bool VerifyCountryCode(string CountryCode, out int length)
        {
            length = CountryCode.Length;
            return CountryCode.Length == 2 ? true : false;
        }
    }


    public class Server_CertGen
    {
        public enum HashAlgorithms
        {
            sha256,
            sha384
        }

        public enum KeyLengths
        {
            RSA_1024,
            RSA_2048,
            RSA_4096
        }

        public HashAlgorithms HashAlgorithm;
        public KeyLengths KeyLength;

        internal string? CountryCodeString;
        public string? CountryCode
        {
            internal get
            {
                if (CountryCode is not null)
                {
                    return CountryCodeString;
                }
                else
                {
                    throw new Exceptions.CountryCodeInvalidException("CountryCode is NULL");
                }
            }
            set
            {
                int length;
                if (value is null) { throw new Exceptions.CountryCodeInvalidException("Passed value is NULL"); }
                if (VerifyCountryCode(value, out length)) { CountryCodeString = value?.ToUpper(); }
                else { throw new Exceptions.CountryCodeInvalidException(length); }

            }
        }
        public string? CountryState { internal get; set; }
        public string? City { internal get; set; }
        public string? Institution { internal get; set; }


        private bool VerifyCountryCode(string CountryCode,out int length)
        {
            length = CountryCode.Length;
            return CountryCode.Length == 2 ? true : false;
        }

    }
 
    public class OpenSSLConfig_
    {

        public OpenSSLConfig_()
        {
            TryToFindOpenSSl();
        }
        public enum Architecture
        {
            x32,
            x64
        }

        public Architecture OpensslArch;

        public string OpenSSL_PATH { private set; get; }


        public void SetOpenSSl_PATH(string path)
        {
            OpenSSL_PATH = path;
            
        }


        public void TryToFindOpenSSl()
        {
            
            string pathx32 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)+"\\OpenSSL\\bin";
            string pathx64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\OpenSSL\\bin";
            DynamicConfiguration.RaiseMessage?.Invoke("Failed to find openSSL", "OpenSSL error");

            if (File.Exists(pathx32 + "\\openssl.exe"))
            {
                OpenSSL_PATH = pathx32;
            }

            if (File.Exists(pathx64 + "\\openssl.exe"))
            {
                OpenSSL_PATH = pathx64;
            }
        }
        
        
    }


}
