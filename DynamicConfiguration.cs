using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
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
        /// <summary>
        /// True - Lib will try to output debug Messages through specified 
        /// </summary>
        static bool DEBUG = false;
        public static DEBUG_MODE debug_mode;
        public static string? debug_message;
        public static string? debug_title;

        public static event DebugLocalVariableEventRaised? DebugLocalVariableEvent;

        private static void RaiseLocalVariableDebugMessage()
        {
            if(DEBUG)
            DebugLocalVariableEvent?.Invoke();
        }
        
        internal static void SetLocalDebugMsg(string Message,string Title)
        {
            
        }
        


        public delegate void RaiseMessageDelegate(string message,string title);
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
                RaiseMessage = void (string message,string Title) =>
                {
                    Console.WriteLine(Title+":\n"+message);
                };
            }

            if (mode == DEBUG_MODE.MessageBox)
            {
                debug_mode |= DEBUG_MODE.MessageBox;
                RaiseMessage = void (string message,string Title) =>
                {
                    Interop.User32.MessageBox((IntPtr)0, message, Title, 0);
                };

            }

           if(mode == DEBUG_MODE.LocalVariable)
            {
                debug_mode = DEBUG_MODE.LocalVariable;
                RaiseMessage = void (string message, string Title) =>
                {
                    debug_message =message;
                    RaiseLocalVariableDebugMessage();
                };

            }
        }

        public static void DisableDebugMode()
        {
            DEBUG = false;
            RaiseMessage = null;
        }



        public static void Save()
        {
            SerializeConfiguration SC = new SerializeConfiguration(SerializeConfiguration.Mode.Save);
           

        }


    }


    /////
    ///

 






}
