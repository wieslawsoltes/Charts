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
    public class HiloSeries : FinancialSeries
    {
        public static readonly DependencyProperty UpStrokeProperty =
            DependencyProperty.Register(nameof(UpStroke), typeof(SKPaint), typeof(HiloSeries), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public static readonly DependencyProperty DownStrokeProperty =
            DependencyProperty.Register(nameof(DownStroke), typeof(SKPaint), typeof(HiloSeries), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public SKPaint? UpStroke { get => (SKPaint?)GetValue(UpStrokeProperty);
            set => SetValue(UpStrokeProperty, value);
        }

        public SKPaint? DownStroke { get => (SKPaint?)GetValue(DownStrokeProperty);
            set => SetValue(DownStrokeProperty, value);
        }

        public HiloSeries()
        {
        }

        public override void RenderSeries(SeriesRenderContext context)
        {
            var points = GetFinancialPoints();
            if (points.Count == 0) return;

            var defaultSKPaint = Stroke ?? context.DefaultSKPaint;
            var bullStroke = UpStroke ?? defaultSKPaint;
            var bearStroke = DownStroke ?? defaultSKPaint;

            double strokeThickness = StrokeThickness > 0 ? StrokeThickness : 2.0;
            var bullSKPaint = new Pen(bullStroke, strokeThickness);
            var bearSKPaint = new Pen(bearStroke, strokeThickness);

            double progress = context.AnimationProgress;

            foreach (var fp in points)
            {
                if (double.IsNaN(fp.High) || double.IsNaN(fp.Low)) continue;

                // Sprout animation from High/Low mid-point
                double mid = (fp.High + fp.Low) / 2.0;
                double high = mid + (fp.High - mid) * progress;
                double low = mid + (fp.Low - mid) * progress;

                bool isBullish = fp.Close >= fp.Open;
                var candleSKPaint = isBullish ? bullSKPaint : bearSKPaint;

                // Screen coordinates
                var ptHigh = context.Transform.ToScreen(fp.X, high);
                var ptLow = context.Transform.ToScreen(fp.X, low);

                // Draw vertical High-Low line
                context.Canvas.DrawLine((float)ptHigh.X, (float)ptHigh.Y, (float)ptLow.X, (float)ptLow.Y, candleSKPaint);
            }
        }
    }
}