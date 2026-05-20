using System;
using Windows.Foundation;
using ProCharts.Uno.Maths;
using Xunit;

namespace ProCharts.Uno.Tests
{
    public class TransformTests
    {
        [Fact]
        public void LinearMapping_CorrectInterpolation()
        {
            var plotArea = new Rect(0, 0, 100, 100);
            var transform = new CoordinateTransform(plotArea, 0.0, 10.0, 0.0, 100.0);

            // Min bound mapping (0, 0) -> (0, 100) since Y is screen-inverted
            var ptMin = transform.ToScreen(0.0, 0.0);
            Assert.Equal(0.0, ptMin.X, 5);
            Assert.Equal(100.0, ptMin.Y, 5);

            // Max bound mapping (10, 100) -> (100, 0)
            var ptMax = transform.ToScreen(10.0, 100.0);
            Assert.Equal(100.0, ptMax.X, 5);
            Assert.Equal(0.0, ptMax.Y, 5);

            // Center mapping (5, 50) -> (50, 50)
            var ptCenter = transform.ToScreen(5.0, 50.0);
            Assert.Equal(50.0, ptCenter.X, 5);
            Assert.Equal(50.0, ptCenter.Y, 5);
        }

        [Fact]
        public void ReversedScale_CorrectInversion()
        {
            var plotArea = new Rect(0, 0, 100, 100);
            var transform = new CoordinateTransform(plotArea, 0.0, 10.0, 0.0, 100.0, isXRev: true, isYRev: true);

            // With reversed scales:
            // Min X (0.0) -> right edge (100.0)
            // Min Y (0.0) -> top edge (0.0) since screen inversion and reversed scale cancel out!
            var ptMin = transform.ToScreen(0.0, 0.0);
            Assert.Equal(100.0, ptMin.X, 5);
            Assert.Equal(0.0, ptMin.Y, 5);
        }

        [Fact]
        public void LogarithmicScale_CorrectNonLinearMapping()
        {
            var plotArea = new Rect(0, 0, 100, 100);
            var transform = new CoordinateTransform(plotArea, 1.0, 100.0, 1.0, 100.0, isXLog: true, isYLog: true);

            // Midpoint of log-scale [1, 100] is Math.Sqrt(1 * 100) = 10.0
            var ptMid = transform.ToScreen(10.0, 10.0);
            Assert.Equal(50.0, ptMid.X, 5);
            Assert.Equal(50.0, ptMid.Y, 5);
        }

        [Fact]
        public void RoundTrip_DataToScreenToData_RestoresOriginalValues()
        {
            var plotArea = new Rect(20, 20, 200, 150);
            var transform = new CoordinateTransform(plotArea, -10.0, 50.0, -100.0, 1000.0);

            var originalPoint = new Point(12.5, 450.0);
            var screenPoint = transform.ToScreen(originalPoint.X, originalPoint.Y);
            var restoredPoint = transform.ToData(screenPoint);

            Assert.Equal(originalPoint.X, restoredPoint.X, 5);
            Assert.Equal(originalPoint.Y, restoredPoint.Y, 5);
        }
    }
}
