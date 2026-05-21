using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Series
{
    public class LineSeries : CartesianSeries
    {
        // --- DEPENDENCY PROPERTIES ---

        public static readonly DependencyProperty IsSmoothProperty =
            DependencyProperty.Register(nameof(IsSmooth), typeof(bool), typeof(LineSeries), new PropertyMetadata(false, OnPropertyChanged));

        public static readonly DependencyProperty EmptyPointModeProperty =
            DependencyProperty.Register(nameof(EmptyPointMode), typeof(EmptyPointMode), typeof(LineSeries), new PropertyMetadata(EmptyPointMode.Zero, OnPropertyChanged));

        public static readonly DependencyProperty ShowMarkersProperty =
            DependencyProperty.Register(nameof(ShowMarkers), typeof(bool), typeof(LineSeries), new PropertyMetadata(true, OnPropertyChanged));

        public static readonly DependencyProperty MarkerSizeProperty =
            DependencyProperty.Register(nameof(MarkerSize), typeof(double), typeof(LineSeries), new PropertyMetadata(6.0, OnPropertyChanged));

        public static readonly DependencyProperty MarkerFillProperty =
            DependencyProperty.Register(nameof(MarkerFill), typeof(Brush), typeof(LineSeries), new PropertyMetadata(default(Brush?), OnPropertyChanged));

        // --- PROPERTIES ---

        public bool IsSmooth { get => (bool)GetValue(IsSmoothProperty);
            set => SetValue(IsSmoothProperty, value);
        }

        public EmptyPointMode EmptyPointMode { get => (EmptyPointMode)GetValue(EmptyPointModeProperty);
            set => SetValue(EmptyPointModeProperty, value);
        }

        public bool ShowMarkers { get => (bool)GetValue(ShowMarkersProperty);
            set => SetValue(ShowMarkersProperty, value);
        }

        public double MarkerSize { get => (double)GetValue(MarkerSizeProperty);
            set => SetValue(MarkerSizeProperty, value);
        }

        public Brush? MarkerFill { get => (Brush?)GetValue(MarkerFillProperty);
            set => SetValue(MarkerFillProperty, value);
        }

        public LineSeries()
        {
        }

        public override void RenderSeries(SeriesRenderContext context)
        {
            var rawPoints = GetDataPoints();
            if (rawPoints.Count == 0) return;

            // 1. Process Empty/NaN Points
            var processedPoints = ProcessEmptyPoints(rawPoints, context.Transform);

            // 2. Build segments for rendering (useful for Gap mode)
            var segments = BuildSegments(processedPoints);

            var lineStroke = Stroke ?? context.DefaultBrush;
            var lineBrush = new Pen(lineStroke, StrokeThickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

            // 3. Render each segment
            foreach (var segment in segments)
            {
                if (segment.Count < 2) continue;

                // Animate points: interpolate Y values from transform.YMin to their actual Y value
                var animatedPoints = AnimatePoints(segment, context.Transform, context.AnimationProgress);

                if (IsSmooth && animatedPoints.Count > 2)
                {
                    DrawBezierSpline(context.Canvas, lineBrush, animatedPoints);
                }
                else
                {
                    DrawStraightLines(context.Canvas, lineBrush, animatedPoints);
                }
            }

            // 4. Render Markers if active
            if (ShowMarkers)
            {
                var markerFillBrush = MarkerFill ?? lineStroke;
                var markerBrush = new Pen(lineStroke, 1.0);
                double halfSize = MarkerSize / 2.0;

                foreach (var segment in segments)
                {
                    var animatedPoints = AnimatePoints(segment, context.Transform, context.AnimationProgress);
                    foreach (var pt in animatedPoints)
                    {
                        context.Canvas.DrawEllipse(markerFillBrush, markerBrush, pt, halfSize, halfSize);
                    }
                }
            }
        }

        public List<Point> ProcessEmptyPoints(IList<Point> points, CoordinateTransform transform)
        {
            var result = new List<Point>(points.Count);

            for (int i = 0; i < points.Count; i++)
            {
                var pt = points[i];
                if (!double.IsNaN(pt.Y))
                {
                    result.Add(pt);
                    continue;
                }

                // Handle NaN Y-value based on EmptyPointMode
                switch (EmptyPointMode)
                {
                    case EmptyPointMode.Zero:
                        result.Add(new Point(pt.X, 0.0));
                        break;

                    case EmptyPointMode.Average:
                        double leftY = FindNearestValidY(points, i, -1);
                        double rightY = FindNearestValidY(points, i, 1);
                        double avgY = (double.IsNaN(leftY) ? 0.0 : leftY) + (double.IsNaN(rightY) ? 0.0 : rightY);
                        avgY = !double.IsNaN(leftY) && !double.IsNaN(rightY) ? avgY / 2.0 : avgY;
                        result.Add(new Point(pt.X, avgY));
                        break;

                    case EmptyPointMode.Interpolate:
                        // Keep as NaN for segment processing (we will interpolate over it)
                        result.Add(pt);
                        break;

                    case EmptyPointMode.Gap:
                        // Keep as NaN (will split segments)
                        result.Add(pt);
                        break;
                }
            }

            return result;
        }

        private double FindNearestValidY(IList<Point> points, int startIndex, int step)
        {
            for (int i = startIndex + step; i >= 0 && i < points.Count; i += step)
            {
                if (!double.IsNaN(points[i].Y)) return points[i].Y;
            }
            return double.NaN;
        }

        public List<List<Point>> BuildSegments(List<Point> points)
        {
            var segments = new List<List<Point>>();
            var currentSegment = new List<Point>();

            foreach (var pt in points)
            {
                if (double.IsNaN(pt.Y))
                {
                    if (EmptyPointMode == EmptyPointMode.Gap)
                    {
                        if (currentSegment.Count > 0)
                        {
                            segments.Add(currentSegment);
                            currentSegment = new List<Point>();
                        }
                    }
                    // For Interpolate, we just skip adding it to segments so it draws seamlessly
                }
                else
                {
                    currentSegment.Add(pt);
                }
            }

            if (currentSegment.Count > 0)
            {
                segments.Add(currentSegment);
            }

            return segments;
        }

        internal List<Point> AnimatePoints(List<Point> segmentPoints, CoordinateTransform transform, double progress)
        {
            var animated = new List<Point>(segmentPoints.Count);
            double baseline = transform.YMin;
            if (baseline < 0 && transform.YMax > 0) baseline = 0.0; // Draw upward/downward from zero if zero is in view

            foreach (var pt in segmentPoints)
            {
                // Interpolate Y value mathematically
                double animatedY = baseline + (pt.Y - baseline) * progress;
                animated.Add(transform.ToScreen(pt.X, animatedY));
            }

            return animated;
        }

        internal void DrawStraightLines(DrawingContext context, Pen pen, List<Point> points)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                geometry.MoveTo(points[0], false);
                for (int i = 1; i < points.Count; i++)
                {
                    geometry.LineTo(points[i]);
                }
            }
            context.DrawGeometry(null, pen, geometry);
        }

        internal void DrawBezierSpline(DrawingContext context, Pen pen, List<Point> points)
        {
            // Catmull-Rom control point calculation
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                geometry.MoveTo(points[0], false);
                
                const double smoothness = 0.4;
                for (int i = 0; i < points.Count - 1; i++)
                {
                    var p0 = i == 0 ? points[i] : points[i - 1];
                    var p1 = points[i];
                    var p2 = points[i + 1];
                    var p3 = i + 2 < points.Count ? points[i + 2] : p2;

                    var cp1 = p1 + (p2 - p0) * (smoothness / 3.0);
                    var cp2 = p2 - (p3 - p1) * (smoothness / 3.0);

                    ctx.CubicBezierTo(cp1, cp2, p2);
                }
            }
            context.DrawGeometry(null, pen, geometry);
        }
    }
}