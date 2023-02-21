using EasySslStream;
using EasySslStream.Connection.Full;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DynamicConfiguration.TransportBufferSize = 8192;
                DynamicConfiguration.EnableDebugMode(DynamicConfiguration.DEBUG_MODE.Console);
                EasySslStream.Connection.Full.Client client = new EasySslStream.Connection.Full.Client();
                client.VerifyCertificateChain = false;
                client.VerifyCertificateName = false;
                client.Connect("127.0.0.1", 10000);

                client.FileReceiveEventAndStats.DefaultReceiveSpeedUnit = ConnectionCommons.Unit.MBs;
                client.FileReceiveEventAndStats.AutoStartFileReceiveSpeedCheck = true;
                client.FileReceiveEventAndStats.OnReceiveSpeedChecked += (object sender, EventArgs e) =>
              {
                    Console.WriteLine(client.FileReceiveEventAndStats.stringReceiveSpeed + "  " +
                        client.FileReceiveEventAndStats.CurrentReceivedBytes + " / " + client.FileReceiveEventAndStats.TotalBytesToReceive);
               };

                //client.SendFile("x.zip");

               // client.FileSendEventAndStats.AutoStartFileSendSpeedCheck = true ;
               // client.FileSendEventAndStats.OnSendSpeedChecked += (object sender, EventArgs e) =>
              //  {
               //     Console.WriteLine(client.FileSendEventAndStats.stringSendSpeed);
               // };

               // Thread.Sleep(12000);

               // client.WriteText(Encoding.UTF8.GetBytes("Test text message to client from server éééę"));

            //    Thread.Sleep(2000);

               // client.SendRawBytes(new byte[] { 0x00, 0x11, 0x12, 0x12, 0x20, 0x21 });
             //   client.SendDirectory("TEST2");














               // client.SendFile("86998.zip");

                // Thread.Sleep(20000);

                // client.SendFile("86998.zip");

                /*

                */
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();

            }

            // client.WriteText(BitConverter.GetBytes(1));
            //  client.WriteText(BitConverter.GetBytes(2));
            //  client.WriteText(BitConverter.GetBytes(1));
            // client.WriteText(BitConverter.GetBytes(2)); client.WriteText(BitConverter.GetBytes(1));
            // client.WriteText(BitConverter.GetBytes(2));

            //client.WriteText(Encoding.UTF8.GetBytes("ooga booga"));
            //Thread.Sleep(2000);

           // client.SendFile("86998.zip");
           // client.SendRawBytes(Encoding.UTF8.GetBytes("wqeweqweq"));

           // client.GentleDisconnect(true);

                 // client.WriteText(Encoding.UTF8.GetBytes("ooga booga")); ;
        }
    }
}