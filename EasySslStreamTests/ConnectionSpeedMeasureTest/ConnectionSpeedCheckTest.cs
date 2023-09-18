using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EasySslStream.ConnectionV2.Communication.ConnectionSpeed;
using EasySslStreamTests.ConnectionV2Testing;

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
            List<double> reads = new List<double>();
            Task.Run(() =>
            {
                Random rnd = new Random();
                for(int i =0; i<= 10000; i++)
                {
                    int buffpos = 0;
                    for(int ii =0; ii <= 100; ii++)
                    {
                        buffpos += rnd.Next(40992768, 107298816); // took from real transfer values
                        tmeasure.CurrentBufferPosition = buffpos;
                        Task.Delay(100).Wait();
                    }                  
                }
                cancellationTokenSource.Cancel();
            });

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                reads.Add(tmeasure.TransferSpeedInbytesPerSecond);
                Thread.Sleep(100);                
            }
            await tmeasure.MeasureTaskHandler;

            double average = reads.Sum() / reads.Count;

            Assert.That(average > 0);
        }

        [Test]
        [Ignore]
        public async Task ConnectionspeedTestOnRealtransferClientToServerTest()
        {
            ConnectionV2Tests ccallback = new ConnectionV2Tests();
            ccallback.Setup();
            ccallback.OneTimeSetup();

            Task senddirTest = Task.Run(() => ccallback.DiretoryTransferAsyncClientToServer());
            await Task.Delay(1000);

            TransferSpeedMeasurment receiveSpeed = ccallback.srv.ConnectedClientsById[0].ConnectionHandler.ReceiveSpeed;
            TransferSpeedMeasurment sendingSpeed = ccallback.client.ConnectionHandler.Sendspeed;

            while(!senddirTest.IsCompleted)
            {
                Debug.WriteLine(receiveSpeed.TransferSpeedInbytesPerSecond);
                await Task.Delay(100);
            }





            await senddirTest;
        }
        


    }
}
