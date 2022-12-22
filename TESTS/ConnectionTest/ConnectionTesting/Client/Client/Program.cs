namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {

            EasySslStream.Connection.Full.Client client = new EasySslStream.Connection.Full.Client("127.0.0.1", 10000);
            client.write(BitConverter.GetBytes(1));
            client.write(BitConverter.GetBytes(2));
            client.write(BitConverter.GetBytes(1));
            client.write(BitConverter.GetBytes(2)); client.write(BitConverter.GetBytes(1));
            client.write(BitConverter.GetBytes(2));

        }
    }
}