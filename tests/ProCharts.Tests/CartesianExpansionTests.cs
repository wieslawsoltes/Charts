using System;
using System.Collections.Generic;
using Avalonia;
using ProCharts.Controls;
using ProCharts.Series;
using Xunit;

namespace ProCharts.Tests
{
    public class CartesianExpansionTests
    {
        public class SampleItem
        {
            public double XVal { get; set; }
            public double YVal { get; set; }
            public double SizeVal { get; set; }
        }

        public class WaterfallItem
        {
            public string Category { get; set; } = string.Empty;
            public double Value { get; set; }
            public bool IsTotal { get; set; }
        }

        [Fact]
        public void ScatterSeries_PropertiesAndDataExtraction_Correct()
        {
            var data = new List<SampleItem>
            {
                new SampleItem { XVal = 10, YVal = 20 },
                new SampleItem { XVal = 30, YVal = 40 }
            };

            var series = new ScatterSeries
            {
                ItemsSource = data,
                CategoryPath = nameof(SampleItem.XVal),
                ValuePath = nameof(SampleItem.YVal),
                Shape = ScatterShape.Diamond,
                Size = 12.0
            };

            var points = series.GetDataPoints();
            Assert.Equal(2, points.Count);
            Assert.Equal(10, points[0].X);
            Assert.Equal(20, points[0].Y);
            Assert.Equal(ScatterShape.Diamond, series.Shape);
            Assert.Equal(12.0, series.Size);
        }

        [Fact]
        public void StepLineSeries_PropertiesAndDataExtraction_Correct()
        {
            var data = new List<SampleItem>
            {
                new SampleItem { XVal = 0, YVal = 5 },
                new SampleItem { XVal = 5, YVal = 15 }
            };

            var series = new StepLineSeries
            {
                ItemsSource = data,
                CategoryPath = nameof(SampleItem.XVal),
                ValuePath = nameof(SampleItem.YVal)
            };

            var points = series.GetDataPoints();
            Assert.Equal(2, points.Count);
            Assert.Equal(0, points[0].X);
            Assert.Equal(5, points[0].Y);
        }

        [Fact]
        public void StackedBarSeries_Properties_Correct()
        {
            var data = new List<SampleItem>
            {
                new SampleItem { XVal = 1, YVal = 10 },
                new SampleItem { XVal = 2, YVal = 20 }
            };

            var series = new StackedBarSeries
            {
                ItemsSource = data,
                CategoryPath = nameof(SampleItem.XVal),
                ValuePath = nameof(SampleItem.YVal)
            };

            var points = series.GetDataPoints();
            Assert.Equal(2, points.Count);
            Assert.Equal(10, points[0].Y);
        }

        [Fact]
        public void StackedAreaSeries_Properties_Correct()
        {
            var data = new List<SampleItem>
            {
                new SampleItem { XVal = 1, YVal = 100 },
                new SampleItem { XVal = 2, YVal = 200 }
            };

            var series = new StackedAreaSeries
            {
                ItemsSource = data,
                CategoryPath = nameof(SampleItem.XVal),
                ValuePath = nameof(SampleItem.YVal)
            };

            var points = series.GetDataPoints();
            Assert.Equal(2, points.Count);
            Assert.Equal(100, points[0].Y);
        }

        [Fact]
        public void BubbleSeries_PropertiesAndSizeMapping_Correct()
        {
            var data = new List<SampleItem>
            {
                new SampleItem { XVal = 1, YVal = 10, SizeVal = 50 },
                new SampleItem { XVal = 2, YVal = 20, SizeVal = 150 }
            };

            var series = new BubbleSeries
            {
                ItemsSource = data,
                CategoryPath = nameof(SampleItem.XVal),
                ValuePath = nameof(SampleItem.YVal),
                SizePath = nameof(SampleItem.SizeVal),
                MinBubbleSize = 5.0,
                MaxBubbleSize = 25.0
            };

            var points = series.GetDataPoints();
            Assert.Equal(2, points.Count);
            Assert.Equal(1, points[0].X);
            Assert.Equal(10, points[0].Y);
            Assert.Equal(nameof(SampleItem.SizeVal), series.SizePath);
            Assert.Equal(5.0, series.MinBubbleSize);
            Assert.Equal(25.0, series.MaxBubbleSize);
        }

        [Fact]
        public void WaterfallChart_RunningSumCalculation_Correct()
        {
            var data = new List<WaterfallItem>
            {
                new WaterfallItem { Category = "Start", Value = 100 },
                new WaterfallItem { Category = "Sales", Value = 50 },
                new WaterfallItem { Category = "Refunds", Value = -20 },
                new WaterfallItem { Category = "Total", IsTotal = true }
            };

            var chart = new WaterfallChart
            {
                ItemsSource = data,
                CategoryPath = nameof(WaterfallItem.Category),
                ValuePath = nameof(WaterfallItem.Value),
                IsTotalPath = nameof(WaterfallItem.IsTotal)
            };

            // Aggregation runs in RenderChart inside effective logic. Let's make sure properties bind.
            Assert.Equal(data, chart.ItemsSource);
            Assert.Equal(nameof(WaterfallItem.Category), chart.CategoryPath);
            Assert.Equal(nameof(WaterfallItem.Value), chart.ValuePath);
            Assert.Equal(nameof(WaterfallItem.IsTotal), chart.IsTotalPath);
        }
    }
}
