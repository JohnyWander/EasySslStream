using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.ConnectionV2.Server
{
    public class ConnectedClient
    {
        TcpClient _client;
        SslStream _stream;

        public ConnectedClient(TcpClient client, X509Certificate2 serverCert, Server srvCallback)
        {
            srvCallback.ConnectedClientsByEndpoint.Add((IPEndPoint)client.Client.RemoteEndPoint, this);

            if (srvCallback._config.authOptions.VerifyClientCertificates)
            {
                this
            }

        }
    }
}
