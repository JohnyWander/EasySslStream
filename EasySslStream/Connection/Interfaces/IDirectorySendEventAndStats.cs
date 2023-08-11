namespace EasySslStream.Connection.Client
{
    public interface IDirectorySendEventAndStats
    {
        /// <summary>
        /// Amount of all files to send
        /// </summary>
        public int TotalFilesToSend { get; set; }

        /// <summary>
        /// Name of currently proccessed file
        /// </summary>
        public string CurrentSendFilename { get; set; }

        /// <summary>
        /// Number of currently processed file
        /// </summary>
        public int CurrentSendFile { get; set; }

        /// <summary>
        /// Amount of processed bytes of currently processed file
        /// </summary>
        public int CurrentSendFileCurrentBytes { get; set; }


        /// <summary>
        /// Total bytes of current file to procces 
        /// </summary>
        public float CurrentSendFileTotalBytes { get; set; }


        /// <summary>
        /// Event raised when any file transfer from directory ends
        /// </summary>
        event EventHandler OnFileFromDirectorySendProcessed;

        /// <summary>
        /// Raised when connection speed is checked<br></br>
        /// You can use <see cref="DirectorySendSpeed"/>(numeric representation) <br></br>
        /// or <see cref="stringDirectorySendSpeed"/> (Mix of numeric result and specified unit in<br></br>
        /// <see cref="DefaultDirectorySendUnit"/>)<br>
        /// with this event</br>
        /// </summary>
        event EventHandler OnDirectorySendSpeedChecked;

        internal void RaiseOnFileFromDirectorySendProcessed();

        /// <summary>
        /// Numeric representation of connection speed in specified unit
        /// </summary>
        public float DirectorySendSpeed { get; set; }

        /// <summary>
        /// Contains combination of connection check result and selected unit in<br>
        /// <see cref="DefaultDirectorySendUnit"/></br>
        /// </summary>
        public string stringDirectorySendSpeed { get; set; }

        /// <summary>
        /// Specifies if <see cref="StartDirectorySendSpeedCheck"/><br></br>
        /// should start automatically
        /// It wont work if <see cref="OnDirectoryProcessed"/> event is not specified
        /// </summary>
        public bool AutoStartDirectorySendSpeedCheck { get; set; }


        /// <summary>
        /// Specifies how often connection speed should be checked
        /// </summary>
        public int DirectorySendCheckInterval { get; set; }

        /// <summary>
        /// Speed unit for <see cref="StartDirectorySendSpeedCheck"/><br>
        /// Bps by default</br>
        /// </summary>
        public ConnectionCommons.Unit DefaultDirectorySendUnit { get; set; }

        /// <summary>
        /// Task that checks connection speed
        /// </summary>
        /// <param name="interval">Delay between each check</param>
        /// <param name="unit">Unit of transfer speed</param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public Task StartDirectorySendSpeedCheck(int interval, ConnectionCommons.Unit unit, CancellationToken cts = default(CancellationToken));





    }
}
