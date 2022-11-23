using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace EasySslStream
{

    public delegate void MessageSet();
    public static class Configuration
    {
        public static event MessageSet MessageSet;
     
        /// <summary>
        /// True - Lib will try to output debug Messages through specified 
        /// </summary>
        static bool DEBUG = false;

        public static string DebugMessage { set { OnDebugMessageSet(); } }

        public  enum DEBUG_MODE
        {
            Console,
            MessageBox,
            LocalVariable


        }


       private static void OnDebugMessageSet()
        {
            if () { }
            MessageSet?.Invoke();
        }


        /// <summary>
        /// Enables debug mode
        /// </summary>
        /// <param name="mode">Optional parameter - By default debug will show by Console.WriteLine, it can be changed here</param>
        public static void EnableDebugMode(DEBUG_MODE mode = DEBUG_MODE.Console)
        {

            if (mode == DEBUG_MODE.Console)
            {
                CommunicationWithEntry.DebugMessage = void (string message,string Title) =>
                {
                    Console.WriteLine(Title+":\n"+message);
                };
            }

            if (mode == DEBUG_MODE.MessageBox)
            {
                CommunicationWithEntry.DebugMessage = void (string message,string Title) =>
                {
                    Interop.User32.MessageBox((IntPtr)0, message, Title, 0);
                };

            }

           if(mode == DEBUG_MODE.LocalVariable)
            {


            }
           





        }


    }


    /////
    ///

 






}
