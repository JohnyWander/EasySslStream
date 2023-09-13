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
        Task MeasureTaskHandler;


        public int CurrentBufferPosition         
        {
            private get { return _currentRead; }
            set
            {
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
                _previousRead = _currentRead;
                await Task.Delay(_checkDelay);
                TransferSpeedInbytesPerSecond = (_currentRead - _previousRead) / (_checkDelay * 1000);
            }
        }


    }
}
