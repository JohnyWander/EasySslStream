using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EasySslStream.ConnectionV2.Communication.ConnectionSpeed;

namespace EasySslStreamTests.ConnectionSpeedMeasureTest
{
    internal class ConnectionSpeedCheckTest
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        [SetUp]
        public void Setup()
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        [Test]
        public async Task ConnectionSpeedTest()
        {

            TransferSpeedMeasurment tmeasure = new TransferSpeedMeasurment(cancellationTokenSource.Token);

            Task.Run(() =>
            {
                Random rnd = new Random();
                for(int i =0; i<= 10000; i++)
                {
                    int buffpos = 0;
                    for(int ii =0; ii <= 100; ii++)
                    {
                        buffpos += rnd.Next(100000 * 8, 500000 * 8);
                        tmeasure.CurrentBufferPosition = buffpos;
                        Task.Delay(100).Wait();
                    }

                   

                }

                cancellationTokenSource.Cancel();


            });


            while (!cancellationTokenSource.IsCancellationRequested)
            {
                Debug.WriteLine(tmeasure.TransferSpeedInbytesPerSecond);
                Thread.Sleep(100);
                
            }


            await tmeasure.MeasureTaskHandler;
        }
            



    }
}
