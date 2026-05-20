using System;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class LinearGauge : ChartBase
    {
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<LinearGauge, double>(nameof(Value), 0.0);

        public static readonly StyledProperty<double> MinimumProperty =
            AvaloniaProperty.Register<LinearGauge, double>(nameof(Minimum), 0.0);

        public static readonly StyledProperty<double> MaximumProperty =
            AvaloniaProperty.Register<LinearGauge, double>(nameof(Maximum), 100.0);

        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<LinearGauge, Orientation>(nameof(Orientation), Orientation.Horizontal);

        public static readonly StyledProperty<double> WarningThresholdProperty =
            AvaloniaProperty.Register<LinearGauge, double>(nameof(WarningThreshold), 75.0);

        public static readonly StyledProperty<double> ErrorThresholdProperty =
            AvaloniaProperty.Register<LinearGauge, double>(nameof(ErrorThreshold), 90.0);

        public static readonly StyledProperty<double> GaugeThicknessProperty =
            AvaloniaProperty.Register<LinearGauge, double>(nameof(GaugeThickness), 16.0);

        public static readonly StyledProperty<string?> UnitProperty =
            AvaloniaProperty.Register<LinearGauge, string?>(nameof(Unit));

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

        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public double WarningThreshold
        {
            get => GetValue(WarningThresholdProperty);
            set => SetValue(WarningThresholdProperty, value);
        }

        public double ErrorThreshold
        {
            get => GetValue(ErrorThresholdProperty);
            set => SetValue(ErrorThresholdProperty, value);
        }

        public double GaugeThickness
        {
            get => GetValue(GaugeThicknessProperty);
            set => SetValue(GaugeThicknessProperty, value);
        }

        public string? Unit
        {
            get => GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        public LinearGauge()
        {
            ValueProperty.Changed.AddClassHandler<LinearGauge>((x, e) => x.InvalidateVisual());
            MinimumProperty.Changed.AddClassHandler<LinearGauge>((x, e) => x.InvalidateVisual());
            MaximumProperty.Changed.AddClassHandler<LinearGauge>((x, e) => x.InvalidateVisual());
            OrientationProperty.Changed.AddClassHandler<LinearGauge>((x, e) => x.InvalidateVisual());
            WarningThresholdProperty.Changed.AddClassHandler<LinearGauge>((x, e) => x.InvalidateVisual());
            ErrorThresholdProperty.Changed.AddClassHandler<LinearGauge>((x, e) => x.InvalidateVisual());
            GaugeThicknessProperty.Changed.AddClassHandler<LinearGauge>((x, e) => x.InvalidateVisual());
            UnitProperty.Changed.AddClassHandler<LinearGauge>((x, e) => x.InvalidateVisual());
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
            var borderPen = new Pen(new SolidColorBrush(Color.Parse("#30FFFFFF")), 1.0);
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

            context.DrawRectangle(bgBrush, borderPen, new RoundedRect(trackRect, trackCornerRadius));

            // Select active bar color based on thresholds
            IBrush fillBrush;
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
                fillBrush = Palette?.GetBrush(0) ?? new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                    GradientStops =
                    {
                        new GradientStop(Color.Parse("#10B981"), 0.0),
                        new GradientStop(Color.Parse("#06B6D4"), 1.0)
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
                context.DrawRectangle(fillBrush, null, new RoundedRect(fillRect, trackCornerRadius));
            }

            // Draw Warning & Error threshold lines inside or on the track
            var thresholdPen = new Pen(new SolidColorBrush(Color.Parse("#60FFFFFF")), 1.5, new DashStyle(new[] { 3.0, 3.0 }, 0.0));
            double warningPct = (WarningThreshold - min) / (max - min);
            double errorPct = (ErrorThreshold - min) / (max - min);

            if (warningPct > 0 && warningPct < 1)
            {
                DrawThresholdLine(context, trackRect, warningPct, isHorizontal, thresholdPen);
            }
            if (errorPct > 0 && errorPct < 1)
            {
                DrawThresholdLine(context, trackRect, errorPct, isHorizontal, thresholdPen);
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
