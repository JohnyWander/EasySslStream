using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.ConnectionV2.Communication.ConnectionSpeed
{
    public class TransferSpeedMeasurment : ITransferSpeed
    {
        public Task MeasureTaskHandler;


        public long CurrentBufferPosition         
        {
            private get { return _currentRead; }
            set
            {
                _currentRead = value;               
            }
        }

        private long _currentRead = 0;
        private long _previousRead = 0;
        private int _checkDelay = 0;
        
        private double DividableCheckDelay;

        public double TransferSpeedInbytesPerSecond { get; set; }

        public TransferSpeedMeasurment(CancellationToken cts, int delay=500)
        {            
            _checkDelay = delay;
            MeasureTaskHandler = Task.Run(() => MeasureTask(cts));
            DividableCheckDelay = delay;
        }

        async Task MeasureTask(CancellationToken cts)
        {
            while(!cts.IsCancellationRequested)
            {
               
                _previousRead = _currentRead;
                await Task.Delay(_checkDelay);
                TransferSpeedInbytesPerSecond = (_currentRead - _previousRead) / (DividableCheckDelay / 1000);
                if(TransferSpeedInbytesPerSecond < 0)
                {
                    TransferSpeedInbytesPerSecond = 0;
                }
            }
        }

        public double KBs(int round = 2)
        {
            return Math.Round(this.TransferSpeedInbytesPerSecond / 1000,round);
        }

        public string KBsString(int round = 2)
        {
            double rounded = KBs(round);
            return $"{rounded} KB/s";
        }

        public double MBs(int round = 2)
        {
            return Math.Round(this.TransferSpeedInbytesPerSecond / 1000000,round);
        }

        public string MBsString(int round = 2)
        {
            double rounded = MBs(round);
            return $"{rounded} MB/s";
        }

    }
}
