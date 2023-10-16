using EasySslStream.ConnectionV2.Communication.ConnectionSpeed;
using EasySslStream.ConnectionV2.Communication.TranferTypeConfigs;
using System.Net.Security;
using System.Text;
using System.Threading.Channels;

namespace EasySslStream.ConnectionV2.Communication
{
    internal enum SteerCodes
    {
        SendBytes = 1,
        SendText = 2,
        SendFile = 3,
        SendDirectory = 4,
        ReceivedDataPropertly = 1000,
        ReceivedFileFromDiretoryPropertly = 1001
    }


    public delegate void HandleReceivedBytes(byte[] bytes);
    public delegate void HandleReceivedText(string text);
    public delegate void HandleReceivedFile(string path);
    public delegate void HandleReceivedDirectory(string path);

    public class ConnectionHandler : TransferMethods
    {
        
        private SslStream WorkingStream;
        
        private Queue<TaskCompletionSource<object>> AsyncOpsQueue = new Queue<TaskCompletionSource<object>>();

        private Task Listener;
        private Task Writer;

        CancellationTokenSource cancelHandler = new CancellationTokenSource();
        internal Channel<KeyValuePair<SteerCodes, object>> WriterChannel;

        // Buffers
        byte[] steerbuffer = new byte[16];

        
        int _transferBufferSize;

        /// <summary>
        /// Path to save received directory/ies
        /// </summary>
        public string DirectorySavePath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Path to save received files
        /// </summary>
        public string FileSavePath = AppDomain.CurrentDomain.BaseDirectory;

        public TransferSpeedMeasurment Sendspeed;
        public TransferSpeedMeasurment ReceiveSpeed;
        internal CancellationTokenSource cts;
       
        
            
        
        internal ConnectionHandler(SslStream stream, int BufferSize, TaskCompletionSource handlerStartedCallback = null)
        {
            cts = new CancellationTokenSource();
            this.SendSpeed = new TransferSpeedMeasurment(cts.Token);
            this.ReceiveSpeed = new TransferSpeedMeasurment(cts.Token);

            base.SendSpeed = this.SendSpeed;
            base.ReceiveSpeed = this.ReceiveSpeed;

            _transferBufferSize = BufferSize;
            Thread handlerThread = new Thread(() =>
            {
                base.stream = stream;
                this.WorkingStream = stream;
                base._bufferSize = BufferSize;
                WriterChannel = Channel.CreateUnbounded<KeyValuePair<SteerCodes, object>>();
                Listener = Task.Run(() => ListenerTask(cancelHandler.Token));
                Writer = Task.Run(() => WriterTask(cancelHandler.Token));

                if (handlerStartedCallback != null)
                {
                    handlerStartedCallback.TrySetResult();
                }

                Task.WaitAll(Listener, Writer);
            });
            handlerThread.Start();

        }

        private async Task ListenerTask(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {               
                await WorkingStream.ReadAsync(steerbuffer);
                int steercode = BitConverter.ToInt32(steerbuffer, 0);
                SteerCodes steer = (SteerCodes)steercode;

                switch (steer)
                {
                    case SteerCodes.SendBytes:
                        byte[] buffer = new byte[this._transferBufferSize];
                        int receivedBytes = await base.ReadBytes(buffer);
                        this.HandleReceivedBytes?.Invoke((buffer.Take(receivedBytes).ToArray()));
                        break;

                    case SteerCodes.SendText:
                        string receivedString = await base.GetTextAsync(this._transferBufferSize);
                        this.HandleReceivedText?.Invoke(receivedString);
                        break;

                    case SteerCodes.SendFile:

                        string receivedFilePath = await base.GetFileAsync(this.FileSavePath);
                        this.HandleReceivedFile?.Invoke(receivedFilePath);
                        break;

                    case SteerCodes.SendDirectory:

                        string receivedDirectoryPath = await base.GetDirectory(this.DirectorySavePath);
                        this.HandleReceivedDirectory?.Invoke(receivedDirectoryPath);
                        break;

                    case SteerCodes.ReceivedDataPropertly:

                        base.PeerResponseWaiter.SetResult(true);
                        base.PeerResponseWaiter = new TaskCompletionSource<object>();

                        break;
                    case SteerCodes.ReceivedFileFromDiretoryPropertly:
                        base.EndOfDirectroryFileTransmission.SetResult(true);
                        base.EndOfDirectroryFileTransmission = new TaskCompletionSource<object>();

                        break;

                }
            }
        }

        private async Task WriterTask(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested)
            {
                await WriterChannel.Reader.WaitToReadAsync(cancel);
                var workpair = await WriterChannel.Reader.ReadAsync(cancel);
                SteerCodes steer = workpair.Key;
                object work = workpair.Value;

                TaskCompletionSource<object> asyncCompletionHandler = null;
                bool asyncWork = AsyncOpsQueue.TryDequeue(out asyncCompletionHandler);

                switch (steer)
                {
                    case SteerCodes.SendBytes:
                        await base.WriteBytesAsync((byte[])work, steer);                        

                        break;
                    case SteerCodes.SendText:
                        await base.SendTextAsync((TextTransferWork)work, steer);
                        break;

                    case SteerCodes.SendFile:
                        await base.SendFileAsync((string)work, steer);
                        break;

                    case SteerCodes.SendDirectory:
                        await base.SendDirectory((string)work, steer);
                        break;
                }

                if (asyncWork) { asyncCompletionHandler.SetResult(true); }
            }
        }
        

        #region Send Methods

        /// <summary>
        /// Queues sending bytes operation
        /// </summary>
        /// <param name="bytes">Bytes to send</param>
        public void SendBytes(byte[] bytes)
        {
            this.WriterChannel.Writer.TryWrite(new KeyValuePair<SteerCodes, object>(SteerCodes.SendBytes, bytes));
        }

        /// <summary>
        /// Queues sending string operation
        /// </summary>
        /// <param name="Text">Message to send</param>
        /// <param name="encoding">message encoding</param>
        public void SendText(string Text, Encoding encoding)
        {
            this.WriterChannel.Writer.TryWrite(new KeyValuePair<SteerCodes, object>(SteerCodes.SendText, new TextTransferWork(encoding, Text)));
        }

        /// <summary>
        /// Queues sending file operation
        /// </summary>
        /// <param name="path">Path to file to send</param>
        public void SendFile(string path)
        {
            this.WriterChannel.Writer.TryWrite(new KeyValuePair<SteerCodes, object>(SteerCodes.SendFile, path));
        }

        /// <summary>
        /// Queues sending directory operation
        /// </summary>
        /// <param name="path">Path to directory to send</param>
        public void SendDirectory(string path)
        {
            this.WriterChannel.Writer.TryWrite(new KeyValuePair<SteerCodes, object>(SteerCodes.SendDirectory, path));
        }

        /// <summary>
        /// Queues Send byte work and exposes task to await transfer completion
        /// </summary>
        /// <param name="bytes">Bytes to send</param>
        /// <returns></returns>
        public async Task SendBytesAsync(byte[] bytes)
        {
            TaskCompletionSource<object> sendbytesCompletion = new TaskCompletionSource<object>();
            AsyncOpsQueue.Enqueue(sendbytesCompletion);
            this.WriterChannel.Writer.TryWrite(new KeyValuePair<SteerCodes, object>(SteerCodes.SendBytes, bytes));

            await sendbytesCompletion.Task;
        }

        /// <summary>
        /// Queues Send text work and exposes task to await transfer completion
        /// </summary>
        /// <param name="Text">Text to send</param>
        /// <param name="encoding">Text encoding</param>
        /// <returns></returns>
        public async Task SendTextAsync(string Text, Encoding encoding)
        {
            TaskCompletionSource<object> sendtextCompletiopn = new TaskCompletionSource<object>();
            AsyncOpsQueue.Enqueue(sendtextCompletiopn);
            this.WriterChannel.Writer.TryWrite(new KeyValuePair<SteerCodes, object>(SteerCodes.SendText, new TextTransferWork(encoding, Text)));

            await sendtextCompletiopn.Task;
        }

        /// <summary>
        /// Queues Send file work and exposes task to await transfer completion
        /// </summary>
        /// <param name="path">Path to file to send</param>
        /// <returns></returns>
        public async Task SendFileAsync(string path)
        {
            TaskCompletionSource<object> sendFileCompletion = new TaskCompletionSource<object>();
            AsyncOpsQueue.Enqueue(sendFileCompletion);
            this.WriterChannel.Writer.TryWrite(new KeyValuePair<SteerCodes, object>(SteerCodes.SendFile, path));

            await sendFileCompletion.Task;
        }

        /// <summary>
        /// Queues Send directory work and exposes task to await transfer completion
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task SendDirectoryAsync(string path)
        {
            TaskCompletionSource<object> sendDirectoryCompletion = new TaskCompletionSource<object>();
            AsyncOpsQueue.Enqueue(sendDirectoryCompletion);
            this.WriterChannel.Writer.TryWrite(new KeyValuePair<SteerCodes, object>(SteerCodes.SendDirectory, path));

            await sendDirectoryCompletion.Task;
        }


        #endregion

        #region Events
        /// <summary>
        /// Fired when connection handler receives bytes
        /// </summary>
        public event HandleReceivedBytes HandleReceivedBytes;

        /// <summary>
        /// Fired when connection handler receives text message
        /// </summary>
        public event HandleReceivedText HandleReceivedText;

        /// <summary>
        /// Fired when connection handler receives file
        /// </summary>
        public event HandleReceivedFile HandleReceivedFile;

        /// <summary>
        /// Fired when connection handler receives directory
        /// </summary>
        public event HandleReceivedDirectory HandleReceivedDirectory;
        #endregion


        
    }
}
