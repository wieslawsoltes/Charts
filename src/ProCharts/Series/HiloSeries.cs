using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using ProCharts.Maths;

namespace ProCharts.Series
{
    public class HiloSeries : FinancialSeries
    {
        public static readonly StyledProperty<IBrush?> UpStrokeProperty =
            AvaloniaProperty.Register<HiloSeries, IBrush?>(nameof(UpStroke));

        public static readonly StyledProperty<IBrush?> DownStrokeProperty =
            AvaloniaProperty.Register<HiloSeries, IBrush?>(nameof(DownStroke));

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

        public HiloSeries()
        {
            UpStrokeProperty.Changed.AddClassHandler<HiloSeries>((x, e) => x.RaiseSeriesChanged());
            DownStrokeProperty.Changed.AddClassHandler<HiloSeries>((x, e) => x.RaiseSeriesChanged());
        }

        public override void RenderSeries(in SeriesRenderContext context)
        {
            var points = GetFinancialPoints();
            if (points.Count == 0) return;

            var defaultBrush = Stroke ?? context.DefaultBrush;
            var bullStroke = UpStroke ?? defaultBrush;
            var bearStroke = DownStroke ?? defaultBrush;

            double strokeThickness = StrokeThickness > 0 ? StrokeThickness : 2.0;
            var bullPen = new Pen(bullStroke, strokeThickness);
            var bearPen = new Pen(bearStroke, strokeThickness);

            double progress = context.AnimationProgress;

            foreach (var fp in points)
            {
                if (double.IsNaN(fp.High) || double.IsNaN(fp.Low)) continue;

                // Sprout animation from High/Low mid-point
                double mid = (fp.High + fp.Low) / 2.0;
                double high = mid + (fp.High - mid) * progress;
                double low = mid + (fp.Low - mid) * progress;

                bool isBullish = fp.Close >= fp.Open;
                var candlePen = isBullish ? bullPen : bearPen;

                // Screen coordinates
                var ptHigh = context.Transform.ToScreen(fp.X, high);
                var ptLow = context.Transform.ToScreen(fp.X, low);

                // Draw vertical High-Low line
                context.DrawingContext.DrawLine(candlePen, ptHigh, ptLow);
            }
        }
    }
}
