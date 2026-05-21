using System;
using Avalonia;
using Avalonia.Media;

namespace ProCharts.Avalonia.Series
{
    public class PieSeries : ChartSeries
    {
        // --- DEPENDENCY PROPERTIES ---

        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<PieSeries, double>(nameof(Value), 0.0);

        public static readonly StyledProperty<bool> ExplodedProperty =
            AvaloniaProperty.Register<PieSeries, bool>(nameof(Exploded), false);

        public static readonly StyledProperty<double> ExplodeDistanceProperty =
            AvaloniaProperty.Register<PieSeries, double>(nameof(ExplodeDistance), 8.0);

        // --- PROPERTIES ---

        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public bool Exploded
        {
            get => GetValue(ExplodedProperty);
            set => SetValue(ExplodedProperty, value);
        }

        public double ExplodeDistance
        {
            get => GetValue(ExplodeDistanceProperty);
            set => SetValue(ExplodeDistanceProperty, value);
        }

        public PieSeries()
        {
            ValueProperty.Changed.AddClassHandler<PieSeries>((x, e) => x.RaiseSeriesChanged());
            ExplodedProperty.Changed.AddClassHandler<PieSeries>((x, e) => x.RaiseSeriesChanged());
            ExplodeDistanceProperty.Changed.AddClassHandler<PieSeries>((x, e) => x.RaiseSeriesChanged());
        }

        /// <summary>
        /// Renders a circular sector slice with dynamic center explosion and donut settings.
        /// </summary>
        public void RenderSlice(
            DrawingContext context,
            Point center,
            double outerRadius,
            double innerRadius,
            double startAngle,
            double sweepAngle,
            IBrush defaultBrush)
        {
            if (sweepAngle <= 0.01) return;

            // 1. Calculate Exploded Center Offset
            if (Exploded)
            {
                double bisectAngle = startAngle + sweepAngle / 2.0;
                double radBisect = bisectAngle * Math.PI / 180.0;
                double dx = ExplodeDistance * Math.Cos(radBisect);
                double dy = ExplodeDistance * Math.Sin(radBisect);
                center = new Point(center.X + dx, center.Y + dy);
            }

            var fillBrush = Fill ?? defaultBrush;
            var strokeBrush = Stroke ?? Brushes.Transparent;
            var strokePen = new Pen(strokeBrush, StrokeThickness);

            // Handle full circles (near 360 degrees) to prevent ArcTo collapse
            if (sweepAngle >= 359.9)
            {
                if (innerRadius <= 0.0)
                {
                    context.DrawEllipse(fillBrush, strokePen, center, outerRadius, outerRadius);
                }
                else
                {
                    // Draw donut using nested geometry subtraction
                    var outerGeom = new EllipseGeometry(new Rect(center.X - outerRadius, center.Y - outerRadius, outerRadius * 2, outerRadius * 2));
                    var innerGeom = new EllipseGeometry(new Rect(center.X - innerRadius, center.Y - innerRadius, innerRadius * 2, innerRadius * 2));
                    var donutGeom = new CombinedGeometry(GeometryCombineMode.Exclude, outerGeom, innerGeom);
                    context.DrawGeometry(fillBrush, strokePen, donutGeom);
                }
                return;
            }

            // 2. Draw Donut or Pie Sector using StreamGeometry
            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                double radStart = startAngle * Math.PI / 180.0;
                double radEnd = (startAngle + sweepAngle) * Math.PI / 180.0;

                Point pos = center + new Point(outerRadius * Math.Cos(radStart), outerRadius * Math.Sin(radStart));
                Point poe = center + new Point(outerRadius * Math.Cos(radEnd), outerRadius * Math.Sin(radEnd));

                bool isLargeArc = sweepAngle > 180.0;

                if (innerRadius <= 0.0)
                {
                    // --- STANDARD PIE SLICE ---
                    ctx.BeginFigure(center, true);
                    ctx.LineTo(pos);
                    ctx.ArcTo(poe, new Size(outerRadius, outerRadius), 0.0, isLargeArc, SweepDirection.Clockwise);
                }
                else
                {
                    // --- HOLLOW DONUT SLICE ---
                    Point pis = center + new Point(innerRadius * Math.Cos(radStart), innerRadius * Math.Sin(radStart));
                    Point pie = center + new Point(innerRadius * Math.Cos(radEnd), innerRadius * Math.Sin(radEnd));

                    ctx.BeginFigure(pis, true);
                    ctx.LineTo(pos);
                    ctx.ArcTo(poe, new Size(outerRadius, outerRadius), 0.0, isLargeArc, SweepDirection.Clockwise);
                    ctx.LineTo(pie);
                    ctx.ArcTo(pis, new Size(innerRadius, innerRadius), 0.0, isLargeArc, SweepDirection.CounterClockwise);
                }
            }

            context.DrawGeometry(fillBrush, strokePen, geom);
        }
    }
}
