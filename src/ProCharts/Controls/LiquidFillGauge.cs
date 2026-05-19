using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class LiquidFillGauge : ChartBase
    {
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<LiquidFillGauge, double>(nameof(Value), 0.0); // 0.0 to 100.0

        public static readonly StyledProperty<IBrush?> LiquidColorProperty =
            AvaloniaProperty.Register<LiquidFillGauge, IBrush?>(nameof(LiquidColor));

        public static readonly StyledProperty<double> WaveAmplitudeProperty =
            AvaloniaProperty.Register<LiquidFillGauge, double>(nameof(WaveAmplitude), 8.0);

        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public IBrush? LiquidColor
        {
            get => GetValue(LiquidColorProperty);
            set => SetValue(LiquidColorProperty, value);
        }

        public double WaveAmplitude
        {
            get => GetValue(WaveAmplitudeProperty);
            set => SetValue(WaveAmplitudeProperty, value);
        }

        private double _waveOffset = 0.0;
        private DispatcherTimer? _waveTimer;

        public LiquidFillGauge()
        {
            ValueProperty.Changed.AddClassHandler<LiquidFillGauge>((x, e) => x.InvalidateVisual());
            LiquidColorProperty.Changed.AddClassHandler<LiquidFillGauge>((x, e) => x.InvalidateVisual());
            WaveAmplitudeProperty.Changed.AddClassHandler<LiquidFillGauge>((x, e) => x.InvalidateVisual());

            Loaded += (s, e) => StartWaveAnimation();
            Unloaded += (s, e) => StopWaveAnimation();
        }

        private void StartWaveAnimation()
        {
            StopWaveAnimation();
            _waveTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(33), // ~30 FPS wave motion
                DispatcherPriority.Render,
                (s, e) =>
                {
                    _waveOffset += 0.15; // Speed of horizontal wave movement
                    InvalidateVisual();
                });
            _waveTimer.Start();
        }

        private void StopWaveAnimation()
        {
            _waveTimer?.Stop();
            _waveTimer = null;
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double padding = 16;
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
            var area = EffectivePlotArea;
            var center = area.Center;
            double radius = area.Width / 2.0;

            double progress = AnimationProgress;
            double pct = Math.Clamp(Value * progress / 100.0, 0.0, 1.0);

            // 1. Draw Outer Glass Ring
            var ringPen = new Pen(new SolidColorBrush(Color.Parse("#40FFFFFF")), 4.0);
            var fillBg = new SolidColorBrush(Color.Parse("#10FFFFFF")); // Dark slate transparent core
            context.DrawEllipse(fillBg, ringPen, center, radius, radius);

            // 2. Wave Level Coordinate
            // pct = 0 means bottom of circle (center.Y + radius), pct = 1 means top (center.Y - radius)
            double targetY = (center.Y + radius) - (radius * 2.0 * pct);

            if (pct > 0.001)
            {
                // Create overlapping waves using mathematical path clipping or boundary intersection
                // To keep drawing fast and robust, we build a StreamGeometry that fills the bottom of the circle
                // up to targetY with a sine-wave peak profile, and clip it within the circle bounds.
                
                var waveColor1 = LiquidColor ?? new SolidColorBrush(Color.Parse("#8006B6D4")); // Transparent Cyan
                var waveColor2 = LiquidColor ?? new SolidColorBrush(Color.Parse("#B00891B2")); // Solid Cyan

                // Create a circular clipping state to keep waves clean inside the circle
                var circleGeom = new EllipseGeometry(new Rect(area.Left, area.Top, area.Width, area.Height));
                using (context.PushGeometryClip(circleGeom))
                {
                    // Draw Wave 1 (Back Wave, slightly offset)
                    DrawWavePath(context, area, targetY, _waveOffset, WaveAmplitude, waveColor1);

                    // Draw Wave 2 (Front Wave, primary offset)
                    DrawWavePath(context, area, targetY, _waveOffset + Math.PI, WaveAmplitude, waveColor2);
                }
            }

            // 3. Draw Digital Percentage Overlay in Center
            var textBrush = SystemBrush;
            var ftPct = new FormattedText(
                $"{pct * 100.0:F0}%",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                radius * 0.4,
                textBrush);

            double tx = center.X - ftPct.Width / 2.0;
            double ty = center.Y - ftPct.Height / 2.0;
            context.DrawText(ftPct, new Point(tx, ty));
        }

        private void DrawWavePath(DrawingContext context, Rect area, double targetY, double phase, double amp, IBrush brush)
        {
            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                ctx.BeginFigure(new Point(area.Left, area.Bottom), true);
                
                // Draw wave along the top boundary
                double step = 4.0;
                for (double x = area.Left; x <= area.Right; x += step)
                {
                    // Sine-wave calculation
                    double wavelength = area.Width / 1.5;
                    double y = targetY + amp * Math.Sin((x / wavelength) * 2.0 * Math.PI + phase);
                    ctx.LineTo(new Point(x, y));
                }

                // Close the shape at the bottom
                ctx.LineTo(new Point(area.Right, area.Bottom));
            }

            context.DrawGeometry(brush, null, geom);
        }
    }
}
