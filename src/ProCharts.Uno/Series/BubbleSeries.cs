using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Series
{
    public class BubbleSeries : CartesianSeries
    {
        public static readonly DependencyProperty SizePathProperty =
            DependencyProperty.Register(nameof(SizePath), typeof(string), typeof(BubbleSeries), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty MinBubbleSizeProperty =
            DependencyProperty.Register(nameof(MinBubbleSize), typeof(double), typeof(BubbleSeries), new PropertyMetadata(6.0, OnPropertyChanged));

        public static readonly DependencyProperty MaxBubbleSizeProperty =
            DependencyProperty.Register(nameof(MaxBubbleSize), typeof(double), typeof(BubbleSeries), new PropertyMetadata(36.0, OnPropertyChanged));

        public string? SizePath { get => (string?)GetValue(SizePathProperty);
            set => SetValue(SizePathProperty, value);
        }

        public double MinBubbleSize { get => (double)GetValue(MinBubbleSizeProperty);
            set => SetValue(MinBubbleSizeProperty, value);
        }

        public double MaxBubbleSize { get => (double)GetValue(MaxBubbleSizeProperty);
            set => SetValue(MaxBubbleSizeProperty, value);
        }

        public BubbleSeries()
        {
        }

        public override void RenderSeries(SeriesRenderContext context)
        {
            if (ItemsSource == null) return;

            var rawPoints = GetDataPoints();
            if (rawPoints.Count == 0) return;

            var bubbleFill = Fill ?? context.DefaultBrush;
            var bubbleStroke = Stroke ?? bubbleFill;
            var bubbleBrush = new Pen(bubbleStroke, StrokeThickness);

            double progress = context.AnimationProgress;

            // First pass: scan Z-values to find range
            double minZ = double.MaxValue;
            double maxZ = double.MinValue;
            var zValues = new double[rawPoints.Count];

            int idx = 0;
            foreach (var item in ItemsSource)
            {
                if (idx >= rawPoints.Count) break;

                double z = 10.0; // Default size
                if (item != null && !string.IsNullOrEmpty(SizePath))
                {
                    var zObj = ResolvePropertyValue(item, SizePath);
                    double temp = ConvertToDouble(zObj);
                    if (!double.IsNaN(temp)) z = temp;
                }
                zValues[idx] = z;

                if (z < minZ) minZ = z;
                if (z > maxZ) maxZ = z;
                idx++;
            }

            double zRange = maxZ - minZ;
            if (Math.Abs(zRange) < 1e-9) zRange = 1.0;

            // Render bubbles
            for (int i = 0; i < rawPoints.Count; i++)
            {
                var pt = rawPoints[i];
                if (double.IsNaN(pt.X) || double.IsNaN(pt.Y)) continue;

                var screenPt = context.Transform.ToScreen(pt.X, pt.Y);
                double z = zValues[i];

                // Scale size between MinBubbleSize and MaxBubbleSize
                double pct = (z - minZ) / zRange;
                double size = MinBubbleSize + pct * (MaxBubbleSize - MinBubbleSize);
                
                // Animate entry: size scale
                double animSize = size * progress;
                double halfSize = animSize / 2.0;

                // Alternate/combined animation: rise from baseline
                double screenBaselineY = context.Transform.ToScreen(pt.X, context.Transform.YMin).Y;
                double animatedY = screenBaselineY + (screenPt.Y - screenBaselineY) * progress;
                var animatedPt = new Point(screenPt.X, animatedY);

                context.Canvas.DrawEllipse(bubbleFill, bubbleBrush, animatedPt, halfSize, halfSize);
            }
        }
    }
}