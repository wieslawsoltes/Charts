using System;
using Avalonia;
using Avalonia.Media;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class ProgressDonut : ChartBase
    {
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<ProgressDonut, double>(nameof(Value), 0.0);

        public static readonly StyledProperty<double> MinimumProperty =
            AvaloniaProperty.Register<ProgressDonut, double>(nameof(Minimum), 0.0);

        public static readonly StyledProperty<double> MaximumProperty =
            AvaloniaProperty.Register<ProgressDonut, double>(nameof(Maximum), 100.0);

        public static readonly StyledProperty<double> RingThicknessProperty =
            AvaloniaProperty.Register<ProgressDonut, double>(nameof(RingThickness), 16.0);

        public static readonly StyledProperty<IBrush?> ProgressBrushProperty =
            AvaloniaProperty.Register<ProgressDonut, IBrush?>(nameof(ProgressBrush));

        public static readonly StyledProperty<IBrush?> TrackBrushProperty =
            AvaloniaProperty.Register<ProgressDonut, IBrush?>(nameof(TrackBrush));

        public static readonly StyledProperty<string?> CenterTextProperty =
            AvaloniaProperty.Register<ProgressDonut, string?>(nameof(CenterText));

        public static readonly StyledProperty<string?> CenterSubTextProperty =
            AvaloniaProperty.Register<ProgressDonut, string?>(nameof(CenterSubText));

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

        public double RingThickness
        {
            get => GetValue(RingThicknessProperty);
            set => SetValue(RingThicknessProperty, value);
        }

        public IBrush? ProgressBrush
        {
            get => GetValue(ProgressBrushProperty);
            set => SetValue(ProgressBrushProperty, value);
        }

        public IBrush? TrackBrush
        {
            get => GetValue(TrackBrushProperty);
            set => SetValue(TrackBrushProperty, value);
        }

        public string? CenterText
        {
            get => GetValue(CenterTextProperty);
            set => SetValue(CenterTextProperty, value);
        }

        public string? CenterSubText
        {
            get => GetValue(CenterSubTextProperty);
            set => SetValue(CenterSubTextProperty, value);
        }

        public ProgressDonut()
        {
            ValueProperty.Changed.AddClassHandler<ProgressDonut>((x, e) => x.InvalidateVisual());
            MinimumProperty.Changed.AddClassHandler<ProgressDonut>((x, e) => x.InvalidateVisual());
            MaximumProperty.Changed.AddClassHandler<ProgressDonut>((x, e) => x.InvalidateVisual());
            RingThicknessProperty.Changed.AddClassHandler<ProgressDonut>((x, e) => x.InvalidateVisual());
            ProgressBrushProperty.Changed.AddClassHandler<ProgressDonut>((x, e) => x.InvalidateVisual());
            TrackBrushProperty.Changed.AddClassHandler<ProgressDonut>((x, e) => x.InvalidateVisual());
            CenterTextProperty.Changed.AddClassHandler<ProgressDonut>((x, e) => x.InvalidateVisual());
            CenterSubTextProperty.Changed.AddClassHandler<ProgressDonut>((x, e) => x.InvalidateVisual());
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
            var area = EffectivePlotArea;
            var center = area.Center;
            double thickness = Math.Clamp(RingThickness, 2.0, area.Width / 3.0);
            double radius = (area.Width - thickness) / 2.0;

            if (radius <= 0) return;

            // 1. Calculate percentage and angles
            double min = Minimum;
            double max = Maximum;
            if (max <= min) max = min + 1.0;

            double progressVal = AnimationProgress;
            double pct = Math.Clamp((Value - min) / (max - min), 0.0, 1.0) * progressVal;

            double startAngleDeg = -90.0; // Start at top
            double sweepAngleDeg = pct * 360.0;

            // 2. Draw Track (Background Ring)
            var bgBrush = TrackBrush ?? new SolidColorBrush(Color.Parse("#1E293B"), 0.5); // 50% opacity slate-800
            var trackPen = new Pen(bgBrush, thickness);
            context.DrawEllipse(null, trackPen, center, radius, radius);

            // 3. Draw Sweeping Progress Arc with Rounded Line Caps
            if (sweepAngleDeg > 0.01)
            {
                var fgBrush = ProgressBrush ?? Palette?.GetBrush(0) ?? new SolidColorBrush(Color.Parse("#38BDF8")); // Sky blue default
                var progressPen = new Pen(fgBrush, thickness, lineCap: PenLineCap.Round);

                if (sweepAngleDeg >= 359.9)
                {
                    // Full circle needs drawing without collapsing
                    context.DrawEllipse(null, progressPen, center, radius, radius);
                }
                else
                {
                    var geom = new StreamGeometry();
                    using (var ctx = geom.Open())
                    {
                        double radStart = startAngleDeg * Math.PI / 180.0;
                        double radEnd = (startAngleDeg + sweepAngleDeg) * Math.PI / 180.0;

                        Point startPt = center + new Point(radius * Math.Cos(radStart), radius * Math.Sin(radStart));
                        Point endPt = center + new Point(radius * Math.Cos(radEnd), radius * Math.Sin(radEnd));

                        ctx.BeginFigure(startPt, false);
                        ctx.ArcTo(endPt, new Size(radius, radius), 0.0, sweepAngleDeg > 180.0, SweepDirection.Clockwise);
                    }
                    context.DrawGeometry(null, progressPen, geom);
                }
            }

            // 4. Render Center Metrics (Text)
            var textBrush = SystemBrush;
            string mainText = CenterText ?? $"{pct * 100.0 / progressVal:F0}%";
            string subText = CenterSubText ?? "COMPLETE";

            var ftMain = new FormattedText(
                mainText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                Math.Clamp(radius * 0.45, 14, 48),
                textBrush);

            double mx = center.X - ftMain.Width / 2.0;
            double my = center.Y - ftMain.Height / 2.0;
            if (!string.IsNullOrEmpty(subText))
            {
                my -= 8;
            }

            context.DrawText(ftMain, new Point(mx, my));

            if (!string.IsNullOrEmpty(subText))
            {
                var ftSub = new FormattedText(
                    subText,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal),
                    Math.Clamp(radius * 0.16, 9, 14),
                    new SolidColorBrush(Color.Parse("#94A3B8"))); // slate-400

                double sx = center.X - ftSub.Width / 2.0;
                double sy = my + ftMain.Height + 2;
                context.DrawText(ftSub, new Point(sx, sy));
            }
        }
    }
}
