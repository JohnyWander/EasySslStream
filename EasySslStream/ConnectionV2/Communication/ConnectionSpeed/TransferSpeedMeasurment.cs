using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySslStream.ConnectionV2.Communication.ConnectionSpeed
{
    internal class TransferSpeedMeasurment : ITransferSpeed
    {
        Task MeasureTaskHandler;


        internal int CurrenBufferPosition         
        {
            private get { return _currentRead; }
            set
            {
                _previousRead = _currentRead;
                _currentRead = value;
                
            }
        }

        private int _currentRead = 0;
        private int _previousRead = 0;
        private int _checkDelay = 0;

        public long TransferSpeedInbytesPerSecond { get; set; }

        public TransferSpeedMeasurment(CancellationToken cts, int delay=500)
        {            
            _checkDelay = delay;
            MeasureTaskHandler = Task.Run(() => MeasureTask(cts));
        }

        async Task MeasureTask(CancellationToken cts)
        {
            while(!cts.IsCancellationRequested)
            {
                await Task.Delay(_checkDelay);               
                this.TransferSpeedInbytesPerSecond = (_currentRead - _previousRead) / (_checkDelay * 1000);                
            }
        }


    }
}
