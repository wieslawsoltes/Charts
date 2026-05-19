using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using ProCharts.Maths;

namespace ProCharts.Series
{
    public class OhlcSeries : FinancialSeries
    {
        public static readonly StyledProperty<IBrush?> UpStrokeProperty =
            AvaloniaProperty.Register<OhlcSeries, IBrush?>(nameof(UpStroke));

        public static readonly StyledProperty<IBrush?> DownStrokeProperty =
            AvaloniaProperty.Register<OhlcSeries, IBrush?>(nameof(DownStroke));

        public static readonly StyledProperty<double> TickWidthProperty =
            AvaloniaProperty.Register<OhlcSeries, double>(nameof(TickWidth), 8.0);

        public IBrush? UpStroke
        {
            get => GetValue(UpStrokeProperty);
            set => SetValue(UpStrokeProperty, value);
        }

        public IBrush? DownStroke
        {
            get => GetValue(DownStrokeProperty);
            set => SetValue(DownStrokeProperty, value);
        }

        public double TickWidth
        {
            get => GetValue(TickWidthProperty);
            set => SetValue(TickWidthProperty, value);
        }

        public OhlcSeries()
        {
            UpStrokeProperty.Changed.AddClassHandler<OhlcSeries>((x, e) => x.RaiseSeriesChanged());
            DownStrokeProperty.Changed.AddClassHandler<OhlcSeries>((x, e) => x.RaiseSeriesChanged());
            TickWidthProperty.Changed.AddClassHandler<OhlcSeries>((x, e) => x.RaiseSeriesChanged());
        }

        public override void RenderSeries(in SeriesRenderContext context)
        {
            var points = GetFinancialPoints();
            if (points.Count == 0) return;

            var bullStroke = UpStroke ?? new SolidColorBrush(Color.Parse("#10B981"));
            var bearStroke = DownStroke ?? new SolidColorBrush(Color.Parse("#EF4444"));

            double strokeThickness = StrokeThickness > 0 ? StrokeThickness : 1.5;
            var bullPen = new Pen(bullStroke, strokeThickness);
            var bearPen = new Pen(bearStroke, strokeThickness);

            double progress = context.AnimationProgress;
            double tick = TickWidth;

            foreach (var fp in points)
            {
                if (!fp.IsValid) continue;

                // Sprout animation
                double open = fp.Open;
                double close = open + (fp.Close - open) * progress;
                double high = open + (fp.High - open) * progress;
                double low = open + (fp.Low - open) * progress;

                bool isBullish = close >= open;
                var candlePen = isBullish ? bullPen : bearPen;

                // Screen coordinates
                var ptOpen = context.Transform.ToScreen(fp.X, open);
                var ptClose = context.Transform.ToScreen(fp.X, close);
                var ptHigh = context.Transform.ToScreen(fp.X, high);
                var ptLow = context.Transform.ToScreen(fp.X, low);

                // Draw vertical High-Low line
                context.DrawingContext.DrawLine(candlePen, ptHigh, ptLow);

                // Draw left Open tick
                context.DrawingContext.DrawLine(candlePen, ptOpen, new Point(ptOpen.X - tick, ptOpen.Y));

                // Draw right Close tick
                context.DrawingContext.DrawLine(candlePen, ptClose, new Point(ptClose.X + tick, ptClose.Y));
            }
        }
    }
}
