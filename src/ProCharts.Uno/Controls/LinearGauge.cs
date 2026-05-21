using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;


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

        protected override void RenderChart(DrawingContext context)
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
            var bgBrush = new SolidColorBrush(Color.Parse("#20FFFFFF"));
            var borderBrush = new Pen(new SolidColorBrush(Color.Parse("#30FFFFFF")), 1.0);
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

            context.DrawRectangle(bgBrush, borderBrush, new RoundedRect(trackRect, trackCornerRadius));

            // Select active bar color based on thresholds
            Brush fillBrush;
            if (targetVal >= ErrorThreshold)
            {
                fillBrush = new SolidColorBrush(Color.Parse("#EF4444")); // Rose/Red
            }
            else if (targetVal >= WarningThreshold)
            {
                fillBrush = new SolidColorBrush(Color.Parse("#F59E0B")); // Amber/Yellow
            }
            else
            {
                // Optimal gradient: sleek emerald to cyan
                var brush = Palette?.GetBrush(0);
                if (brush == null)
                {
                    var lgb = new LinearGradientBrush();
                    lgb.StartPoint = new Windows.Foundation.Point(0, 0);
                    lgb.EndPoint = new Windows.Foundation.Point(1, 1);
                    lgb.GradientStops.Add(new GradientStop { Color = Color.Parse("#10B981"), Offset = 0.0 });
                    lgb.GradientStops.Add(new GradientStop { Color = Color.Parse("#06B6D4"), Offset = 1.0 });
                    fillBrush = lgb;
                }
                else
                {
                    fillBrush = brush;
                }
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
                context.DrawRectangle(fillBrush, null, new RoundedRect(fillRect, trackCornerRadius));
            }

            // Draw Warning & Error threshold lines inside or on the track
            var thresholdBrush = new Pen(new SolidColorBrush(Color.Parse("#60FFFFFF")), 1.5, new DashStyle(new[] { 3.0, 3.0 }, 0.0));
            double warningPct = (WarningThreshold - min) / (max - min);
            double errorPct = (ErrorThreshold - min) / (max - min);

            if (warningPct > 0 && warningPct < 1)
            {
                DrawThresholdLine(context, trackRect, warningPct, isHorizontal, thresholdBrush);
            }
            if (errorPct > 0 && errorPct < 1)
            {
                DrawThresholdLine(context, trackRect, errorPct, isHorizontal, thresholdBrush);
            }

            // Add text value on top
            var valBrush = SystemBrush;
            string unitText = string.IsNullOrEmpty(Unit) ? "" : $" {Unit}";
            var ftVal = new FormattedText(
                $"{targetVal:F1}{unitText} / {max:F0}{unitText}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                12,
                valBrush);

            double vx = area.Left + (area.Width - ftVal.Width) / 2.0;
            double vy = isHorizontal ? trackRect.Bottom + 6.0 : area.Top - 18.0;
            context.DrawText(ftVal, new Point(vx, vy));
        }

        private void DrawThresholdLine(DrawingContext context, Rect trackRect, double pct, bool isHorizontal, Pen pen)
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