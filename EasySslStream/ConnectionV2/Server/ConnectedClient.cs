using EasySslStream.ConnectionV2.Communication;
using EasySslStream.ConnectionV2.Server.Configuration;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace EasySslStream.ConnectionV2.Server
{

    public class ConnectedClient
    {
        public int ConnectionID;
        TcpClient _client;
        SslStream _stream;

        public ConnectionHandler ConnectionHandler;

        public SslStream sslStream
        {
            get { return _stream; }
            private set { }
        }
        ServerConfiguration _servConf;

        public IPEndPoint clientEndPoint
        {
            get
            {
                return this._client.Client.RemoteEndPoint as IPEndPoint;               
            }
            private set
            {

            }
        }

        private bool ValidateClientCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;

            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch && _servConf.connectionOptions.VerifyDomainName == false)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors && _servConf.connectionOptions.VerifyCertificateChain == false)
            {
                return true;
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                Console.WriteLine("CERT NOV AVAIABLE????");
                return false;
            }
            else
            {
                return false;
            }

        }

        public ConnectedClient(int ID, TcpClient client, X509Certificate2 serverCert, Server srvCallback)
        {
            this._client = client;

            ConnectionID = ID;
            this._servConf = srvCallback._config;
            srvCallback.ConnectedClientsByEndpoint.TryAdd((IPEndPoint)client.Client.RemoteEndPoint, this);
            srvCallback.ConnectedClientsById.TryAdd(ID, this);

            if (this._servConf.connectionOptions.VerifyClientCertificates)
            {
                this._stream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(srvCallback._config.ValidadeClientCert));
            }
            else
            {
                this._stream = new SslStream(client.GetStream(), false);
            }

            SslServerAuthenticationOptions options = new SslServerAuthenticationOptions();
            options.ServerCertificate = serverCert;
            options.EnabledSslProtocols = srvCallback._config.connectionOptions.enabledProtocols;
            options.EncryptionPolicy = EncryptionPolicy.RequireEncryption;

            if (srvCallback._config.connectionOptions.VerifyClientCertificates)
            {
                options.ClientCertificateRequired = true;
            }
            else
            {
                options.ClientCertificateRequired = false;
            }
            this._stream.AuthenticateAsServer(options);
           

            this.ConnectionHandler = new ConnectionHandler(this._stream, this._servConf.BufferSize);

            srvCallback.InvokeClientConnected();

        }


    }
}
