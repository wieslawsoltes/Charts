using System;
using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Series
{
    public class CandlestickSeries : FinancialSeries
    {
        public static readonly DependencyProperty UpFillProperty =
            DependencyProperty.Register(nameof(UpFill), typeof(SKPaint), typeof(CandlestickSeries), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public static readonly DependencyProperty DownFillProperty =
            DependencyProperty.Register(nameof(DownFill), typeof(SKPaint), typeof(CandlestickSeries), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public static readonly DependencyProperty UpStrokeProperty =
            DependencyProperty.Register(nameof(UpStroke), typeof(SKPaint), typeof(CandlestickSeries), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public static readonly DependencyProperty DownStrokeProperty =
            DependencyProperty.Register(nameof(DownStroke), typeof(SKPaint), typeof(CandlestickSeries), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public static readonly DependencyProperty CandleWidthPercentProperty =
            DependencyProperty.Register(nameof(CandleWidthPercent), typeof(double), typeof(CandlestickSeries), new PropertyMetadata(0.7, OnPropertyChanged));

        public SKPaint? UpFill { get => (SKPaint?)GetValue(UpFillProperty);
            set => SetValue(UpFillProperty, value);
        }

        public SKPaint? DownFill { get => (SKPaint?)GetValue(DownFillProperty);
            set => SetValue(DownFillProperty, value);
        }

        public SKPaint? UpStroke { get => (SKPaint?)GetValue(UpStrokeProperty);
            set => SetValue(UpStrokeProperty, value);
        }

        public SKPaint? DownStroke { get => (SKPaint?)GetValue(DownStrokeProperty);
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

            // SKPaintes with beautiful default gradients or solids
            var bullFill = UpFill ?? new SolidSKColorSKPaint(SKColor.Parse("#10B981")); // Emerald Green
            var bearFill = DownFill ?? new SolidSKColorSKPaint(SKColor.Parse("#EF4444")); // Rose Red
            var bullStroke = UpStroke ?? new SolidSKColorSKPaint(SKColor.Parse("#059669"));
            var bearStroke = DownStroke ?? new SolidSKColorSKPaint(SKColor.Parse("#DC2626"));

            double strokeThickness = StrokeThickness > 0 ? StrokeThickness : 1.0;
            var bullSKPaint = new Pen(bullStroke, strokeThickness);
            var bearSKPaint = new Pen(bearStroke, strokeThickness);

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
                var fillSKPaint = isBullish ? bullFill : bearFill;
                var candleSKPaint = isBullish ? bullSKPaint : bearSKPaint;

                // Screen coordinates
                var ptOpen = context.Transform.ToScreen(fp.X, open);
                var ptClose = context.Transform.ToScreen(fp.X, close);
                var ptHigh = context.Transform.ToScreen(fp.X, high);
                var ptLow = context.Transform.ToScreen(fp.X, low);

                // Draw Wick (vertical High-Low line)
                context.Canvas.DrawLine((float)ptHigh.X, (float)ptHigh.Y, (float)ptLow.X, (float)ptLow.Y, candleSKPaint);

                // Draw Body (Open-Close rectangle)
                double topY = Math.Min(ptOpen.Y, ptClose.Y);
                double bottomY = Math.Max(ptOpen.Y, ptClose.Y);
                double height = Math.Max(1.0, bottomY - topY);

                var bodyRect = new Rect(ptOpen.X - candleWidth / 2.0, topY, candleWidth, height);
                context.Canvas.DrawRect((float)bodyRect.X, (float)bodyRect.Y, (float)bodyRect.Width, (float)bodyRect.Height, candleSKPaint ?? fillSKPaint);
            }
        }
    }
}