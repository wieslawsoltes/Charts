using System;
using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Series
{
    public class HiloSeries : FinancialSeries
    {
        public static readonly DependencyProperty UpStrokeProperty =
            DependencyProperty.Register(nameof(UpStroke), typeof(Brush), typeof(HiloSeries), new PropertyMetadata(default(Brush?), OnPropertyChanged));

        public static readonly DependencyProperty DownStrokeProperty =
            DependencyProperty.Register(nameof(DownStroke), typeof(Brush), typeof(HiloSeries), new PropertyMetadata(default(Brush?), OnPropertyChanged));

        public Brush? UpStroke { get => (Brush?)GetValue(UpStrokeProperty);
            set => SetValue(UpStrokeProperty, value);
        }

        public Brush? DownStroke { get => (Brush?)GetValue(DownStrokeProperty);
            set => SetValue(DownStrokeProperty, value);
        }

        public HiloSeries()
        {
        }

        public override void RenderSeries(SeriesRenderContext context)
        {
            var points = GetFinancialPoints();
            if (points.Count == 0) return;

            var defaultBrush = Stroke ?? context.DefaultBrush;
            var bullStroke = UpStroke ?? defaultBrush;
            var bearStroke = DownStroke ?? defaultBrush;

            double strokeThickness = StrokeThickness > 0 ? StrokeThickness : 2.0;
            var bullBrush = new Pen(bullStroke, strokeThickness);
            var bearBrush = new Pen(bearStroke, strokeThickness);

            double progress = context.AnimationProgress;

            foreach (var fp in points)
            {
                if (double.IsNaN(fp.High) || double.IsNaN(fp.Low)) continue;

                // Sprout animation from High/Low mid-point
                double mid = (fp.High + fp.Low) / 2.0;
                double high = mid + (fp.High - mid) * progress;
                double low = mid + (fp.Low - mid) * progress;

                bool isBullish = fp.Close >= fp.Open;
                var candleBrush = isBullish ? bullBrush : bearBrush;

                // Screen coordinates
                var ptHigh = context.Transform.ToScreen(fp.X, high);
                var ptLow = context.Transform.ToScreen(fp.X, low);

                // Draw vertical High-Low line
                context.Canvas.DrawLine((float)ptHigh.X, (float)ptHigh.Y, (float)ptLow.X, (float)ptLow.Y, candleBrush);
            }
        }
    }
}