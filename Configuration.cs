using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace EasySslStream
{
    public static class Configuration
    {
        /// <summary>
        /// True - Lib will try to output debug Messages through specified 
        /// </summary>
        static bool DEBUG = false;

        public  enum DEBUG_MODE
        {
            Console,
            MessageBox,
            LocalVariable


        }

        /// <summary>
        /// Enables debug mode
        /// </summary>
        /// <param name="mode">Optional parameter - By default debug will show by Console.WriteLine, it can be changed here</param>
        public static void EnableDebugMode(DEBUG_MODE mode = DEBUG_MODE.Console)
        {

            if(DEBUG_MODE == DEBUG_MODE.Console)
            CommunicationWithEntry.DebugMessage = void(string message) =>
            {

            };
          
           
           





        }





        private static void x(string x)
        {
            Console.WriteLine("XD");
        }






    }
}
