using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Maths;

namespace ProCharts.Series
{
    public class LineSeries : CartesianSeries
    {
        // --- DEPENDENCY PROPERTIES ---

        public static readonly StyledProperty<bool> IsSmoothProperty =
            AvaloniaProperty.Register<LineSeries, bool>(nameof(IsSmooth), false);

        public static readonly StyledProperty<EmptyPointMode> EmptyPointModeProperty =
            AvaloniaProperty.Register<LineSeries, EmptyPointMode>(nameof(EmptyPointMode), EmptyPointMode.Zero);

        public static readonly StyledProperty<bool> ShowMarkersProperty =
            AvaloniaProperty.Register<LineSeries, bool>(nameof(ShowMarkers), true);

        public static readonly StyledProperty<double> MarkerSizeProperty =
            AvaloniaProperty.Register<LineSeries, double>(nameof(MarkerSize), 6.0);

        public static readonly StyledProperty<IBrush?> MarkerFillProperty =
            AvaloniaProperty.Register<LineSeries, IBrush?>(nameof(MarkerFill));

        // --- PROPERTIES ---

        public bool IsSmooth
        {
            get => GetValue(IsSmoothProperty);
            set => SetValue(IsSmoothProperty, value);
        }

        public EmptyPointMode EmptyPointMode
        {
            get => GetValue(EmptyPointModeProperty);
            set => SetValue(EmptyPointModeProperty, value);
        }

        public bool ShowMarkers
        {
            get => GetValue(ShowMarkersProperty);
            set => SetValue(ShowMarkersProperty, value);
        }

        public double MarkerSize
        {
            get => GetValue(MarkerSizeProperty);
            set => SetValue(MarkerSizeProperty, value);
        }

        public IBrush? MarkerFill
        {
            get => GetValue(MarkerFillProperty);
            set => SetValue(MarkerFillProperty, value);
        }

        public LineSeries()
        {
            IsSmoothProperty.Changed.AddClassHandler<LineSeries>((x, e) => x.RaiseSeriesChanged());
            EmptyPointModeProperty.Changed.AddClassHandler<LineSeries>((x, e) => x.RaiseSeriesChanged());
            ShowMarkersProperty.Changed.AddClassHandler<LineSeries>((x, e) => x.RaiseSeriesChanged());
            MarkerSizeProperty.Changed.AddClassHandler<LineSeries>((x, e) => x.RaiseSeriesChanged());
            MarkerFillProperty.Changed.AddClassHandler<LineSeries>((x, e) => x.RaiseSeriesChanged());
        }

        public override void RenderSeries(in SeriesRenderContext context)
        {
            var rawPoints = GetDataPoints();
            if (rawPoints.Count == 0) return;

            // 1. Process Empty/NaN Points
            var processedPoints = ProcessEmptyPoints(rawPoints, context.Transform);

            // 2. Build segments for rendering (useful for Gap mode)
            var segments = BuildSegments(processedPoints);

            var lineStroke = Stroke ?? context.DefaultBrush;
            var linePen = new Pen(lineStroke, StrokeThickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

            // 3. Render each segment
            foreach (var segment in segments)
            {
                if (segment.Count < 2) continue;

                // Animate points: interpolate Y values from transform.YMin to their actual Y value
                var animatedPoints = AnimatePoints(segment, context.Transform, context.AnimationProgress);

                if (IsSmooth && animatedPoints.Count > 2)
                {
                    DrawBezierSpline(context.DrawingContext, linePen, animatedPoints);
                }
                else
                {
                    DrawStraightLines(context.DrawingContext, linePen, animatedPoints);
                }
            }

            // 4. Render Markers if active
            if (ShowMarkers)
            {
                var markerFillBrush = MarkerFill ?? lineStroke;
                var markerPen = new Pen(lineStroke, 1.0);
                double halfSize = MarkerSize / 2.0;

                foreach (var segment in segments)
                {
                    var animatedPoints = AnimatePoints(segment, context.Transform, context.AnimationProgress);
                    foreach (var pt in animatedPoints)
                    {
                        context.DrawingContext.DrawEllipse(
                            markerFillBrush,
                            markerPen,
                            pt,
                            halfSize,
                            halfSize);
                    }
                }
            }
        }

        internal List<Point> ProcessEmptyPoints(IList<Point> points, CoordinateTransform transform)
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

        internal List<List<Point>> BuildSegments(List<Point> points)
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
            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                ctx.BeginFigure(points[0], false);
                for (int i = 1; i < points.Count; i++)
                {
                    ctx.LineTo(points[i]);
                }
            }
            context.DrawGeometry(null, pen, geom);
        }

        internal void DrawBezierSpline(DrawingContext context, Pen pen, List<Point> points)
        {
            // Catmull-Rom control point calculation
            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                ctx.BeginFigure(points[0], false);
                
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
            context.DrawGeometry(null, pen, geom);
        }
    }
}
