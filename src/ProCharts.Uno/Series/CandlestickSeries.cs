using System;
using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Series
{
    public class CandlestickSeries : FinancialSeries
    {
        public static readonly DependencyProperty UpFillProperty =
            DependencyProperty.Register(nameof(UpFill), typeof(Brush), typeof(CandlestickSeries), new PropertyMetadata(default(Brush?), OnPropertyChanged));

        public static readonly DependencyProperty DownFillProperty =
            DependencyProperty.Register(nameof(DownFill), typeof(Brush), typeof(CandlestickSeries), new PropertyMetadata(default(Brush?), OnPropertyChanged));

        public static readonly DependencyProperty UpStrokeProperty =
            DependencyProperty.Register(nameof(UpStroke), typeof(Brush), typeof(CandlestickSeries), new PropertyMetadata(default(Brush?), OnPropertyChanged));

        public static readonly DependencyProperty DownStrokeProperty =
            DependencyProperty.Register(nameof(DownStroke), typeof(Brush), typeof(CandlestickSeries), new PropertyMetadata(default(Brush?), OnPropertyChanged));

        public static readonly DependencyProperty CandleWidthPercentProperty =
            DependencyProperty.Register(nameof(CandleWidthPercent), typeof(double), typeof(CandlestickSeries), new PropertyMetadata(0.7, OnPropertyChanged));

        public Brush? UpFill { get => (Brush?)GetValue(UpFillProperty);
            set => SetValue(UpFillProperty, value);
        }

        public Brush? DownFill { get => (Brush?)GetValue(DownFillProperty);
            set => SetValue(DownFillProperty, value);
        }

        public Brush? UpStroke { get => (Brush?)GetValue(UpStrokeProperty);
            set => SetValue(UpStrokeProperty, value);
        }

        public Brush? DownStroke { get => (Brush?)GetValue(DownStrokeProperty);
            set => SetValue(DownStrokeProperty, value);
        }

        public double CandleWidthPercent { get => (double)GetValue(CandleWidthPercentProperty);
            set => SetValue(CandleWidthPercentProperty, value);
        }

        public CandlestickSeries()
        {
        }

        public override void RenderSeries(SeriesRenderContext context)
        {
            var points = GetFinancialPoints();
            if (points.Count == 0) return;

            // Brushes with beautiful default gradients or solids
            var bullFill = UpFill ?? new SolidColorBrush(Color.Parse("#10B981")); // Emerald Green
            var bearFill = DownFill ?? new SolidColorBrush(Color.Parse("#EF4444")); // Rose Red
            var bullStroke = UpStroke ?? new SolidColorBrush(Color.Parse("#059669"));
            var bearStroke = DownStroke ?? new SolidColorBrush(Color.Parse("#DC2626"));

            double strokeThickness = StrokeThickness > 0 ? StrokeThickness : 1.0;
            var bullBrush = new Pen(bullStroke, strokeThickness);
            var bearBrush = new Pen(bearStroke, strokeThickness);

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
                var candleBrush = isBullish ? bullBrush : bearBrush;

                // Screen coordinates
                var ptOpen = context.Transform.ToScreen(fp.X, open);
                var ptClose = context.Transform.ToScreen(fp.X, close);
                var ptHigh = context.Transform.ToScreen(fp.X, high);
                var ptLow = context.Transform.ToScreen(fp.X, low);

                // Draw Wick (vertical High-Low line)
                context.Canvas.DrawLine((float)ptHigh.X, (float)ptHigh.Y, (float)ptLow.X, (float)ptLow.Y, candleBrush);

                // Draw Body (Open-Close rectangle)
                double topY = Math.Min(ptOpen.Y, ptClose.Y);
                double bottomY = Math.Max(ptOpen.Y, ptClose.Y);
                double height = Math.Max(1.0, bottomY - topY);

                var bodyRect = new Rect(ptOpen.X - candleWidth / 2.0, topY, candleWidth, height);
                context.Canvas.DrawRectangle(fillBrush, candleBrush, bodyRect);
            }
        }
    }
}