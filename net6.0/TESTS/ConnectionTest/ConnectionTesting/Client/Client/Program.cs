using EasySslStream;
using EasySslStream.Connection.Full;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Clientx
{
    internal class Program
    {
        static void Main(string[] args)
        {



            EasySslStream.Connection.Full.Client cl = new Client();
            cl.VerifyCertificateChain = false;
            cl.VerifyCertificateName = false;
            cl.Connect("localhost", 5000);

            cl.WriteText(Encoding.UTF8.GetBytes("bcvjh"));
        }
    }
}