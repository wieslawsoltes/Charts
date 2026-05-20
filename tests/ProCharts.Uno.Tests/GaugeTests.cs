using System;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;
using ProCharts.Uno.Controls;
using Xunit;

namespace ProCharts.Uno.Tests
{
    public class GaugeTests
    {
        static GaugeTests()
        {
            try
            {
                if (Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread() == null)
                {
                    Microsoft.UI.Dispatching.DispatcherQueueController.CreateOnCurrentThread();
                }
            }
            catch { }
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void CircularGauge_DefaultInitialValues()
        {
            var gauge = new CircularGauge();

            Assert.Equal(0.0, gauge.Value);
            Assert.Equal(0.0, gauge.Minimum);
            Assert.Equal(100.0, gauge.Maximum);
            Assert.Equal(16.0, gauge.GaugeThickness);
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void LinearGauge_DefaultInitialValues()
        {
            var gauge = new LinearGauge();

            Assert.Equal(0.0, gauge.Value);
            Assert.Equal(0.0, gauge.Minimum);
            Assert.Equal(100.0, gauge.Maximum);
            Assert.Equal(Orientation.Horizontal, gauge.Orientation);
            Assert.Equal(75.0, gauge.WarningThreshold);
            Assert.Equal(90.0, gauge.ErrorThreshold);
            Assert.Equal(16.0, gauge.GaugeThickness);
            Assert.Null(gauge.Unit);
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void LiquidFillGauge_DefaultInitialValues()
        {
            var gauge = new LiquidFillGauge();

            Assert.Equal(0.0, gauge.Value);
            Assert.Equal(8.0, gauge.WaveAmplitude);
        }
    }
}
