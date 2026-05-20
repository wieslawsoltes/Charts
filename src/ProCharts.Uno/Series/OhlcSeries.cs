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
    public class OhlcSeries : FinancialSeries
    {
        public static readonly DependencyProperty UpStrokeProperty =
            DependencyProperty.Register(nameof(UpStroke), typeof(SKPaint), typeof(OhlcSeries), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public static readonly DependencyProperty DownStrokeProperty =
            DependencyProperty.Register(nameof(DownStroke), typeof(SKPaint), typeof(OhlcSeries), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public static readonly DependencyProperty TickWidthProperty =
            DependencyProperty.Register(nameof(TickWidth), typeof(double), typeof(OhlcSeries), new PropertyMetadata(8.0, OnPropertyChanged));

        public SKPaint? UpStroke { get => (SKPaint?)GetValue(UpStrokeProperty);
            set => SetValue(UpStrokeProperty, value);
        }

        public SKPaint? DownStroke { get => (SKPaint?)GetValue(DownStrokeProperty);
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

            var bullStroke = UpStroke ?? new SolidSKColorSKPaint(SKColor.Parse("#10B981"));
            var bearStroke = DownStroke ?? new SolidSKColorSKPaint(SKColor.Parse("#EF4444"));

            double strokeThickness = StrokeThickness > 0 ? StrokeThickness : 1.5;
            var bullSKPaint = new Pen(bullStroke, strokeThickness);
            var bearSKPaint = new Pen(bearStroke, strokeThickness);

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
                var candleSKPaint = isBullish ? bullSKPaint : bearSKPaint;

                // Screen coordinates
                var ptOpen = context.Transform.ToScreen(fp.X, open);
                var ptClose = context.Transform.ToScreen(fp.X, close);
                var ptHigh = context.Transform.ToScreen(fp.X, high);
                var ptLow = context.Transform.ToScreen(fp.X, low);

                // Draw vertical High-Low line
                context.Canvas.DrawLine((float)ptHigh.X, (float)ptHigh.Y, (float)ptLow.X, (float)ptLow.Y, candleSKPaint);

                // Draw left Open tick
                context.Canvas.DrawLine(candleSKPaint, ptOpen, new Point(ptOpen.X - tick, ptOpen.Y));

                // Draw right Close tick
                context.Canvas.DrawLine(candleSKPaint, ptClose, new Point(ptClose.X + tick, ptClose.Y));
            }
        }
    }
}