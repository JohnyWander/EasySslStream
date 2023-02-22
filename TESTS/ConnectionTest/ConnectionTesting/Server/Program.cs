﻿using EasySslStream;
using EasySslStream.Connection.Full;
using System.Net;
using System.Text;
namespace ConnectionTesting
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DynamicConfiguration.EnableDebugMode(DynamicConfiguration.DEBUG_MODE.Console);
            DynamicConfiguration.TransportBufferSize = 8192;
            Server server = new Server();
            server.CertificateCheckSettings.VerifyCertificateName = false;
            server.CertificateCheckSettings.VerifyCertificateChain = false;

            server.StartServer(IPAddress.Any, 10000, "pfxcert.pfx.pfx", "231", false);

          

            Thread.Sleep(12000); // Waiting for client to connect

             // Server To client ----->>>>>>
            foreach(SSLClient cl in server.ConnectedClients)
            {
                cl.FileReceiveEventAndStats.DefaultReceiveSpeedUnit = ConnectionCommons.Unit.MBs;
                cl.FileReceiveEventAndStats.AutoStartFileReceiveSpeedCheck = true;
                cl.FileReceiveEventAndStats.OnReceiveSpeedChecked += (object sender, EventArgs e) =>
                {
                    Console.WriteLine(cl.FileReceiveEventAndStats.stringReceiveSpeed + "  " +
                        cl.FileReceiveEventAndStats.CurrentReceivedBytes + " / " + cl.FileReceiveEventAndStats.TotalBytesToReceive);
                };
               // cl.WriteText(Encoding.UTF8.GetBytes("Test text message to client from server ćńéé")); //OK

                 Thread.Sleep(2000);

               // cl.SendRawBytes(new byte[] { 0x00, 0x11, 0x12, 0x12, 0x20, 0x21 }); // OK

                // cl.SendFile("Cent.iso"); // large file //OK, works two ways
               // Thread.Sleep(1000);
                //cl.SendDirectory("C:\\TEST");

              
            }
        }
    }
}