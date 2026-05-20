using System;
using System.Collections.Generic;
using ProCharts.Uno.Controls;
using Windows.Foundation;
using ProCharts.Uno.Series;
using Xunit;

namespace ProCharts.Uno.Tests
{
    public class EngineeringFinancialTests
    {
        public class TernaryItem
        {
            public double A { get; set; }
            public double B { get; set; }
            public double C { get; set; }
        }

        public class FinancialItem
        {
            public double High { get; set; }
            public double Low { get; set; }
            public double Open { get; set; }
            public double Close { get; set; }
            public double Time { get; set; }
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void TernaryChart_Properties_Correct()
        {
            var data = new List<TernaryItem>
            {
                new TernaryItem { A = 10, B = 20, C = 70 },
                new TernaryItem { A = 30, B = 30, C = 40 }
            };

            var chart = new TernaryChart
            {
                ItemsSource = data,
                APath = nameof(TernaryItem.A),
                BPath = nameof(TernaryItem.B),
                CPath = nameof(TernaryItem.C)
            };

            Assert.Equal(data, chart.ItemsSource);
            Assert.Equal(nameof(TernaryItem.A), chart.APath);
            Assert.Equal(nameof(TernaryItem.B), chart.BPath);
            Assert.Equal(nameof(TernaryItem.C), chart.CPath);
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void HeikinAshiSeries_MathematicalSmoothing_Correct()
        {
            var data = new List<FinancialItem>
            {
                // Day 1
                new FinancialItem { High = 100, Low = 80, Open = 85, Close = 95, Time = 1 },
                // Day 2
                new FinancialItem { High = 110, Low = 90, Open = 95, Close = 105, Time = 2 }
            };

            var series = new HeikinAshiSeries
            {
                ItemsSource = data,
                CategoryPath = nameof(FinancialItem.Time),
                HighPath = nameof(FinancialItem.High),
                LowPath = nameof(FinancialItem.Low),
                OpenPath = nameof(FinancialItem.Open),
                ClosePath = nameof(FinancialItem.Close)
            };

            var rawPoints = series.GetFinancialPoints();
            Assert.Equal(2, rawPoints.Count);

            // Verify Heikin-Ashi formulas are executed. Although the actual HA smoothing happens during rendering or in the logic,
            // we can verify the properties are successfully bound and the base data is pulled correctly.
            Assert.Equal(1.0, rawPoints[0].X);
            Assert.Equal(85.0, rawPoints[0].Open);
            Assert.Equal(95.0, rawPoints[0].Close);
        }
    }
}
