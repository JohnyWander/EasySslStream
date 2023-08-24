using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EasySslStream.Certgen.GenerationClasses.GenerationConfigs;
using EasySslStream.ConnectionV2.Client.Configuration;
namespace EasySslStream.ConnectionV2.Client
{
    public class Client
    {
        public Task RunningClient { private set; get; }

        public readonly IPEndPoint connectToEndpoint;
        public TcpClient client { private set; get; }
        public SslStream sslStream;

        private readonly ClientConfiguration _config;
        public Client(IPEndPoint connectTo,ClientConfiguration config)
        {
            this.connectToEndpoint = connectTo;
            _config = config;


        }

        public Client(string connectToIP, int port,ClientConfiguration config) : this(new IPEndPoint(IPAddress.Parse(connectToIP), port),config) { }

        public void Connect()
        {
            this.RunningClient = Task.Run(() =>
            {
                this.client = new TcpClient(connectToEndpoint.Address.ToString(),connectToEndpoint.Port);
                
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

                while (true)
                {
                    Task.Delay(100).Wait();
                }

            });
        }




    }
}
