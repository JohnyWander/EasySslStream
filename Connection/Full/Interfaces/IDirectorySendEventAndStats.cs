using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Connection.Full
{
    public interface IDirectorySendEventAndStats
    {
        
        public int TotalFilesToSend { get; set; }
        public string CurrentFilename { get; set; }
        public int CurrentFile { get; set; }


        public int CurrentFileCurrentBytes { get; set; }
        public float CurrentFileTotalBytes { get; set; }

         event EventHandler OnDirectoryProcessed;
         event EventHandler OnDirectorySendSpeedChecked;

        public void RaiseOnDirectoryProcessed();

        public float DirectorySendSpeed { get; set; }
        public string stringDirectorySendSpeed { get; set; }

        public bool AutoStartDirectowrySendSpeedCheck { get; set; }
        public int DirectorySendCheckInterval { get; set; }
        public ConnectionCommons.Unit DefaultDirectorySendUnit { get; set; }
        public Task StartDirectorySendSpeedCheck(int interval, ConnectionCommons.Unit unit, CancellationToken cts = default(CancellationToken));





    }
}
