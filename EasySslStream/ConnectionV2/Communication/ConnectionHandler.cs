using EasySslStream.ConnectionV2.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EasySslStream.ConnectionV2.Communication
{
    enum SteerCodes
    {
        SendBytes
    }


    public delegate void HandleReceivedBytes(byte[] bytes);

    public class ConnectionHandler : TransformMethods
    {
        private SslStream WorkingStream;
        private ConnectedClient _ClientCallback;

        private Task Listener;
        private Task Writer;

        CancellationTokenSource cancelHandler = new CancellationTokenSource();
        internal Channel<KeyValuePair<SteerCodes,object>> WriterChannel;

        byte[] steerbuffer = new byte[16];

        
        

        internal ConnectionHandler(SslStream stream,TaskCompletionSource handlerStartedCallback = null) 
        {
            Thread handlerThread = new Thread(() =>
            {
                base.stream = stream;
                this.WorkingStream = stream;
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

                        byte[] buffer = new byte[4096];
                        int received =await base.ReadBytes(buffer);

                        this.HandleReceivedBytes?.Invoke((buffer.Take(received).ToArray()));
                        

                    break;
               
                
                }
            }
        }

        private async Task WriterTask(CancellationToken cancel)
        {
            await WriterChannel.Reader.WaitToReadAsync(cancel);
            var workpair = await WriterChannel.Reader.ReadAsync(cancel);
            SteerCodes steer = workpair.Key;
            object work = workpair.Value;


            switch (steer)
            {
                case SteerCodes.SendBytes:
                    base.WriteBytesAsync((byte[])work,steer);
                    break;
            }

        }

        #region Send Methods

        public void SendBytes(byte[] bytes)
        {
            this.WriterChannel.Writer.TryWrite(new KeyValuePair<SteerCodes, object>(SteerCodes.SendBytes, bytes));
        }

        #endregion

        #region Events

        public event HandleReceivedBytes HandleReceivedBytes;

        #endregion
    }
}
