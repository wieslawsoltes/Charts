using System;
using System.Collections.Generic;
using ProCharts.Uno.Controls;
using Windows.Foundation;
using Xunit;

namespace ProCharts.Uno.Tests
{
    public class AnalyticsTests
    {
        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void KpiCard_InitialSetup()
        {
            var card = new KpiCard();
            Assert.Null(card.ValueString);
            Assert.Equal(0.0, card.TrendValue);
            Assert.Equal("%", card.TrendUnit);

            card.ValueString = "1,245.5";
            card.TrendValue = 12.8;

            Assert.Equal("1,245.5", card.ValueString);
            Assert.Equal(12.8, card.TrendValue);
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void HeatmapChart_InitialSetup()
        {
            var chart = new HeatmapChart();
            Assert.Null(chart.Cells);
            Assert.Null(chart.XLabels);
            Assert.Null(chart.YLabels);

            var cells = new List<HeatmapChart.HeatmapCell>
            {
                new HeatmapChart.HeatmapCell { X = 0, Y = 0, Value = 10.0 }
            };

            chart.Cells = cells;
            Assert.Single(chart.Cells);
            Assert.Equal(10.0, chart.Cells[0].Value);
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void BulletChart_InitialSetup()
        {
            var chart = new BulletChart();
            Assert.Equal(0.0, chart.Value);
            Assert.Equal(0.0, chart.Target);
            Assert.Equal(0.0, chart.Minimum);
            Assert.Equal(100.0, chart.Maximum);
            Assert.Equal(40.0, chart.BadRange);
            Assert.Equal(70.0, chart.SatisfactoryRange);
        }
    }
}
