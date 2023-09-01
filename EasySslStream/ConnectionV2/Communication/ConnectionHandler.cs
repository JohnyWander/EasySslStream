using EasySslStream.ConnectionV2.Server;
using EasySslStream.ConnectionV2.Server.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Net.Security;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using EasySslStream.ConnectionV2.Communication.TranferTypeConfigs;
using System.Diagnostics;
using EasySslStream.Connection.Client;

namespace EasySslStream.ConnectionV2.Communication
{
    enum SteerCodes
    {
        SendBytes =1,
        SendText = 2,
        SendFile = 3
    }


    public delegate void HandleReceivedBytes(byte[] bytes);
    public delegate void HandleReceivedText(string text);
    public delegate void HandleReceivedFile(string path);

    public class ConnectionHandler : TransformMethods
    {
        private SslStream WorkingStream;
        //private ConnectedClient _ClientCallback;

        private Task Listener;
        private Task Writer;

        CancellationTokenSource cancelHandler = new CancellationTokenSource();
        internal Channel<KeyValuePair<SteerCodes,object>> WriterChannel;

        // Buffers
        byte[] steerbuffer = new byte[16];


        int _transferBufferSize;

        public string DirectorySavePath = AppDomain.CurrentDomain.BaseDirectory;
        public string FileSavePath = AppDomain.CurrentDomain.BaseDirectory;


        internal ConnectionHandler(SslStream stream,int BufferSize,TaskCompletionSource handlerStartedCallback = null) 
        {
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
                        int receivedBytes =await base.ReadBytes(buffer);                      
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
               
                
                }
            }
        }

        private async Task WriterTask(CancellationToken cancel)
        {
            while (!cancel.IsCancellationRequested) {
                await WriterChannel.Reader.WaitToReadAsync(cancel);
                var workpair = await WriterChannel.Reader.ReadAsync(cancel);
                SteerCodes steer = workpair.Key;
                object work = workpair.Value;


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
                }

            }
        }

        #region Send Methods

        public void SendBytes(byte[] bytes)
        {
            this.WriterChannel.Writer.TryWrite(new KeyValuePair<SteerCodes, object>(SteerCodes.SendBytes, bytes));
        }

        public void SendText(string Text, Encoding encoding)
        {
            this.WriterChannel.Writer.TryWrite(new KeyValuePair<SteerCodes, object>(SteerCodes.SendText, new TextTransferWork(encoding, Text)));            
        }

        public void SendFile(string path)
        {
            this.WriterChannel.Writer.TryWrite(new KeyValuePair<SteerCodes, object>(SteerCodes.SendFile, path));
        }

        

        #endregion

        #region Events

        public event HandleReceivedBytes HandleReceivedBytes;
        public event HandleReceivedText HandleReceivedText;
        public event HandleReceivedFile HandleReceivedFile;
        #endregion
    }
}
