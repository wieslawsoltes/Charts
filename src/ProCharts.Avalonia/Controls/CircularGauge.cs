using System;
using Avalonia;
using Avalonia.Media;
using ProCharts.Avalonia.Styles;

namespace ProCharts.Avalonia.Controls
{
    public class CircularGauge : ChartBase
    {
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<CircularGauge, double>(nameof(Value), 0.0);

        public static readonly StyledProperty<double> MinimumProperty =
            AvaloniaProperty.Register<CircularGauge, double>(nameof(Minimum), 0.0);

        public static readonly StyledProperty<double> MaximumProperty =
            AvaloniaProperty.Register<CircularGauge, double>(nameof(Maximum), 100.0);

        public static readonly StyledProperty<string?> UnitProperty =
            AvaloniaProperty.Register<CircularGauge, string?>(nameof(Unit));

        public static readonly StyledProperty<double> GaugeThicknessProperty =
            AvaloniaProperty.Register<CircularGauge, double>(nameof(GaugeThickness), 16.0);

        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public string? Unit
        {
            get => GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        public double GaugeThickness
        {
            get => GetValue(GaugeThicknessProperty);
            set => SetValue(GaugeThicknessProperty, value);
        }

        public CircularGauge()
        {
            ValueProperty.Changed.AddClassHandler<CircularGauge>((x, e) => x.InvalidateVisual());
            MinimumProperty.Changed.AddClassHandler<CircularGauge>((x, e) => x.InvalidateVisual());
            MaximumProperty.Changed.AddClassHandler<CircularGauge>((x, e) => x.InvalidateVisual());
            UnitProperty.Changed.AddClassHandler<CircularGauge>((x, e) => x.InvalidateVisual());
            GaugeThicknessProperty.Changed.AddClassHandler<CircularGauge>((x, e) => x.InvalidateVisual());
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double padding = 20;
            if (!string.IsNullOrEmpty(Title)) padding += 24;

            double side = Math.Min(bounds.Width, bounds.Height) - padding * 2;
            side = Math.Max(10, side);

            double cx = (bounds.Width - side) / 2.0;
            double cy = (bounds.Height - side) / 2.0;

            if (!string.IsNullOrEmpty(Title)) cy += 12;

            return new Rect(cx, cy, side, side);
        }

        protected override void RenderChart(DrawingContext context)
        {
            var center = EffectivePlotArea.Center;
            double radius = (EffectivePlotArea.Width - GaugeThickness) / 2.0;

            double min = Minimum;
            double max = Maximum;
            if (max <= min) max = min + 1.0;

            double progress = AnimationProgress;
            double targetVal = min + (Value - min) * progress;
            targetVal = Math.Clamp(targetVal, min, max);

            double pct = (targetVal - min) / (max - min);

            // Define gauge angles: standard radial speedometers sweep from 135 deg to 405 deg (total 270 sweep)
            double startAngleRad = 135.0 * Math.PI / 180.0;
            double sweepAngleTotal = 270.0;
            double endAngleRad = (135.0 + sweepAngleTotal * pct) * Math.PI / 180.0;
            double fullEndAngleRad = (135.0 + sweepAngleTotal) * Math.PI / 180.0;

            // 1. Draw Background Track (Glassmorphic look)
            var bgPen = new Pen(new SolidColorBrush(Color.Parse("#20FFFFFF")), GaugeThickness, lineCap: PenLineCap.Round);
            var bgGeom = new StreamGeometry();
            using (var ctx = bgGeom.Open())
            {
                var ptStart = center + new Point(Math.Cos(startAngleRad), Math.Sin(startAngleRad)) * radius;
                ctx.BeginFigure(ptStart, false);
                // Draw arc to full end
                double step = 5.0 * Math.PI / 180.0;
                for (double a = startAngleRad + step; a <= fullEndAngleRad; a += step)
                {
                    var pt = center + new Point(Math.Cos(a), Math.Sin(a)) * radius;
                    ctx.LineTo(pt);
                }
                // Ensure precise endpoint
                var ptEnd = center + new Point(Math.Cos(fullEndAngleRad), Math.Sin(fullEndAngleRad)) * radius;
                ctx.LineTo(ptEnd);
            }
            context.DrawGeometry(null, bgPen, bgGeom);

            // 2. Draw Colored Progress Arc (High-quality cyan-to-purple gradient or solid)
            var progressBrush = Palette?.GetBrush(0) ?? new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.Parse("#06B6D4"), 0.0), // Cyan
                    new GradientStop(Color.Parse("#A855F7"), 1.0)  // Purple
                }
            };

            var progressPen = new Pen(progressBrush, GaugeThickness, lineCap: PenLineCap.Round);
            if (pct > 0.001)
            {
                var pgGeom = new StreamGeometry();
                using (var ctx = pgGeom.Open())
                {
                    var ptStart = center + new Point(Math.Cos(startAngleRad), Math.Sin(startAngleRad)) * radius;
                    ctx.BeginFigure(ptStart, false);
                    double step = 5.0 * Math.PI / 180.0;
                    for (double a = startAngleRad + step; a <= endAngleRad; a += step)
                    {
                        var pt = center + new Point(Math.Cos(a), Math.Sin(a)) * radius;
                        ctx.LineTo(pt);
                    }
                    var ptEnd = center + new Point(Math.Cos(endAngleRad), Math.Sin(endAngleRad)) * radius;
                    ctx.LineTo(ptEnd);
                }
                context.DrawGeometry(null, progressPen, pgGeom);
            }

            // 3. Draw Needle / Needle Pin (Modern sleek pointer)
            double needleAngleRad = endAngleRad;
            var needlePt = center + new Point(Math.Cos(needleAngleRad), Math.Sin(needleAngleRad)) * (radius - 12.0);
            var needlePen = new Pen(Brushes.White, 3.0, lineCap: PenLineCap.Round);
            
            // Central pin
            var pinBrush = new SolidColorBrush(Color.Parse("#0F172A"));
            var pinPen = new Pen(new SolidColorBrush(Color.Parse("#A855F7")), 2.0);
            context.DrawEllipse(pinBrush, pinPen, center, 8.0, 8.0);
            context.DrawLine(needlePen, center + new Point(Math.Cos(needleAngleRad), Math.Sin(needleAngleRad)) * 8.0, needlePt);

            // 4. Draw Digital Readouts in Center (Value & Unit)
            var valBrush = SystemBrush;
            var ftVal = new FormattedText(
                $"{targetVal:F1}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                radius * 0.45,
                valBrush);

            double tx = center.X - ftVal.Width / 2.0;
            double ty = center.Y - ftVal.Height / 2.0 - (string.IsNullOrEmpty(Unit) ? 0.0 : 6.0);
            context.DrawText(ftVal, new Point(tx, ty));

            if (!string.IsNullOrEmpty(Unit))
            {
                var ftUnit = new FormattedText(
                    Unit,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Medium),
                    radius * 0.2,
                    new SolidColorBrush(Color.Parse("#94A3B8"))); // slate-400

                double ux = center.X - ftUnit.Width / 2.0;
                double uy = ty + ftVal.Height;
                context.DrawText(ftUnit, new Point(ux, uy));
            }
        }
    }
}
