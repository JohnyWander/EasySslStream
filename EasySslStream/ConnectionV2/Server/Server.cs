using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EasySslStream.ConnectionV2.Server.Configuration;

namespace EasySslStream.ConnectionV2.Server
{
    public delegate void ServerEvent();

    public class Server
    {
        private IPEndPoint _serverEndpoint;
        private ServerConfiguration _serverConfiguration;
        private X509Certificate2 _serverCertificate;
        private TcpListener _tcpListener;

        
        public Task RunningServerListener { get; private set; }
        internal readonly ServerConfiguration _config;




        #region Constructors
        public Server(IPEndPoint serverEndpoint,ServerConfiguration config)
        {
            this._serverEndpoint = serverEndpoint;
        }

        public Server(string hostOnIP, int port,ServerConfiguration config) : this(new IPEndPoint(IPAddress.Parse(hostOnIP), port), config) { }
        #endregion

        
        public void StartServer(string pathToPfxCertificate,string certPassword)
        {
            this._serverCertificate = new X509Certificate2(pathToPfxCertificate,certPassword,X509KeyStorageFlags.PersistKeySet);
            this._tcpListener = new TcpListener(this._serverEndpoint);

            this.RunningServerListener = Task.Run(async () =>
            {
                this._tcpListener.Start();
                int id = 0;
                while (_tcpListener.Server.IsBound)
                {
                    TcpClient client = await _tcpListener.AcceptTcpClientAsync();                    
                    ConnectedClient connection = new ConnectedClient(id,client, this._serverCertificate,this) ;
                    ClientConnected.Invoke();
                    id++;
                }

            });


        }


        public void StopServer()
        {
            this._tcpListener.Stop();           
        }

        #region Events

        public ServerEvent ClientConnected;


        #endregion

        #region ConnectedClients

        public IDictionary<int,ConnectedClient> ConnectedClientsById = new Dictionary<int, ConnectedClient>();
        public IDictionary<IPEndPoint,ConnectedClient> ConnectedClientsByEndpoint = new Dictionary<IPEndPoint, ConnectedClient>();


        #endregion

    }
}
