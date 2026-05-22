using System;
using System.Collections.Generic;
using Avalonia;
using ProCharts.Avalonia.Controls;
using ProCharts.Avalonia.Series;
using ProCharts.Avalonia.Components;
using Xunit;

namespace ProCharts.Avalonia.Tests
{
    public class NewComponentsTests
    {
        [Fact]
        public void NightingaleRoseChart_DefaultInitialValues()
        {
            var chart = new NightingaleRoseChart();

            Assert.Equal(0.0, chart.StartAngle);
            Assert.Equal(360.0, chart.TotalAngle);
            Assert.Equal(0.0, chart.HollowRadius);
            Assert.Empty(chart.Series);
        }

        [Fact]
        public void NightingaleRoseChart_PropertyMutation()
        {
            var chart = new NightingaleRoseChart
            {
                StartAngle = 45.0,
                TotalAngle = 180.0,
                HollowRadius = 0.5
            };

            Assert.Equal(45.0, chart.StartAngle);
            Assert.Equal(180.0, chart.TotalAngle);
            Assert.Equal(0.5, chart.HollowRadius);
        }

        [Fact]
        public void RoseSeries_DefaultInitialValues()
        {
            var series = new RoseSeries();

            Assert.Equal(0.0, series.Value);
            Assert.Null(series.Title);
            Assert.True(series.IsVisible);
        }

        [Fact]
        public void RoseSeries_PropertyMutation()
        {
            var series = new RoseSeries
            {
                Value = 25.5,
                Title = "Segment Alpha",
                IsVisible = false
            };

            Assert.Equal(25.5, series.Value);
            Assert.Equal("Segment Alpha", series.Title);
            Assert.False(series.IsVisible);
        }

        [Fact]
        public void GanttChart_DefaultInitialValues()
        {
            var chart = new GanttChart();

            Assert.Null(chart.Tasks);
        }

        [Fact]
        public void GanttChart_PropertyMutation()
        {
            var chart = new GanttChart();
            var tasks = new List<GanttTask>
            {
                new GanttTask { Name = "Task 1" }
            };

            chart.Tasks = tasks;

            Assert.NotNull(chart.Tasks);
            Assert.Single(chart.Tasks);
            Assert.Equal("Task 1", chart.Tasks[0].Name);
        }

        [Fact]
        public void GanttTask_DefaultInitialValues()
        {
            var task = new GanttTask();

            Assert.Equal(string.Empty, task.Name);
            Assert.Equal(DateTime.MinValue, task.Start);
            Assert.Equal(DateTime.MinValue, task.End);
            Assert.Equal(0.0, task.Progress);
            Assert.Null(task.CustomBrush);
        }

        [Fact]
        public void GanttTask_PropertyMutation()
        {
            var start = new DateTime(2026, 5, 1);
            var end = new DateTime(2026, 5, 10);
            var task = new GanttTask
            {
                Name = "Engineering Design",
                Start = start,
                End = end,
                Progress = 0.75
            };

            Assert.Equal("Engineering Design", task.Name);
            Assert.Equal(start, task.Start);
            Assert.Equal(end, task.End);
            Assert.Equal(0.75, task.Progress);
        }

        [Fact]
        public void CarpetPlotChart_DefaultInitialValues()
        {
            var chart = new CarpetPlotChart();

            Assert.Empty(chart.Series);
        }

        [Fact]
        public void CarpetSeries_DefaultInitialValues()
        {
            var series = new CarpetSeries();

            Assert.Equal(0, series.Rows);
            Assert.Equal(0, series.Columns);
            Assert.Empty(series.XValues);
            Assert.Empty(series.YValues);
        }

        [Fact]
        public void CarpetSeries_PropertyMutation()
        {
            var xData = new double[] { 1.0, 2.0, 3.0, 4.0 };
            var yData = new double[] { 10.0, 20.0, 30.0, 40.0 };
            var series = new CarpetSeries
            {
                Rows = 2,
                Columns = 2,
                XValues = xData,
                YValues = yData
            };

            Assert.Equal(2, series.Rows);
            Assert.Equal(2, series.Columns);
            Assert.Equal(xData, series.XValues);
            Assert.Equal(yData, series.YValues);
        }

        [Fact]
        public void BubbleMapChart_DefaultInitialValues()
        {
            var chart = new BubbleMapChart();

            Assert.Null(chart.Bubbles);
        }

        [Fact]
        public void MapBubbleItem_DefaultInitialValues()
        {
            var item = new MapBubbleItem();

            Assert.Equal(string.Empty, item.Label);
            Assert.Equal(0.0, item.Latitude);
            Assert.Equal(0.0, item.Longitude);
            Assert.Equal(0.0, item.Value);
            Assert.Null(item.Color);
        }

        [Fact]
        public void MapBubbleItem_PropertyMutation()
        {
            var item = new MapBubbleItem
            {
                Label = "London",
                Latitude = 51.5074,
                Longitude = -0.1278,
                Value = 120.0
            };

            Assert.Equal("London", item.Label);
            Assert.Equal(51.5074, item.Latitude);
            Assert.Equal(-0.1278, item.Longitude);
            Assert.Equal(120.0, item.Value);
        }
    }
}
