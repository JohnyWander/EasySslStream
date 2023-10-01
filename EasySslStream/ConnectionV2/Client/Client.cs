using EasySslStream.ConnectionV2.Client.Configuration;
using EasySslStream.ConnectionV2.Communication;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace EasySslStream.ConnectionV2.Client
{
    public class Client
    {
        public Task RunningClient { private set; get; }

        public readonly IPEndPoint connectToEndpoint;
        public TcpClient client { private set; get; }
        public SslStream sslStream;

        public ConnectionHandler ConnectionHandler;

        private readonly ClientConfiguration _config;
        public Client(IPEndPoint connectTo, ClientConfiguration config)
        {
            this.connectToEndpoint = connectTo;
            _config = config;
        }

        public Client(string connectToIP, int port, ClientConfiguration config) : this(new IPEndPoint(IPAddress.Parse(connectToIP), port), config) { }

        public Task Connect()
        {
            TaskCompletionSource connectionCompletion = new TaskCompletionSource();
            this.RunningClient = Task.Run(() =>
            {
                this.client = new TcpClient(connectToEndpoint.Address.ToString(), connectToEndpoint.Port);

                this.sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(_config.ValidateServerCertificate));
                this.sslStream.WriteTimeout = -1;
                this.sslStream.ReadTimeout = -1;

                SslClientAuthenticationOptions options = new SslClientAuthenticationOptions();
                options.TargetHost = connectToEndpoint.Address.ToString();
                options.EnabledSslProtocols = this._config.enabledSSLProtocols;
                options.EncryptionPolicy = EncryptionPolicy.RequireEncryption;


                if (this._config.serverVerifiesClient)
                {
                    X509Certificate2 clientCert = new X509Certificate2(this._config.pathToClientPfxCertificate, this._config.certificatePassword, X509KeyStorageFlags.PersistKeySet);
                    X509Certificate2Collection certstore = new X509Certificate2Collection(clientCert);
                    options.ClientCertificates = certstore;
                }

                this.sslStream.AuthenticateAsClient(options);
                this.ConnectionHandler = new ConnectionHandler(this.sslStream, this._config.BufferSize, connectionCompletion);

            });

            return connectionCompletion.Task;
        }

        public void Disconnect()
        {
            this.sslStream.Dispose();
            this.client.Dispose();
        }




    }
}
