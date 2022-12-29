using System.Text;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {

            EasySslStream.Connection.Full.Client client = new EasySslStream.Connection.Full.Client("127.0.0.1", 10000);
           // client.WriteText(BitConverter.GetBytes(1));
          //  client.WriteText(BitConverter.GetBytes(2));
          //  client.WriteText(BitConverter.GetBytes(1));
           // client.WriteText(BitConverter.GetBytes(2)); client.WriteText(BitConverter.GetBytes(1));
           // client.WriteText(BitConverter.GetBytes(2));

            client.WriteText(Encoding.UTF8.GetBytes("XDDD"));

        }
    }
}