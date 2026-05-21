using System;
using System.Collections.Generic;
using ProCharts.Avalonia.Controls;
using Xunit;

namespace ProCharts.Avalonia.Tests
{
    public class GaugeAnalyticsTests
    {
        public class TelemetryItem
        {
            public string Name { get; set; } = string.Empty;
            public double Metric { get; set; }
            public string Color { get; set; } = string.Empty;
        }

        [Fact]
        public void ProgressDonut_Properties_Correct()
        {
            var progress = new ProgressDonut
            {
                Value = 75.0,
                Minimum = 10.0,
                Maximum = 110.0,
                RingThickness = 20.0,
                CenterText = "Progressing",
                CenterSubText = "Sub"
            };

            Assert.Equal(75.0, progress.Value);
            Assert.Equal(10.0, progress.Minimum);
            Assert.Equal(110.0, progress.Maximum);
            Assert.Equal(20.0, progress.RingThickness);
            Assert.Equal("Progressing", progress.CenterText);
            Assert.Equal("Sub", progress.CenterSubText);
        }

        [Fact]
        public void GradientRingChart_Properties_Correct()
        {
            var data = new List<TelemetryItem>
            {
                new TelemetryItem { Name = "Stream A", Metric = 45.0, Color = "#FF0000" }
            };

            var chart = new GradientRingChart
            {
                ItemsSource = data,
                ValuePath = nameof(TelemetryItem.Metric),
                TitlePath = nameof(TelemetryItem.Name),
                ColorPath = nameof(TelemetryItem.Color),
                RingThickness = 15.0,
                RingSpacing = 8.0,
                Maximum = 120.0
            };

            Assert.Equal(data, chart.ItemsSource);
            Assert.Equal(nameof(TelemetryItem.Metric), chart.ValuePath);
            Assert.Equal(nameof(TelemetryItem.Name), chart.TitlePath);
            Assert.Equal(nameof(TelemetryItem.Color), chart.ColorPath);
            Assert.Equal(15.0, chart.RingThickness);
            Assert.Equal(8.0, chart.RingSpacing);
            Assert.Equal(120.0, chart.Maximum);
        }

        [Fact]
        public void FunnelChart_Properties_Correct()
        {
            var data = new List<TelemetryItem>
            {
                new TelemetryItem { Name = "Leads", Metric = 1000 },
                new TelemetryItem { Name = "Sales", Metric = 100 }
            };

            var chart = new FunnelChart
            {
                ItemsSource = data,
                ValuePath = nameof(TelemetryItem.Metric),
                TitlePath = nameof(TelemetryItem.Name)
            };

            Assert.Equal(data, chart.ItemsSource);
            Assert.Equal(nameof(TelemetryItem.Metric), chart.ValuePath);
            Assert.Equal(nameof(TelemetryItem.Name), chart.TitlePath);
        }

        [Fact]
        public void WaffleChart_Properties_Correct()
        {
            var data = new List<TelemetryItem>
            {
                new TelemetryItem { Name = "A", Metric = 30 },
                new TelemetryItem { Name = "B", Metric = 70 }
            };

            var chart = new WaffleChart
            {
                ItemsSource = data,
                ValuePath = nameof(TelemetryItem.Metric),
                TitlePath = nameof(TelemetryItem.Name),
                CellSpacing = 5.0,
                CellCornerRadius = 2.0
            };

            Assert.Equal(data, chart.ItemsSource);
            Assert.Equal(nameof(TelemetryItem.Metric), chart.ValuePath);
            Assert.Equal(nameof(TelemetryItem.Name), chart.TitlePath);
            Assert.Equal(5.0, chart.CellSpacing);
            Assert.Equal(2.0, chart.CellCornerRadius);
        }

        [Fact]
        public void WordCloudChart_Properties_Correct()
        {
            var data = new List<TelemetryItem>
            {
                new TelemetryItem { Name = "Avalonia", Metric = 50 },
                new TelemetryItem { Name = "Charts", Metric = 25 }
            };

            var chart = new WordCloudChart
            {
                ItemsSource = data,
                TextPath = nameof(TelemetryItem.Name),
                WeightPath = nameof(TelemetryItem.Metric),
                MinFontSize = 12.0,
                MaxFontSize = 38.0
            };

            Assert.Equal(data, chart.ItemsSource);
            Assert.Equal(nameof(TelemetryItem.Name), chart.TextPath);
            Assert.Equal(nameof(TelemetryItem.Metric), chart.WeightPath);
            Assert.Equal(12.0, chart.MinFontSize);
            Assert.Equal(38.0, chart.MaxFontSize);
        }
    }
}
