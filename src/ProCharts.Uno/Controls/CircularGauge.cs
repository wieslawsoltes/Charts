using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public partial class CircularGauge : ChartBase
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(CircularGauge), new PropertyMetadata(0.0, OnPropertyChanged));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(CircularGauge), new PropertyMetadata(0.0, OnPropertyChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(CircularGauge), new PropertyMetadata(100.0, OnPropertyChanged));

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit), typeof(string), typeof(CircularGauge), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty GaugeThicknessProperty =
            DependencyProperty.Register(nameof(GaugeThickness), typeof(double), typeof(CircularGauge), new PropertyMetadata(16.0, OnPropertyChanged));

        public double Value { get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double Minimum { get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double Maximum { get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public string? Unit { get => (string?)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        public double GaugeThickness { get => (double)GetValue(GaugeThicknessProperty);
            set => SetValue(GaugeThicknessProperty, value);
        }

        public CircularGauge()
        {
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

        protected override void RenderChart(SKCanvas context)
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
            var bgSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#20FFFFFF")), GaugeThickness, lineCap: SKStrokeCap.Round);
            var bgGeom = new SKPath();
            using (var ctx = bgGeom.Open())
            {
                var ptStart = center + new Point(Math.Cos(startAngleRad), Math.Sin(startAngleRad)) * radius;
                bgGeom.MoveTo(ptStart, false);
                // Draw arc to full end
                double step = 5.0 * Math.PI / 180.0;
                for (double a = startAngleRad + step; a <= fullEndAngleRad; a += step)
                {
                    var pt = center + new Point(Math.Cos(a), Math.Sin(a)) * radius;
                    bgGeom.LineTo(pt);
                }
                // Ensure precise endpoint
                var ptEnd = center + new Point(Math.Cos(fullEndAngleRad), Math.Sin(fullEndAngleRad)) * radius;
                bgGeom.LineTo(ptEnd);
            }
            context.DrawGeometry(null, bgSKPaint, bgGeom);

            // 2. Draw SKColored Progress Arc (High-quality cyan-to-purple gradient or solid)
            var progressSKPaint = Palette?.GetSKPaint(0) ?? new LinearGradientSKPaint
            {
                StartPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(SKColor.Parse("#06B6D4"), 0.0), // Cyan
                    new GradientStop(SKColor.Parse("#A855F7"), 1.0)  // Purple
                }
            };

            var progressPen = new Pen(progressSKPaint, GaugeThickness, lineCap: SKStrokeCap.Round);
            if (pct > 0.001)
            {
                var pgGeom = new SKPath();
                using (var ctx = pgGeom.Open())
                {
                    var ptStart = center + new Point(Math.Cos(startAngleRad), Math.Sin(startAngleRad)) * radius;
                    pgGeom.MoveTo(ptStart, false);
                    double step = 5.0 * Math.PI / 180.0;
                    for (double a = startAngleRad + step; a <= endAngleRad; a += step)
                    {
                        var pt = center + new Point(Math.Cos(a), Math.Sin(a)) * radius;
                        pgGeom.LineTo(pt);
                    }
                    var ptEnd = center + new Point(Math.Cos(endAngleRad), Math.Sin(endAngleRad)) * radius;
                    pgGeom.LineTo(ptEnd);
                }
                context.DrawGeometry(null, progressPen, pgGeom);
            }

            // 3. Draw Needle / Needle Pin (Modern sleek pointer)
            double needleAngleRad = endAngleRad;
            var needlePt = center + new Point(Math.Cos(needleAngleRad), Math.Sin(needleAngleRad)) * (radius - 12.0);
            var needleSKPaint = new Pen(SKPaintes.White, 3.0, lineCap: SKStrokeCap.Round);
            
            // Central pin
            var pinFill = new SolidSKColorSKPaint(SKColor.Parse("#0F172A"));
            var pinStroke = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#A855F7")), 2.0);
            context.DrawEllipse(pinFill, pinStroke, center, 8.0, 8.0);
            context.DrawLine(needleSKPaint, center + new Point(Math.Cos(needleAngleRad), Math.Sin(needleAngleRad)) * 8.0, needlePt);

            // 4. Draw Digital Readouts in Center (Value & Unit)
            var valSKPaint = SystemSKPaint;
            var ftVal = new FormattedText(
                $"{targetVal:F1}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                radius * 0.45,
                valSKPaint);

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
                    new SolidSKColorSKPaint(SKColor.Parse("#94A3B8"))); // slate-400

                double ux = center.X - ftUnit.Width / 2.0;
                double uy = ty + ftVal.Height;
                context.DrawText(ftUnit, new Point(ux, uy));
            }
        }
    }
}