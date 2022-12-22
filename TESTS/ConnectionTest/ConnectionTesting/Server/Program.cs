using System.Net;

namespace ConnectionTesting
{
    internal class Program
    {
        static void Main(string[] args)
        {

            EasySslStream.Connection.Full.Server srv = new EasySslStream.Connection.Full.Server(IPAddress.Any, 10000, "pfxcert.pfx.pfx","231", false);


        }
    }
}