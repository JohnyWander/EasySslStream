namespace EasySslStream.Connection.Full
{

    public interface IFileReceiveEventAndStats
    {
        /// <summary>
        /// Current bytes of transfer
        /// </summary>
        long CurrentReceivedBytes { get; set; }

        /// <summary>
        /// Total bytes of processed file
        /// </summary>
        long TotalBytesToReceive { get; set; }

        /// <summary>
        /// Raised when chunk of data is received
        /// </summary>
        event EventHandler OnDataChunkReceived;
        internal void FireDataChunkReceived();





        // // // // // / Connection speed

        /// <summary>
        /// Units of transfer speed
        /// </summary>
        public ConnectionCommons.Unit DefaultReceiveSpeedUnit { get; set; }


        /// <summary>
        /// If true everytime file transfer start, StartFileReceiveSpeedCheck is started with pre-set settings
        /// </summary>
        public bool AutoStartFileReceiveSpeedCheck { get; set; }
        /// <summary>
        /// Interval between each speed check
        /// </summary>
        public int DefaultIntervalForFileReceiveCheck { get; set; }

        /// <summary>
        /// Units of transfer speed
        /// </summary>
        public enum Unit
        {
            Bps,
            KBs,
            MBs
        }

        /// <summary>
        /// Calculated speed as float
        /// </summary>
        public float ReceiveSpeed { get; set; }

        /// <summary>
        /// Transfer speed with it's unit
        /// </summary>
        public string stringReceiveSpeed { get; set; }

        /// <summary>
        /// Raised when transfer speed is checked
        /// </summary>
        public event EventHandler OnReceiveSpeedChecked;

        /// <summary>
        /// Checks connection speed evetytime specified Interval passes
        /// </summary>
        /// <param name="Interval">Delay between each check</param>
        /// <param name="unit">Unit of transfer speed</param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public Task StartFileReceiveSpeedCheck(int Interval, ConnectionCommons.Unit unit, CancellationToken cts = default(CancellationToken));





    }
}
