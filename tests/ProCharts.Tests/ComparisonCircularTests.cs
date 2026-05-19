using System;
using Avalonia;
using ProCharts.Controls;
using ProCharts.Series;
using Xunit;

namespace ProCharts.Tests
{
    public class ComparisonCircularTests
    {
        [Fact]
        public void DonutChart_Properties_Correct()
        {
            var chart = new DonutChart
            {
                CenterText = "1234",
                CenterSubText = "TELEMETRY",
                HollowRadius = 0.5
            };

            Assert.Equal("1234", chart.CenterText);
            Assert.Equal("TELEMETRY", chart.CenterSubText);
            Assert.Equal(0.5, chart.HollowRadius);
        }

        [Fact]
        public void SemiDonutChart_Properties_Correct()
        {
            var chart = new SemiDonutChart
            {
                CenterText = "50%",
                HollowRadius = 0.7
            };

            Assert.Equal("50%", chart.CenterText);
            Assert.Equal(0.7, chart.HollowRadius);
            Assert.Equal(180.0, chart.StartAngle);
            Assert.Equal(180.0, chart.TotalAngle);
        }

        [Fact]
        public void DivergingBarChart_Properties_Correct()
        {
            var chart = new DivergingBarChart
            {
                Title = "Diverging Performance",
                CategoryPath = "Category",
                LeftValuePath = "Low",
                RightValuePath = "High"
            };

            Assert.Equal("Diverging Performance", chart.Title);
            Assert.Equal("Category", chart.CategoryPath);
            Assert.Equal("Low", chart.LeftValuePath);
            Assert.Equal("High", chart.RightValuePath);
        }

        [Fact]
        public void TornadoChart_Properties_Correct()
        {
            var chart = new TornadoChart
            {
                Title = "Sensitivity Analysis",
                CategoryPath = "Var",
                LowValuePath = "Low",
                HighValuePath = "High"
            };

            Assert.Equal("Sensitivity Analysis", chart.Title);
            Assert.Equal("Var", chart.CategoryPath);
            Assert.Equal("Low", chart.LowValuePath);
            Assert.Equal("High", chart.HighValuePath);
        }
    }
}
