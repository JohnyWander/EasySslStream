using System;
using System.Collections.Generic;
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
        public void ConnectionSpeedTest()
        {

            TransferSpeedMeasurment tmeasure = new TransferSpeedMeasurment(cancellationTokenSource.Token);


            while (!cancellationTokenSource.IsCancellationRequested)
            {


            }
        }
            



    }
}
