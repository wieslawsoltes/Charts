using System;
using System.Collections.Generic;
using ProCharts.Controls;
using Xunit;

namespace ProCharts.Tests
{
    public class HierarchyTests
    {
        [Fact]
        public void SankeyChart_InitialSetup()
        {
            var chart = new SankeyChart();
            Assert.Null(chart.Nodes);
            Assert.Null(chart.Links);

            var nodes = new List<SankeyChart.SankeyNode>
            {
                new SankeyChart.SankeyNode { Name = "A" },
                new SankeyChart.SankeyNode { Name = "B" }
            };

            var links = new List<SankeyChart.SankeyLink>
            {
                new SankeyChart.SankeyLink { Source = "A", Target = "B", Flow = 50.0 }
            };

            chart.Nodes = nodes;
            chart.Links = links;

            Assert.Equal(2, chart.Nodes.Count);
            Assert.Single(chart.Links);
            Assert.Equal("A", chart.Links[0].Source);
            Assert.Equal("B", chart.Links[0].Target);
            Assert.Equal(50.0, chart.Links[0].Flow);
        }

        [Fact]
        public void TreemapChart_InitialSetup()
        {
            var chart = new TreemapChart();
            Assert.Null(chart.Items);

            var items = new List<TreemapChart.TreemapItem>
            {
                new TreemapChart.TreemapItem { Label = "Category A", Value = 100 },
                new TreemapChart.TreemapItem { Label = "Category B", Value = 200 }
            };

            chart.Items = items;

            Assert.Equal(2, chart.Items.Count);
            Assert.Equal("Category A", chart.Items[0].Label);
            Assert.Equal(100, chart.Items[0].Value);
        }
    }
}
