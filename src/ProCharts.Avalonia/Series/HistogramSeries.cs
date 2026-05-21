using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Avalonia.Maths;

namespace ProCharts.Avalonia.Series
{
    public class HistogramSeries : CartesianSeries
    {
        public static readonly StyledProperty<int> BinCountProperty =
            AvaloniaProperty.Register<HistogramSeries, int>(nameof(BinCount), 10);

        public int BinCount
        {
            get => GetValue(BinCountProperty);
            set => SetValue(BinCountProperty, value);
        }

        public HistogramSeries()
        {
            BinCountProperty.Changed.AddClassHandler<HistogramSeries>((x, e) => x.RaiseSeriesChanged());
        }

        public struct HistogramBin
        {
            public double LowerBound { get; set; }
            public double UpperBound { get; set; }
            public int Frequency { get; set; }
            public double Center => (LowerBound + UpperBound) / 2.0;
        }

        public List<HistogramBin> GetBins()
        {
            var bins = new List<HistogramBin>();
            if (ItemsSource == null) return bins;

            // Extract all continuous values
            var values = new List<double>();
            foreach (var item in ItemsSource)
            {
                if (item == null) continue;
                double val;
                if (string.IsNullOrEmpty(ValuePath))
                {
                    val = ConvertToDouble(item);
                }
                else
                {
                    val = ConvertToDouble(ResolvePropertyValue(item, ValuePath));
                }

                if (!double.IsNaN(val))
                {
                    values.Add(val);
                }
            }

            if (values.Count == 0) return bins;

            double min = values.Min();
            double max = values.Max();
            int binCount = Math.Max(2, BinCount);

            if (Math.Abs(max - min) < 1e-9)
            {
                max = min + 1.0;
            }

            double binWidth = (max - min) / binCount;

            // Initialize bins
            for (int i = 0; i < binCount; i++)
            {
                bins.Add(new HistogramBin
                {
                    LowerBound = min + i * binWidth,
                    UpperBound = min + (i + 1) * binWidth,
                    Frequency = 0
                });
            }

            // Distribute values
            foreach (var val in values)
            {
                int binIndex = (int)((val - min) / binWidth);
                if (binIndex >= binCount) binIndex = binCount - 1;
                if (binIndex < 0) binIndex = 0;

                var b = bins[binIndex];
                b.Frequency++;
                bins[binIndex] = b;
            }

            return bins;
        }

        public override IList<Point> GetDataPoints()
        {
            // Help scale axes: X runs from Min Bin Center to Max Bin Center, Y runs from 0 to Max Frequency
            var points = new List<Point>();
            var bins = GetBins();
            foreach (var b in bins)
            {
                points.Add(new Point(b.Center, b.Frequency));
                points.Add(new Point(b.Center, 0.0));
            }
            return points;
        }

        public override void RenderSeries(in SeriesRenderContext context)
        {
            var bins = GetBins();
            if (bins.Count == 0) return;

            var columnFill = Fill ?? new SolidColorBrush(Color.Parse("#40A855F7")); // Slate purple transparent
            var columnStroke = Stroke ?? context.DefaultBrush;
            double strokeThickness = StrokeThickness > 0 ? StrokeThickness : 1.0;
            var columnPen = new Pen(columnStroke, strokeThickness);

            double progress = context.AnimationProgress;

            // Draw each bin as a nice vertical bar
            foreach (var b in bins)
            {
                // Animate frequency column height
                double animatedFreq = b.Frequency * progress;

                // Map bounds to screen
                var ptBottomLeft = context.Transform.ToScreen(b.LowerBound, 0.0);
                var ptTopRight = context.Transform.ToScreen(b.UpperBound, animatedFreq);

                double x = ptBottomLeft.X;
                double y = ptTopRight.Y;
                double w = ptTopRight.X - ptBottomLeft.X;
                double h = ptBottomLeft.Y - ptTopRight.Y;

                // Gap spacing between columns
                double gap = 1.0;
                if (w > gap * 2.0 && h > 0)
                {
                    var rect = new Rect(x + gap, y, w - gap * 2.0, h);
                    context.DrawingContext.DrawRectangle(columnFill, columnPen, rect);
                }
            }
        }
    }
}
