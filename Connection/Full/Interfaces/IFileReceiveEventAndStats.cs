using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Connection.Full
{
    
    public interface IFileReceiveEventAndStats
    {
        
        int CurrentBytes { get; set; } 
        int TotalBytes { get; set; }

        delegate void DataChunkReceivedEvent(int CurrentBytes, int TotalBytes);
        event EventHandler OnDataChunkReceived;
        internal void FireDataChunkReceived();





        // // // // // / Connection speed
        public enum Unit
        {
            Bps,
            KBs,
            MBs
        }
        public float Speed { get; set; }
        public string stringSpeed { get; set; }

        public event EventHandler OnReceiveSpeedChecked;
        public Task GetFileReceiveSpeed(int Interval,ConnectionCommons.Unit unit, CancellationToken cts = default(CancellationToken));





    }
}
