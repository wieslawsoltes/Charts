using System;
using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Series
{
    public class OhlcSeries : FinancialSeries
    {
        public static readonly DependencyProperty UpStrokeProperty =
            DependencyProperty.Register(nameof(UpStroke), typeof(Brush), typeof(OhlcSeries), new PropertyMetadata(default(Brush?), OnPropertyChanged));

        public static readonly DependencyProperty DownStrokeProperty =
            DependencyProperty.Register(nameof(DownStroke), typeof(Brush), typeof(OhlcSeries), new PropertyMetadata(default(Brush?), OnPropertyChanged));

        public static readonly DependencyProperty TickWidthProperty =
            DependencyProperty.Register(nameof(TickWidth), typeof(double), typeof(OhlcSeries), new PropertyMetadata(8.0, OnPropertyChanged));

        public Brush? UpStroke { get => (Brush?)GetValue(UpStrokeProperty);
            set => SetValue(UpStrokeProperty, value);
        }

        public Brush? DownStroke { get => (Brush?)GetValue(DownStrokeProperty);
            set => SetValue(DownStrokeProperty, value);
        }

        public double TickWidth { get => (double)GetValue(TickWidthProperty);
            set => SetValue(TickWidthProperty, value);
        }

        public OhlcSeries()
        {
        }

        public override void RenderSeries(SeriesRenderContext context)
        {
            var points = GetFinancialPoints();
            if (points.Count == 0) return;

            var bullStroke = UpStroke ?? new SolidColorBrush(Color.Parse("#10B981"));
            var bearStroke = DownStroke ?? new SolidColorBrush(Color.Parse("#EF4444"));

            double strokeThickness = StrokeThickness > 0 ? StrokeThickness : 1.5;
            var bullBrush = new Pen(bullStroke, strokeThickness);
            var bearBrush = new Pen(bearStroke, strokeThickness);

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
                var candleBrush = isBullish ? bullBrush : bearBrush;

                // Screen coordinates
                var ptOpen = context.Transform.ToScreen(fp.X, open);
                var ptClose = context.Transform.ToScreen(fp.X, close);
                var ptHigh = context.Transform.ToScreen(fp.X, high);
                var ptLow = context.Transform.ToScreen(fp.X, low);

                // Draw vertical High-Low line
                context.Canvas.DrawLine((float)ptHigh.X, (float)ptHigh.Y, (float)ptLow.X, (float)ptLow.Y, candleBrush);

                // Draw left Open tick
                context.Canvas.DrawLine(candleBrush, ptOpen, new Point(ptOpen.X - tick, ptOpen.Y));

                // Draw right Close tick
                context.Canvas.DrawLine(candleBrush, ptClose, new Point(ptClose.X + tick, ptClose.Y));
            }
        }
    }
}