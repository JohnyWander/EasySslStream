namespace EasySslStream.Connection.Client
{
    public interface IFileSendEventAndStats
    {
        public long CurrentSendBytes { get; set; }
        public long TotalBytesToSend { get; set; }
        public event EventHandler OnDataChunkSent;
        internal void FireOnDataChunkSent();


        ////////////////


        public event EventHandler OnSendSpeedChecked;

        internal void FireOnSpeedChecked();


        public bool AutoStartFileSendSpeedCheck { get; set; }
        public int FileSendSpeedCheckInterval { get; set; }
        public ConnectionCommons.Unit DefaultFileSendCheckUnit { get; set; }

        public float SendSpeed { get; set; }
        public string stringSendSpeed { get; set; }

        public Task StartFileSendSpeedCheck(int interval, ConnectionCommons.Unit unit, CancellationToken cts = default(CancellationToken));




    }
}
