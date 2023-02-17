using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Connection.Full
{
    public partial class ConnectionCommons : IFileReceiveEventAndStats,IFileSendEventAndStats,IDirectorySendEventAndStats
    {
        private ConnectionCommons() { }
        //File Transfer//////////////////////////////////////////////
        public int CurrentReceivedBytes { get; set; } = 0;
        public int TotalBytesToReceive { get; set; } = 0;

        ///////////////////////
        public event EventHandler OnDataChunkReceived;
        public void FireDataChunkReceived()
        {
            OnDataChunkReceived?.Invoke(this, EventArgs.Empty);
        }
        ////////////////////////////////////////////////////////


        // // //////////////
        //// Receive speed
        public bool AutoStartFileReceiveSpeedCheck { get; set; } = false;
        public int DefaultIntervalForFileReceiveCheck { get; set; } = 1000;
        public Unit DefaultReceiveSpeedUnit { get; set; } = Unit.Bps;
        public float ReceiveSpeed { get; set; } = 0;
        public string stringReceiveSpeed { get; set; } = string.Empty;
        public enum Unit
        {
            Bps,
            KBs,
            MBs

        }

        public event EventHandler OnReceiveSpeedChecked;
        public void FireOnReceiveSpeedChecked()
        {
            OnReceiveSpeedChecked?.Invoke(this, EventArgs.Empty);
        }

        public async Task StartFileReceiveSpeedCheck(int Interval,Unit unit, CancellationToken cts=default(CancellationToken))
        {       
            while (!cts.IsCancellationRequested)
            {

                int current = CurrentReceivedBytes;
                    await Task.Delay(Interval);
                int AfterInterval = CurrentReceivedBytes;

                ReceiveSpeed = (AfterInterval - current)/(Interval/1000);
                //Console.WriteLine(Speed);
                switch (unit)
                {
                    case Unit.Bps:
                        stringReceiveSpeed = ReceiveSpeed + " " + Unit.Bps.ToString();
                        break;
                    case Unit.KBs:
                        ReceiveSpeed = ReceiveSpeed / 1024;
                        stringReceiveSpeed = ReceiveSpeed + " " + Unit.KBs.ToString();
                        break;
                    case Unit.MBs:
                        ReceiveSpeed = ReceiveSpeed / 1024 / 1024;
                        stringReceiveSpeed = ReceiveSpeed + " " + Unit.MBs.ToString();
                        break;
                }
                FireOnReceiveSpeedChecked();
               // Console.WriteLine(stringSpeed);
            }

            
        }

        internal static IFileReceiveEventAndStats CreateFileReceive()
        {
            return new ConnectionCommons();
        }
        ////////////////////////////////////////////////////////////



        ////////////////////////////////////////////////////////////////
        // File Send events and stats

        /////////data chunk
       
        
        public int CurrentSendBytes { get; set; } = 0;
        public int TotalBytesToSend { get; set; } = 0;

        public event EventHandler OnDataChunkSent;

        public void FireOnDataChunkSent()
        {
            OnDataChunkSent.Invoke(this, EventArgs.Empty);
        }

        /////////////////////////////////////////////
        // Send speed

        public event EventHandler OnSendSpeedChecked;

        public void FireOnSpeedChecked()
        {
            OnSendSpeedChecked.Invoke(this, EventArgs.Empty);
        }
        public bool AutoStartFileSendSpeedCheck { get; set; } = false;
        public int FileSendSpeedCheckInterval { get; set; } = 1000;
        public Unit DefaultFileSendCheckUnit    { get; set; } = Unit.Bps;


        public float SendSpeed { get; set; } = 0;
        public string stringSendSpeed { get; set; } = string.Empty;



        public async Task StartFileSendSpeedCheck(int interval,Unit unit, CancellationToken cts = default(CancellationToken))
        {
            while (!cts.IsCancellationRequested)
            {
                int current = CurrentSendBytes;
                await Task.Delay(interval);
                int AfterInvterval = CurrentSendBytes;

                SendSpeed = (AfterInvterval - current) / (interval / 1000);

                switch (unit)
                {
                    case Unit.Bps:
                        stringSendSpeed = SendSpeed + " " + Unit.Bps.ToString();
                        break;
                    case Unit.KBs:
                        SendSpeed = SendSpeed / 1024;
                        stringSendSpeed = SendSpeed + " " + Unit.KBs.ToString();
                        break;
                    case Unit.MBs:
                        SendSpeed = SendSpeed / 1024 / 1024;
                        stringSendSpeed = SendSpeed + " " + Unit.MBs.ToString();
                        break;


                }
                OnSendSpeedChecked.Invoke(this, EventArgs.Empty);
            }
        }
        internal static IFileSendEventAndStats CreateFileSend()
        {
            return new ConnectionCommons();
        }






    }


    public partial class ConnectionCommons : IFileReceiveEventAndStats, IFileSendEventAndStats, IDirectorySendEventAndStats
    {

        ////////// SendDirectory
        internal static IDirectorySendEventAndStats CreateDirectorySendEventAndStats()
        {
            return new ConnectionCommons();
        }
        public int TotalFilesToSend { get; set; } = 0;
        public string CurrentFilename { get; set; } = string.Empty;
        public int CurrentFile { get; set; } = 0;

        public int CurrentFileCurrentBytes { get; set; } = 0;
        public float CurrentFileTotalBytes { get; set; } = 0;


        public event EventHandler OnDirectoryProcessed;
        public event EventHandler OnDirectorySendSpeedChecked;


        public void RaiseOnDirectoryProcessed()
        {
            OnDirectoryProcessed?.Invoke(this, EventArgs.Empty);
        }

        internal void RaiseOnDirectorySendSpeedChecked()
        {
            OnDirectorySendSpeedChecked?.Invoke(this, EventArgs.Empty);
        }


        public float DirectorySendSpeed { get; set; } = 0;
        public string stringDirectorySendSpeed { get; set; } = string.Empty;

        public bool AutoStartDirectowrySendSpeedCheck { get; set; } = false;
        public int DirectorySendCheckInterval { get; set; } = 1000;
        public Unit DefaultDirectorySendUnit { get; set; } = Unit.Bps;

        public async Task StartDirectorySendSpeedCheck(int Interval,Unit unit,CancellationToken cts = default(CancellationToken))
        {
            while (!cts.IsCancellationRequested)
            {
                int current = CurrentFileCurrentBytes;
                await Task.Delay(Interval);
                int AfterInvterval = CurrentFileCurrentBytes;


                DirectorySendSpeed = (AfterInvterval - current) / (Interval / 1000);

                switch (unit)
                {
                    case Unit.Bps:
                        stringDirectorySendSpeed = DirectorySendSpeed + " " + Unit.Bps.ToString();
                        break;
                    case Unit.KBs:
                        DirectorySendSpeed = DirectorySendSpeed / 1024;
                        stringDirectorySendSpeed = DirectorySendSpeed + " " + Unit.KBs.ToString();
                        break;
                    case Unit.MBs:
                        DirectorySendSpeed = DirectorySendSpeed / 1024 / 1024;
                        stringDirectorySendSpeed = DirectorySendSpeed + " " + Unit.MBs.ToString();
                        break;


                }
                RaiseOnDirectorySendSpeedChecked();


            }
        }


        
        

    }
}
