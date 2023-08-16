using System.Collections.Concurrent;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;

namespace EasySslStream.Connection.Client
{
    /// <summary>
    /// Server class that can handle multiple clients
    /// </summary>
    public class Server
    {

        public Server(int bufferSize)
        {
            this.bufferSize = bufferSize; 
        }


        #region Options

        public int bufferSize;

        /// <summary>
        /// List that contains connected clients 
        /// </summary>
        public List<SSLClient> ConnectedClients = new List<SSLClient>();

        /// <summary>
        /// Thread safe dictionary that contains connected clients referenced by int
        /// </summary>
        public ConcurrentDictionary<int, SSLClient> ConnectedClientsByNumber = new ConcurrentDictionary<int, SSLClient>();


        /// <summary>
        /// Thread safe dictionary that contains connected clients referenced by string endpoint ( 127.0.0.1:5000 etc)
        /// </summary>
        public ConcurrentDictionary<IPEndPoint, SSLClient> ConnectedClientsByEndPoint = new ConcurrentDictionary<IPEndPoint, SSLClient>();


        private X509Certificate2 serverCert = null;


        private TcpListener listener = null;

        private CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// Encoding for text messages
        /// </summary>
        public Encoding TextReceiveEncoding = Encoding.UTF8;

        /// <summary>
        /// Encoding for filenames
        /// </summary>
        public Encoding FileNameEncoding = Encoding.UTF8;

        /// <summary>
        /// Specifies how certificate verification should behave
        /// </summary>
        public CertificateCheckSettings CertificateCheckSettings = new CertificateCheckSettings();

        /// <summary>
        /// Action Delegate for handling text data received from client, by default it prints message by Console.WriteLine()
        /// </summary>
        public Action<string> HandleReceivedText = (string text) =>
        {
            Console.WriteLine(text);
        };

        /// <summary>
        /// Action Delegate for handling bytes received from client, by default it prints int representation of them in console
        /// </summary>
        public Action<byte[]> HandleReceivedBytes = (byte[] bytes) =>
        {
            foreach (byte b in bytes) { Console.Write(Convert.ToInt32(b) + " "); }
            //return bytes
        };

        /// <summary>
        /// Location for the received file from clients
        /// </summary>
        public string ReceivedFilesLocation = AppDomain.CurrentDomain.BaseDirectory;
        #endregion

        #region Server related
        #region Stopping Server
        // Shutting down the server
        ////////////////////////////////////////////////////////////////////  
        private TaskCompletionSource<object> GentleStopLock = new TaskCompletionSource<object>();
        internal void WorkLock()
        {
            bool NoJobs = true;
            foreach (SSLClient client in ConnectedClients)
            {
                if (client.Busy)
                {
                    NoJobs = false;
                }
            }

            if (NoJobs == true)
            {
                if (!GentleStopLock.Task.IsCompleted)
                    GentleStopLock.SetResult(null);
            }



        }
        

        /// <summary>
        /// Waits for currently running transfers to end, for all connections, then shuts down the server.
        /// </summary>
        public async Task GentleStopServer(int interval = 100)
        {
            if (ConnectedClients.Count != 0)
            {
                Console.WriteLine("Waiting for all jobs to terminate");
                bool loopcancel = true;
                Task.Run(() =>
                {
                    while (loopcancel)
                    {
                        WorkLock();
                        Task.Delay(interval).Wait();
                    }

                });



                await GentleStopLock.Task;

                loopcancel = false;

                Parallel.ForEach(ConnectedClients, SSLClient =>
                {
                    SSLClient.Stop();
                });
                listener.Stop();


            }
            else
            {
                Parallel.ForEach(ConnectedClients, SSLClient =>
                {
                    SSLClient.Stop();
                });
                listener.Stop();
            }

        }




        /// <summary>
        /// Disposes all connected clients and stops server from listening
        /// </summary>
        public void StopServer()
        {
            Parallel.ForEach(ConnectedClients, SSLClient =>
            {
                SSLClient.Stop();
            });

            this.listener.Stop();
        }
        #endregion
        #region Starting server
        /// <summary>
        /// Starts server
        /// </summary>
        /// <param name="ListenOnIp">Listening ip</param>
        /// <param name="port">Listening port</param>
        /// <param name="ServerPFXCertificatePath">Path to the Certificate with private key in pfx format</param>
        /// <param name="CertPassword">Password to the certificate use empty string if there's no password</param>
        /// <param name="VerifyClients">Set true if server is meant to check for client certificate, otherwise set false</param>
        public void StartServer(string ListenOnIp, int port, string ServerPFXCertificatePath, string CertPassword, bool VerifyClients)
        {

            serverCert = new X509Certificate2(ServerPFXCertificatePath, CertPassword, X509KeyStorageFlags.PersistKeySet);
            listener = new TcpListener(IPAddress.Parse(ListenOnIp), port);
            Thread listenerThread = new Thread(() =>
            {
                listener.Start();
                int connected = 0;
                while (listener.Server.IsBound)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    SSLClient connection = new SSLClient(client, serverCert, VerifyClients, this);                    
                }
                connected++;
            });
            listenerThread.Start();         
            
            
        }

        /// <summary>
        /// Starts server
        /// </summary>
        /// <param name="ListenOnIp">Listening ip</param>
        /// <param name="port">Listening port</param>
        /// <param name="ServerPFXCertificatePath">Path to the Certificate with private key in pfx format</param>
        /// <param name="CertPassword">Password to the certificate use empty string if there's no password</param>
        /// <param name="VerifyClients">Set true if server is meant to check for client certificate, otherwise set false</param>
        public void StartServer(IPAddress ListenOnIp, int port, string ServerPFXCertificatePath, string CertPassword, bool VerifyClients)
        {

            this.serverCert = new X509Certificate2(ServerPFXCertificatePath, CertPassword, X509KeyStorageFlags.PersistKeySet);
            listener = new TcpListener(ListenOnIp, port);
            listener.Start();
            Thread listenerThrewad = new Thread(() =>
            {
                int connected = 0;
                while (listener.Server.IsBound)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    SSLClient connection = new SSLClient(client, serverCert, VerifyClients, this);
                }
                connected++;
            });
            listenerThrewad.Start();
            
        }
        #endregion

        #endregion

        #region Send methods
        /// <summary>
        /// Sends text Message to client
        /// </summary>
        /// <param name="clientEndpoint">Client endpoint</param>
        /// <param name="Message">byte array representation of the message</param>
        public void WriteTextToClient(IPEndPoint clientEndpoint, byte[] Message)
        {
            ConnectedClientsByEndPoint[clientEndpoint].WriteText(Message);
        }

        /// <summary>
        /// Sends text Message to client
        /// </summary>
        /// <param name="ConnectionID"></param>
        /// <param name="Message"></param>
        public void WriteTextToClient(int ConnectionID, byte[] Message)
        {
            ConnectedClients[ConnectionID].WriteText(Message);
        }

        public void SendRawBytesToClient(int ConnectionID, byte[] Message)
        {
            ConnectedClients[ConnectionID].SendRawBytes(Message);
        }

        public void SendRawBytesToClient(IPEndPoint clientEndpoint, byte[] Message)
        {
            ConnectedClientsByEndPoint[clientEndpoint].SendRawBytes(Message);
        }

        /// <summary>
        /// Sends file to client
        /// </summary>
        /// <param name="ConnectionID">Id of connection</param>
        /// <param name="Path">path to the file to send</param>
        public void WriteFileToClient(int ConnectionID, string Path)
        {
            ConnectedClients[ConnectionID].SendFile(Path);
        }

        /// <summary>
        /// Sends file to client
        /// </summary>
        /// <param name="clientEndpoint">client endpoint</param>
        /// <param name="Path">path to the file to send</param>
        public void WriteFileToClient(IPEndPoint clientEndpoint, string Path)
        {
            ConnectedClientsByEndPoint[clientEndpoint].SendFile(Path);
        }

        /// <summary>
        /// Sends Directory to client
        /// </summary>
        /// <param name="ConnectionID">Connection id</param>
        /// <param name="Path">Path to directory to send</param>
        /// <param name="StopAndThrowOnFailedTransfer">Stops transfer ic coulndn't read the file,if true.If false ignores any errors</param>
        /// <param name="FailSafeInterval">If connection crashes try to raise this value</param>
        public void SendDirectoryToClient(int ConnectionID, string Path, bool StopAndThrowOnFailedTransfer = true, int FailSafeInterval = 20)
        {
            ConnectedClientsByNumber[ConnectionID].SendDirectory(Path, StopAndThrowOnFailedTransfer, FailSafeInterval);
        }

        /// <summary>
        /// Sends Directory to client
        /// </summary>
        /// <param name="clientEndPoint">Client endpoint</param>
        /// <param name="Path">Path to directory to send</param>
        /// <param name="StopAndThrowOnFailedTransfer">Stops transfer ic coulndn't read the file,if true.If false ignores any errors</param>
        /// <param name="FailSafeInterval">If connection crashes try to raise this value</param>
        public void SendDirectoryToClient(IPEndPoint clientEndPoint, string Path, bool StopAndThrowOnFailedTransfer = true, int FailSafeInterval = 20)
        {
            ConnectedClientsByEndPoint[clientEndPoint].SendDirectory(Path, StopAndThrowOnFailedTransfer, FailSafeInterval);
        }

        /// <summary>
        /// Optimized - Sends directory to client
        /// </summary>
        /// <param name="ConnectionID"></param>
        /// <param name="Path"></param>
        /// <param name="StopAndThrowOnFailedTransfer"></param>
        /// <param name="FailSafeInterval"></param>
        public void SendDirectoryToClientV2(int ConnectionID, string Path, bool StopAndThrowOnFailedTransfer = true, int FailSafeInterval = 20)
        {
            ConnectedClientsByNumber[ConnectionID].SendDirectoryV2(Path, StopAndThrowOnFailedTransfer, FailSafeInterval);
        }

        public void SendDirectoryToClientV2(IPEndPoint clientEndPoint, string Path, bool StopAndThrowOnFailedTransfer = true, int FailSafeInterval = 20)
        {
            ConnectedClientsByEndPoint[clientEndPoint].SendDirectoryV2(Path, StopAndThrowOnFailedTransfer, FailSafeInterval);
        }

        #endregion

    }

   

}