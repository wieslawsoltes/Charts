using System;
using System.Collections.Generic;
using Avalonia;
using ProCharts.Avalonia.Series;
using Xunit;

namespace ProCharts.Avalonia.Tests
{
    public class FinancialTests
    {
        public class SampleFinancialItem
        {
            public double High { get; set; }
            public double Low { get; set; }
            public double Open { get; set; }
            public double Close { get; set; }
            public double Time { get; set; }
        }

        [Fact]
        public void FinancialPointExtraction_CorrectValues()
        {
            var dataList = new List<SampleFinancialItem>
            {
                new SampleFinancialItem { High = 150.0, Low = 100.0, Open = 110.0, Close = 140.0, Time = 1.0 },
                new SampleFinancialItem { High = 160.0, Low = 120.0, Open = 145.0, Close = 130.0, Time = 2.0 }
            };

            var series = new CandlestickSeries
            {
                ItemsSource = dataList,
                CategoryPath = nameof(SampleFinancialItem.Time),
                HighPath = nameof(SampleFinancialItem.High),
                LowPath = nameof(SampleFinancialItem.Low),
                OpenPath = nameof(SampleFinancialItem.Open),
                ClosePath = nameof(SampleFinancialItem.Close)
            };

            var points = series.GetFinancialPoints();

            Assert.Equal(2, points.Count);

            Assert.Equal(1.0, points[0].X);
            Assert.Equal(110.0, points[0].Open);
            Assert.Equal(150.0, points[0].High);
            Assert.Equal(100.0, points[0].Low);
            Assert.Equal(140.0, points[0].Close);

            Assert.Equal(2.0, points[1].X);
            Assert.Equal(145.0, points[1].Open);
            Assert.Equal(160.0, points[1].High);
            Assert.Equal(120.0, points[1].Low);
            Assert.Equal(130.0, points[1].Close);
        }

        [Fact]
        public void FinancialSeries_GetDataPoints_ReturnsHighAndLowBoundaries()
        {
            var dataList = new List<SampleFinancialItem>
            {
                new SampleFinancialItem { High = 200.0, Low = 100.0, Open = 120.0, Close = 180.0, Time = 0.0 }
            };

            var series = new HiloSeries
            {
                ItemsSource = dataList,
                CategoryPath = nameof(SampleFinancialItem.Time),
                HighPath = nameof(SampleFinancialItem.High),
                LowPath = nameof(SampleFinancialItem.Low),
                OpenPath = nameof(SampleFinancialItem.Open),
                ClosePath = nameof(SampleFinancialItem.Close)
            };

            var points = series.GetDataPoints();

            // Should return two data points to help the chart scale its Y axis correctly: High and Low
            Assert.Equal(2, points.Count);
            Assert.Contains(new Point(0.0, 200.0), points);
            Assert.Contains(new Point(0.0, 100.0), points);
        }
    }
}
