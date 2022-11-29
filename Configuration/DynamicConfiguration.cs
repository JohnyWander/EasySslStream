using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EasySslStream
{
    public delegate void DebugLocalVariableEventRaised();

    public static class DynamicConfiguration
    {

        
     
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


 
    


}
