using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using ProCharts.Avalonia.Maths;

namespace ProCharts.Avalonia.Series
{
    public class CandlestickSeries : FinancialSeries
    {
        public static readonly StyledProperty<IBrush?> UpFillProperty =
            AvaloniaProperty.Register<CandlestickSeries, IBrush?>(nameof(UpFill));

        public static readonly StyledProperty<IBrush?> DownFillProperty =
            AvaloniaProperty.Register<CandlestickSeries, IBrush?>(nameof(DownFill));

        public static readonly StyledProperty<IBrush?> UpStrokeProperty =
            AvaloniaProperty.Register<CandlestickSeries, IBrush?>(nameof(UpStroke));

        public static readonly StyledProperty<IBrush?> DownStrokeProperty =
            AvaloniaProperty.Register<CandlestickSeries, IBrush?>(nameof(DownStroke));

        public static readonly StyledProperty<double> CandleWidthPercentProperty =
            AvaloniaProperty.Register<CandlestickSeries, double>(nameof(CandleWidthPercent), 0.7);

        public IBrush? UpFill
        {
            get => GetValue(UpFillProperty);
            set => SetValue(UpFillProperty, value);
        }

        public IBrush? DownFill
        {
            get => GetValue(DownFillProperty);
            set => SetValue(DownFillProperty, value);
        }

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

        public double CandleWidthPercent
        {
            get => GetValue(CandleWidthPercentProperty);
            set => SetValue(CandleWidthPercentProperty, value);
        }

        public CandlestickSeries()
        {
            UpFillProperty.Changed.AddClassHandler<CandlestickSeries>((x, e) => x.RaiseSeriesChanged());
            DownFillProperty.Changed.AddClassHandler<CandlestickSeries>((x, e) => x.RaiseSeriesChanged());
            UpStrokeProperty.Changed.AddClassHandler<CandlestickSeries>((x, e) => x.RaiseSeriesChanged());
            DownStrokeProperty.Changed.AddClassHandler<CandlestickSeries>((x, e) => x.RaiseSeriesChanged());
            CandleWidthPercentProperty.Changed.AddClassHandler<CandlestickSeries>((x, e) => x.RaiseSeriesChanged());
        }

        public override void RenderSeries(in SeriesRenderContext context)
        {
            var points = GetFinancialPoints();
            if (points.Count == 0) return;

            // Brushes with beautiful default gradients or solids
            var bullFill = UpFill ?? new SolidColorBrush(Color.Parse("#10B981")); // Emerald Green
            var bearFill = DownFill ?? new SolidColorBrush(Color.Parse("#EF4444")); // Rose Red
            var bullStroke = UpStroke ?? new SolidColorBrush(Color.Parse("#059669"));
            var bearStroke = DownStroke ?? new SolidColorBrush(Color.Parse("#DC2626"));

            double strokeThickness = StrokeThickness > 0 ? StrokeThickness : 1.0;
            var bullPen = new Pen(bullStroke, strokeThickness);
            var bearPen = new Pen(bearStroke, strokeThickness);

            // Determine slot width on screen
            double slotWidth = context.PlotArea.Width / Math.Max(1, points.Count);
            double candleWidth = Math.Clamp(slotWidth * CandleWidthPercent, 2.0, 50.0);

            double progress = context.AnimationProgress;

            foreach (var fp in points)
            {
                if (!fp.IsValid) continue;

                // Sprout animation from Open price
                double open = fp.Open;
                double close = open + (fp.Close - open) * progress;
                double high = open + (fp.High - open) * progress;
                double low = open + (fp.Low - open) * progress;

                bool isBullish = close >= open;
                var fillBrush = isBullish ? bullFill : bearFill;
                var candlePen = isBullish ? bullPen : bearPen;

                // Screen coordinates
                var ptOpen = context.Transform.ToScreen(fp.X, open);
                var ptClose = context.Transform.ToScreen(fp.X, close);
                var ptHigh = context.Transform.ToScreen(fp.X, high);
                var ptLow = context.Transform.ToScreen(fp.X, low);

                // Draw Wick (vertical High-Low line)
                context.DrawingContext.DrawLine(candlePen, ptHigh, ptLow);

                // Draw Body (Open-Close rectangle)
                double topY = Math.Min(ptOpen.Y, ptClose.Y);
                double bottomY = Math.Max(ptOpen.Y, ptClose.Y);
                double height = Math.Max(1.0, bottomY - topY);

                var bodyRect = new Rect(ptOpen.X - candleWidth / 2.0, topY, candleWidth, height);
                context.DrawingContext.DrawRectangle(fillBrush, candlePen, bodyRect);
            }
        }
    }
}
