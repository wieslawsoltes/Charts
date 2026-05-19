using System;
using System.Collections.Generic;
using ProCharts.Controls;
using ProCharts.Series;
using Xunit;

namespace ProCharts.Tests
{
    public class HierarchyStatisticalTests
    {
        [Fact]
        public void AlluvialChart_Properties_Correct()
        {
            var nodes = new List<AlluvialChart.AlluvialNode>
            {
                new AlluvialChart.AlluvialNode { Name = "A", Stage = 0, Value = 100 },
                new AlluvialChart.AlluvialNode { Name = "B", Stage = 1, Value = 100 }
            };

            var links = new List<AlluvialChart.AlluvialLink>
            {
                new AlluvialChart.AlluvialLink { Source = "A", Target = "B", Flow = 50 }
            };

            var chart = new AlluvialChart
            {
                Nodes = nodes,
                Links = links,
                NodeWidth = 20.0,
                NodeGap = 15.0
            };

            Assert.Equal(nodes, chart.Nodes);
            Assert.Equal(links, chart.Links);
            Assert.Equal(20.0, chart.NodeWidth);
            Assert.Equal(15.0, chart.NodeGap);
        }

        [Fact]
        public void SunburstChart_Properties_Correct()
        {
            var rootNodes = new List<SunburstChart.SunburstNode>
            {
                new SunburstChart.SunburstNode
                {
                    Name = "Root A",
                    Value = 100
                }
            };

            var chart = new SunburstChart
            {
                Title = "Sunburst Hierarchical Structure",
                RootNodes = rootNodes,
                InnerHollowRadiusPercent = 0.35
            };

            Assert.Equal("Sunburst Hierarchical Structure", chart.Title);
            Assert.Equal(rootNodes, chart.RootNodes);
            Assert.Equal(0.35, chart.InnerHollowRadiusPercent);
        }

        [Fact]
        public void ViolinPlotSeries_KdeCalculations_Correct()
        {
            var samples = new List<double> { 2, 4, 4, 4, 5, 5, 7, 9 };
            var metrics = ViolinPlotSeries.Calculate(1.0, samples, 20);

            Assert.True(metrics.IsValid);
            Assert.Equal(1.0, metrics.X);
            Assert.Equal(2.0, metrics.Min);
            Assert.Equal(9.0, metrics.Max);
            Assert.Equal(4.5, metrics.Median);
            Assert.Equal(20, metrics.SamplePoints.Count);
            Assert.Equal(20, metrics.Densities.Count);

            foreach (var d in metrics.Densities)
            {
                Assert.True(d >= 0.0);
            }
        }
    }
}
