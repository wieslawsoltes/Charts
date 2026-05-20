using System;
using System.Collections.Generic;
using ProCharts.Uno.Series;
using Xunit;

namespace ProCharts.Uno.Tests
{
    public class StatisticalTests
    {
        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void BoxPlot_PercentileAndOutliersCalculation_Correct()
        {
            var values = new List<double> { 10, 12, 14, 15, 16, 18, 20, 100 }; // 100 is outlier
            var metrics = BoxPlotSeries.Calculate(0.0, values);

            Assert.True(metrics.IsValid);
            Assert.Equal(15.5, metrics.Median);
            Assert.Equal(13.5, metrics.Q1);
            Assert.Equal(18.5, metrics.Q3);

            // IQR = 5. Upper bound = 17.5 + 7.5 = 25. Lower bound = 12.5 - 7.5 = 5.
            // 100 is outlier.
            Assert.Single(metrics.Outliers);
            Assert.Contains(100.0, metrics.Outliers);

            // Min and Max adjusted for regular values (excluding outliers)
            Assert.Equal(10.0, metrics.Min);
            Assert.Equal(20.0, metrics.Max);
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void Histogram_FrequencyBinning_Correct()
        {
            var dataset = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var series = new HistogramSeries
            {
                ItemsSource = dataset,
                BinCount = 5
            };

            var bins = series.GetBins();

            Assert.Equal(5, bins.Count);

            // Total frequencies sum must match continuous list size
            int totalFreq = 0;
            foreach (var b in bins)
            {
                totalFreq += b.Frequency;
            }
            Assert.Equal(10, totalFreq);
        }
    }
}
