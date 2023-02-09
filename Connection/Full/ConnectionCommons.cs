using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.Connection.Full
{
    public class ConnectionCommons : IFileReceiveEventAndStats
    {
        private ConnectionCommons() { }
        //File Transfer
        public int CurrentBytes { get; set; } = 0;
        public int TotalBytes { get; set; } = 0;

        ///////////////////////
        public event EventHandler OnDataChunkReceived;
        public void FireDataChunkReceived()
        {
            OnDataChunkReceived?.Invoke(this, EventArgs.Empty);
        }
        ////////////////////////////////////////////////////////


        // // //////////////
        //// Connection Speed
        public float Speed { get; set; } = 0;
        public string stringSpeed { get; set; } = string.Empty;
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

        public async Task GetFileReceiveSpeed(int Interval,Unit unit, CancellationToken cts=default(CancellationToken))
        {       
            while (!cts.IsCancellationRequested)
            {
                
                int current = CurrentBytes;
                    await Task.Delay(Interval);
                int AfterInterval = CurrentBytes;

                Speed = (AfterInterval - current)/(Interval/1000);
                //Console.WriteLine(Speed);
                switch (unit)
                {
                    case Unit.Bps:
                        stringSpeed = Speed + " " + Unit.Bps.ToString();
                        break;
                    case Unit.KBs:
                        Speed = Speed / 1024;
                        stringSpeed = Speed + " " + Unit.KBs.ToString();
                        break;
                    case Unit.MBs:
                        Speed = Speed / 1024 / 1024;
                        stringSpeed = Speed + " " + Unit.MBs.ToString();
                        break;
                }
                FireOnReceiveSpeedChecked();
               // Console.WriteLine(stringSpeed);
            }

            
        }
         ////////////////////////////////////////////////////////////
  


        
        internal static IFileReceiveEventAndStats Create()
        {
            return new ConnectionCommons();
        }




    }
}
