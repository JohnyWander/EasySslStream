using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySslStream;
using EasySslStream.ConnectionV2.Client;

namespace TestClient
{
    internal class ClientHandler
    {
        public string FileSavePath;
        public string DirectorySavePath;
        public string ConnectTo
        {
            private get { return ip + port; }
            set
            {
                string[] split = value.Split(':');
            }
        }
        string ip;
        int port;


        bool ServerVerifiesCerts = false;
        public string CertificatePath;
        public string CertificatePassword;
        

        public void StartClient()
        {
            string ip;
          //  Client client = new Client()
        }



    
    }
}
