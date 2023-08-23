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
        public int ConnectionID;
        TcpClient _client;
        SslStream _stream;

        public ConnectedClient(int ID,TcpClient client, X509Certificate2 serverCert, Server srvCallback)
        {
            ConnectionID = ID;
            srvCallback.ConnectedClientsByEndpoint.TryAdd((IPEndPoint)client.Client.RemoteEndPoint, this);
            srvCallback.ConnectedClientsById.TryAdd(ID, this);

            this._stream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(srvCallback._config.ValidadeClientCert));
            
            SslServerAuthenticationOptions options = new SslServerAuthenticationOptions();
            options.ServerCertificate = serverCert;
            options.EnabledSslProtocols = srvCallback._config.enabledSSLProtocols;
            options.EncryptionPolicy = EncryptionPolicy.RequireEncryption;

            if (srvCallback._config.authOptions.VerifyClientCertificates)
            {
                options.ClientCertificateRequired = true;
            }
            else
            {
                options.ClientCertificateRequired = false;
            }
            this._stream.AuthenticateAsServer(options);
            
        }
    }
}
