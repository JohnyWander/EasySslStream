using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
namespace EasySslStream.Interop
{
    /// <summary>
    /// Contains functions invoked from User32.dll
    /// </summary>
    internal class User32
    {
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr handle, string message,string title, int type);

     

    }
}
