using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public partial class LinearGauge : ChartBase
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(LinearGauge), new PropertyMetadata(0.0, OnPropertyChanged));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(LinearGauge), new PropertyMetadata(0.0, OnPropertyChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(LinearGauge), new PropertyMetadata(100.0, OnPropertyChanged));

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(LinearGauge), new PropertyMetadata(Orientation.Horizontal, OnPropertyChanged));

        public static readonly DependencyProperty WarningThresholdProperty =
            DependencyProperty.Register(nameof(WarningThreshold), typeof(double), typeof(LinearGauge), new PropertyMetadata(75.0, OnPropertyChanged));

        public static readonly DependencyProperty ErrorThresholdProperty =
            DependencyProperty.Register(nameof(ErrorThreshold), typeof(double), typeof(LinearGauge), new PropertyMetadata(90.0, OnPropertyChanged));

        public static readonly DependencyProperty GaugeThicknessProperty =
            DependencyProperty.Register(nameof(GaugeThickness), typeof(double), typeof(LinearGauge), new PropertyMetadata(16.0, OnPropertyChanged));

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit), typeof(string), typeof(LinearGauge), new PropertyMetadata(default(string?), OnPropertyChanged));

        public double Value { get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double Minimum { get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double Maximum { get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public Orientation Orientation { get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public double WarningThreshold { get => (double)GetValue(WarningThresholdProperty);
            set => SetValue(WarningThresholdProperty, value);
        }

        public double ErrorThreshold { get => (double)GetValue(ErrorThresholdProperty);
            set => SetValue(ErrorThresholdProperty, value);
        }

        public double GaugeThickness { get => (double)GetValue(GaugeThicknessProperty);
            set => SetValue(GaugeThicknessProperty, value);
        }

        public string? Unit { get => (string?)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        public LinearGauge()
        {
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double top = 20;
            double bottom = 20;
            double left = 20;
            double right = 20;

            if (!string.IsNullOrEmpty(Title)) top += 24;

            double w = Math.Max(10, bounds.Width - left - right);
            double h = Math.Max(10, bounds.Height - top - bottom);

            return new Rect(left, top, w, h);
        }

        protected override void RenderChart(SKCanvas context)
        {
            var area = EffectivePlotArea;

            double min = Minimum;
            double max = Maximum;
            if (max <= min) max = min + 1.0;

            double progress = AnimationProgress;
            double targetVal = min + (Value - min) * progress;
            targetVal = Math.Clamp(targetVal, min, max);

            double pct = (targetVal - min) / (max - min);

            bool isHorizontal = Orientation == Orientation.Horizontal;

            // Tracks & Bands design
            double barThickness = GaugeThickness;
            
            // Draw background track (semi-transparent glassmorphic)
            var bgSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#20FFFFFF"));
            var borderSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#30FFFFFF")), 1.0);
            var trackCornerRadius = new CornerRadius(barThickness / 2.0);

            Rect trackRect;
            if (isHorizontal)
            {
                double ty = area.Top + (area.Height - barThickness) / 2.0;
                trackRect = new Rect(area.Left, ty, area.Width, barThickness);
            }
            else
            {
                double tx = area.Left + (area.Width - barThickness) / 2.0;
                trackRect = new Rect(tx, area.Top, barThickness, area.Height);
            }

            context.DrawRectangle(bgSKPaint, borderSKPaint, new RoundedRect(trackRect, trackCornerRadius));

            // Select active bar color based on thresholds
            SKPaint fillSKPaint;
            if (targetVal >= ErrorThreshold)
            {
                fillSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#EF4444")); // Rose/Red
            }
            else if (targetVal >= WarningThreshold)
            {
                fillSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#F59E0B")); // Amber/Yellow
            }
            else
            {
                // Optimal gradient: sleek emerald to cyan
                fillSKPaint = Palette?.GetSKPaint(0) ?? new LinearGradientSKPaint
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                    GradientStops =
                    {
                        new GradientStop(SKColor.Parse("#10B981"), 0.0),
                        new GradientStop(SKColor.Parse("#06B6D4"), 1.0)
                    }
                };
            }

            // Draw active value fill
            Rect fillRect;
            if (isHorizontal)
            {
                double fillWidth = area.Width * pct;
                fillRect = new Rect(trackRect.Left, trackRect.Top, fillWidth, trackRect.Height);
            }
            else
            {
                double fillHeight = area.Height * pct;
                fillRect = new Rect(trackRect.Left, trackRect.Bottom - fillHeight, trackRect.Width, fillHeight);
            }

            if (pct > 0.001)
            {
                context.DrawRectangle(fillSKPaint, null, new RoundedRect(fillRect, trackCornerRadius));
            }

            // Draw Warning & Error threshold lines inside or on the track
            var thresholdSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#60FFFFFF")), 1.5, new DashStyle(new[] { 3.0, 3.0 }, 0.0));
            double warningPct = (WarningThreshold - min) / (max - min);
            double errorPct = (ErrorThreshold - min) / (max - min);

            if (warningPct > 0 && warningPct < 1)
            {
                DrawThresholdLine(context, trackRect, warningPct, isHorizontal, thresholdSKPaint);
            }
            if (errorPct > 0 && errorPct < 1)
            {
                DrawThresholdLine(context, trackRect, errorPct, isHorizontal, thresholdSKPaint);
            }

            // Add text value on top
            var valSKPaint = SystemSKPaint;
            string unitText = string.IsNullOrEmpty(Unit) ? "" : $" {Unit}";
            var ftVal = new FormattedText(
                $"{targetVal:F1}{unitText} / {max:F0}{unitText}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                12,
                valSKPaint);

            double vx = area.Left + (area.Width - ftVal.Width) / 2.0;
            double vy = isHorizontal ? trackRect.Bottom + 6.0 : area.Top - 18.0;
            context.DrawText(ftVal, new Point(vx, vy));
        }

        private void DrawThresholdLine(SKCanvas context, Rect trackRect, double pct, bool isHorizontal, SKPaint pen)
        {
            if (isHorizontal)
            {
                double x = trackRect.Left + trackRect.Width * pct;
                context.DrawLine(pen, new Point(x, trackRect.Top), new Point(x, trackRect.Bottom));
            }
            else
            {
                double y = trackRect.Bottom - trackRect.Height * pct;
                context.DrawLine(pen, new Point(trackRect.Left, y), new Point(trackRect.Right, y));
            }
        }
    }
}