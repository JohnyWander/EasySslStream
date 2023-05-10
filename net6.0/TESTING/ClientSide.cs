using EasySslStream.Connection.Full;
using EasySslStream.TESTING.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.TESTING
{
    /// <summary>
    /// Class for client side testing
    /// </summary>
    public class ClientSide
    {

        public Client client;

        /// <summary>
        /// Action delegate which launches when test has something to communnicate, by default it shows messages with Console.WriteLine()
        /// <br></br>You may do anything with the output data, write output to file for example.
        /// </summary>
        public Action<string> CommunnicateResults = (string Message) =>
        {
            Console.WriteLine(Message);
        };


        public ClientSide()
        {

            client = new Client();
        }


        public void CreateClientAndConnect(ClientTestConfig conf)
        {
            try
            {
                client.VerifyCertificateChain = conf.VerifyCertificateChain;
                client.VerifyCertificateName = conf.VerifyCertificateName;

                if (conf.ServerVerifiesClientCertificate)
                {
                    client.Connect(conf.ServerIPstring, conf.ServerPort);
                }
                else
                {
                    client.Connect(conf.ServerIPstring, conf.ServerPort);
                }


            }catch (Exception ex)
            {
            
            
            }
        }









    }
}
