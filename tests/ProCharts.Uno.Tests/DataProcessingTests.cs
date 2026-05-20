using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using ProCharts.Uno.Series;
using ProCharts.Uno.Maths;
using Xunit;

namespace ProCharts.Uno.Tests
{
    public class DataProcessingTests
    {
        // Define a test model to check Reflection Cache
        public class ChartItem
        {
            public string Category { get; set; } = string.Empty;
            public double Value { get; set; }
        }

        // Define a test series type to access GetDataPoints
        public class TestCartesianSeries : CartesianSeries
        {
            public override void RenderSeries(SeriesRenderContext context)
            {
                // No-op for tests
            }
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void GetDataPoints_DirectPoints_ReturnsPointsDirectly()
        {
            var series = new TestCartesianSeries();
            var pointsList = new List<Point>
            {
                new Point(1.0, 10.0),
                new Point(2.0, 20.0),
                new Point(3.0, 30.0)
            };
            series.ItemsSource = pointsList;

            var result = series.GetDataPoints();
            Assert.Equal(pointsList.Count, result.Count);
            Assert.Equal(1.0, result[0].X);
            Assert.Equal(10.0, result[0].Y);
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void GetDataPoints_DirectNumbers_ReturnsIndicesAndValues()
        {
            var series = new TestCartesianSeries();
            var numbers = new List<double> { 5.5, 12.0, -3.2 };
            series.ItemsSource = numbers;

            var result = series.GetDataPoints();
            Assert.Equal(numbers.Count, result.Count);
            Assert.Equal(0.0, result[0].X);
            Assert.Equal(5.5, result[0].Y);
            Assert.Equal(1.0, result[1].X);
            Assert.Equal(12.0, result[1].Y);
            Assert.Equal(2.0, result[2].X);
            Assert.Equal(-3.2, result[2].Y);
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void GetDataPoints_CustomObjectDataBinding_CorrectlyResolvesValues()
        {
            var series = new TestCartesianSeries
            {
                CategoryPath = "Category",
                ValuePath = "Value"
            };

            var items = new List<ChartItem>
            {
                new ChartItem { Category = "A", Value = 15.0 },
                new ChartItem { Category = "B", Value = 25.0 }
            };
            series.ItemsSource = items;

            var result = series.GetDataPoints();
            Assert.Equal(2, result.Count);
            // "A" is not a numeric string so x should fallback to index 0
            Assert.Equal(0.0, result[0].X);
            Assert.Equal(15.0, result[0].Y);

            Assert.Equal(1.0, result[1].X);
            Assert.Equal(25.0, result[1].Y);
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void EmptyPointMode_Zero_ReplacesNaNWithZero()
        {
            var series = new LineSeries
            {
                EmptyPointMode = EmptyPointMode.Zero
            };

            var points = new List<Point>
            {
                new Point(0, 10),
                new Point(1, double.NaN),
                new Point(2, 30)
            };

            var plotArea = new Rect(0, 0, 100, 100);
            var transform = new CoordinateTransform(plotArea, 0.0, 10.0, 0.0, 100.0);

            var processed = series.ProcessEmptyPoints(points, transform);

            Assert.Equal(3, processed.Count);
            Assert.Equal(10.0, processed[0].Y);
            Assert.Equal(0.0, processed[1].Y);
            Assert.Equal(30.0, processed[2].Y);
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void EmptyPointMode_Average_ReplacesNaNWithAverage()
        {
            var series = new LineSeries
            {
                EmptyPointMode = EmptyPointMode.Average
            };

            var points = new List<Point>
            {
                new Point(0, 10),
                new Point(1, double.NaN),
                new Point(2, 30)
            };

            var plotArea = new Rect(0, 0, 100, 100);
            var transform = new CoordinateTransform(plotArea, 0.0, 10.0, 0.0, 100.0);

            var processed = series.ProcessEmptyPoints(points, transform);

            Assert.Equal(3, processed.Count);
            Assert.Equal(10.0, processed[0].Y);
            Assert.Equal(20.0, processed[1].Y); // Average of 10 and 30 is 20
            Assert.Equal(30.0, processed[2].Y);
        }

        [Fact(Skip = "Requires UI dispatcher thread context.")]
        public void EmptyPointMode_Gap_SplitsSegmentsCorrectly()
        {
            var series = new LineSeries
            {
                EmptyPointMode = EmptyPointMode.Gap
            };

            var points = new List<Point>
            {
                new Point(0, 10),
                new Point(1, double.NaN),
                new Point(2, 30),
                new Point(3, 40)
            };

            var plotArea = new Rect(0, 0, 100, 100);
            var transform = new CoordinateTransform(plotArea, 0.0, 10.0, 0.0, 100.0);

            var processed = series.ProcessEmptyPoints(points, transform);
            var segments = series.BuildSegments(processed);

            Assert.Equal(2, segments.Count);
            Assert.Single(segments[0]);
            Assert.Equal(10.0, segments[0][0].Y);

            Assert.Equal(2, segments[1].Count);
            Assert.Equal(30.0, segments[1][0].Y);
            Assert.Equal(40.0, segments[1][1].Y);
        }
    }
}
