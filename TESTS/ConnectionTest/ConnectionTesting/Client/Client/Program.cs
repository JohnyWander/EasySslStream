using EasySslStream;
using System.Text;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DynamicConfiguration.TransportBufferSize = 4096;
            EasySslStream.Connection.Full.Client client = new EasySslStream.Connection.Full.Client();
            client.VerifyCertificateChain = false;
            client.VerifyCertificateName = false;
            client.Connect("127.0.0.1", 10000);


            // client.WriteText(BitConverter.GetBytes(1));
            //  client.WriteText(BitConverter.GetBytes(2));
            //  client.WriteText(BitConverter.GetBytes(1));
            // client.WriteText(BitConverter.GetBytes(2)); client.WriteText(BitConverter.GetBytes(1));
            // client.WriteText(BitConverter.GetBytes(2));

            client.WriteText(Encoding.UTF8.GetBytes("ooga booga"));
            Thread.Sleep(2000);

            client.SendFile("86998.zip");
            client.SendRawBytes(Encoding.UTF8.GetBytes("wqeweqweq"));

                 // client.WriteText(Encoding.UTF8.GetBytes("ooga booga")); ;
        }
    }
}