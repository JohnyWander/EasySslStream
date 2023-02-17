using EasySslStream;
using EasySslStream.Connection.Full;
using System.Text;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DynamicConfiguration.TransportBufferSize = 4096;
                DynamicConfiguration.EnableDebugMode(DynamicConfiguration.DEBUG_MODE.Console);
                EasySslStream.Connection.Full.Client client = new EasySslStream.Connection.Full.Client();
                client.VerifyCertificateChain = false;
                client.VerifyCertificateName = false;
                client.Connect("127.0.0.1", 10000);

                //Thread.Sleep(12000);
                
                //client.SendFile("86998.zip");

               // Thread.Sleep(20000);

               // client.SendFile("86998.zip");

/*
                IFileReceiveEventAndStats ceas = client.FileReceiveEventAndStats;
                ceas.OnReceiveSpeedChecked += (object sender, EventArgs e) =>
                {
                    Console.WriteLine(ceas.stringSpeed);
                };
                ceas.AutoStartFileReceiveSpeedCheck = true;
                ceas.DefaultIntervalForFileReceiveCheck = 1000;
                ceas.DefaultSpeedUnit = ConnectionCommons.Unit.MBs;
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