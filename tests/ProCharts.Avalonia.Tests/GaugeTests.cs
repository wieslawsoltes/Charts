using System;
using Avalonia;
using Avalonia.Layout;
using ProCharts.Avalonia.Controls;
using Xunit;

namespace ProCharts.Avalonia.Tests
{
    public class GaugeTests
    {
        [Fact]
        public void CircularGauge_DefaultInitialValues()
        {
            var gauge = new CircularGauge();

            Assert.Equal(0.0, gauge.Value);
            Assert.Equal(0.0, gauge.Minimum);
            Assert.Equal(100.0, gauge.Maximum);
            Assert.Equal(16.0, gauge.GaugeThickness);
        }

        [Fact]
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

        [Fact]
        public void LiquidFillGauge_DefaultInitialValues()
        {
            var gauge = new LiquidFillGauge();

            Assert.Equal(0.0, gauge.Value);
            Assert.Equal(8.0, gauge.WaveAmplitude);
        }
    }
}
