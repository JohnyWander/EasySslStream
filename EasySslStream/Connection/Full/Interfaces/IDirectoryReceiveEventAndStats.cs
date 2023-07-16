using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Connection.Full
{
    public interface IDirectoryReceiveEventAndStats
    {

        /// <summary>
        /// Amount of all files to receive
        /// </summary>
        public int TotalFilesToReceive { get; set; }

        /// <summary>
        /// Name of currently proccessed file
        /// </summary>
        public string CurrentReceivedFileName { get; internal set; }

        /// <summary>
        /// Number of currently processed file
        /// </summary>
        public int CurrentReceiveFile { get; internal set; }

        /// <summary>
        /// Amount of processed bytes of currently processed file
        /// </summary>
        public int CurrentReceiveFileCurrentBytes { get; internal set; }


        /// <summary>
        /// Total bytes of current file to procces 
        /// </summary>
        public float CurrentReceiveFileTotalBytes { get; internal set; }


        /// <summary>
        /// Event raised when receiving file completes
        /// </summary>
        public event EventHandler OnFileFromDirectoryReceiveProcessed;

        /// <summary>
        /// Raised when connection speed is checked<br></br>
        /// You can use <see cref="DirectorySendSpeed"/>(numeric representation) <br></br>
        /// or <see cref="stringDirectorySendSpeed"/> (Mix of numeric result and specified unit in<br></br>
        /// <see cref="DefaultDirectorySendUnit"/>)<br>
        /// with this event</br>
        /// </summary>
        public event EventHandler OnDirectoryReceiveSpeedChecked;

        internal void RaiseOnFileFromDirectoryReceiveProcessed();



        internal void RaiseOnDirectoryReceiveSpeedChecked();

        /// <summary>
        /// Numeric representation of connection speed in specified unit
        /// </summary>
        public float DirectoryReceiveSpeed { get; set; }

        /// <summary>
        /// Contains combination of connection check result and selected unit in<br>
        /// <see cref="DefaultDirectoryReceiveUnit"/></br>
        /// </summary>
        public string stringDirectoryReceiveSpeed { get; set; }

        /// <summary>
        /// Specifies if <see cref="StartDirectoryReceiveSpeedCheck"/><br></br>
        /// should start automatically
        /// It wont work if <see cref="OnDirectoryReceiveSpeedChecked"/> event is not specified
        /// </summary>
        public bool AutoStartDirectoryReceiveSpeedCheck { get; set; }
        public int DirectoryReceiveCheckInterval { get; set; }
        public ConnectionCommons.Unit DefaultDirectoryReceiveUnit { get; set; }

        public Task StartDirectoryReceiveSpeedCheck(int Interval, ConnectionCommons.Unit unit, CancellationToken cts = default(CancellationToken));
        






    }
}
