using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Series
{
    public class BoxPlotSeries : CartesianSeries
    {
        public static readonly DependencyProperty BoxWidthPercentProperty =
            DependencyProperty.Register(nameof(BoxWidthPercent), typeof(double), typeof(BoxPlotSeries), new PropertyMetadata(0.5, OnPropertyChanged));

        public double BoxWidthPercent { get => (double)GetValue(BoxWidthPercentProperty);
            set => SetValue(BoxWidthPercentProperty, value);
        }

        public BoxPlotSeries()
        {
        }

        public struct BoxPlotMetrics
        {
            public double X { get; set; }
            public double Min { get; set; }
            public double Q1 { get; set; }
            public double Median { get; set; }
            public double Q3 { get; set; }
            public double Max { get; set; }
            public List<double> Outliers { get; set; }
            public bool IsValid => !double.IsNaN(Min);
        }

        public static BoxPlotMetrics Calculate(double x, List<double> values)
        {
            var metrics = new BoxPlotMetrics
            {
                X = x,
                Outliers = new List<double>()
            };

            if (values == null || values.Count == 0)
            {
                metrics.Min = double.NaN;
                return metrics;
            }

            var sorted = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToList();
            int count = sorted.Count;
            if (count == 0)
            {
                metrics.Min = double.NaN;
                return metrics;
            }

            metrics.Min = sorted[0];
            metrics.Max = sorted[count - 1];

            metrics.Median = GetPercentile(sorted, 0.5);
            metrics.Q1 = GetPercentile(sorted, 0.25);
            metrics.Q3 = GetPercentile(sorted, 0.75);

            double iqr = metrics.Q3 - metrics.Q1;
            double lowerBound = metrics.Q1 - 1.5 * iqr;
            double upperBound = metrics.Q3 + 1.5 * iqr;

            var regularValues = new List<double>();
            foreach (var v in sorted)
            {
                if (v < lowerBound || v > upperBound)
                {
                    metrics.Outliers.Add(v);
                }
                else
                {
                    regularValues.Add(v);
                }
            }

            if (regularValues.Count > 0)
            {
                metrics.Min = regularValues[0];
                metrics.Max = regularValues[regularValues.Count - 1];
            }

            return metrics;
        }

        private static double GetPercentile(List<double> sorted, double percentile)
        {
            int count = sorted.Count;
            if (count == 0) return 0.0;
            if (count == 1) return sorted[0];

            double idx = (count - 1) * percentile;
            int low = (int)Math.Floor(idx);
            int high = (int)Math.Ceiling(idx);

            if (low == high) return sorted[low];
            return sorted[low] + (idx - low) * (sorted[high] - sorted[low]);
        }

        public override IList<Point> GetDataPoints()
        {
            // Scans and returns data points (Min and Max boundaries) to help CartesianChart auto-scale the axes
            var list = new List<Point>();
            var metricsList = GetMetricsList();
            foreach (var m in metricsList)
            {
                if (m.IsValid)
                {
                    list.Add(new Point(m.X, m.Min));
                    list.Add(new Point(m.X, m.Max));
                    foreach (var outl in m.Outliers)
                    {
                        list.Add(new Point(m.X, outl));
                    }
                }
            }
            return list;
        }

        private List<BoxPlotMetrics> GetMetricsList()
        {
            var list = new List<BoxPlotMetrics>();
            if (ItemsSource == null) return list;

            int index = 0;
            foreach (var item in ItemsSource)
            {
                if (item == null)
                {
                    index++;
                    continue;
                }

                // If item is already BoxPlotMetrics
                if (item is BoxPlotMetrics bpm)
                {
                    list.Add(bpm);
                    index++;
                    continue;
                }

                object? catObj = ResolvePropertyValue(item, CategoryPath);
                double xVal = CategoryPath != null ? ConvertToDouble(catObj) : index;
                if (double.IsNaN(xVal)) xVal = index;

                object? valObj = ResolvePropertyValue(item, ValuePath);
                var numValues = new List<double>();
                if (valObj is IEnumerable enumerable)
                {
                    foreach (var val in enumerable)
                    {
                        numValues.Add(ConvertToDouble(val));
                    }
                }
                else if (valObj != null)
                {
                    numValues.Add(ConvertToDouble(valObj));
                }

                if (numValues.Count > 0)
                {
                    list.Add(Calculate(xVal, numValues));
                }
                index++;
            }

            return list;
        }

        public override void RenderSeries(SeriesRenderContext context)
        {
            var metricsList = GetMetricsList();
            if (metricsList.Count == 0) return;

            var boxFill = Fill ?? new SolidColorBrush(Color.Parse("#4006B6D4")); // Transparent cyan
            var boxStroke = Stroke ?? context.DefaultBrush;
            double strokeThickness = StrokeThickness > 0 ? StrokeThickness : 1.5;
            var boxBrush = new Pen(boxStroke, strokeThickness);

            double slotWidth = context.PlotArea.Width / Math.Max(1, metricsList.Count);
            double boxWidth = Math.Clamp(slotWidth * BoxWidthPercent, 4.0, 60.0);

            double progress = context.AnimationProgress;

            foreach (var m in metricsList)
            {
                if (!m.IsValid) continue;

                // Sprout animation from Median
                double median = m.Median;
                double q1 = median + (m.Q1 - median) * progress;
                double q3 = median + (m.Q3 - median) * progress;
                double min = median + (m.Min - median) * progress;
                double max = median + (m.Max - median) * progress;

                // Screen coords
                var ptMin = context.Transform.ToScreen(m.X, min);
                var ptQ1 = context.Transform.ToScreen(m.X, q1);
                var ptMedian = context.Transform.ToScreen(m.X, median);
                var ptQ3 = context.Transform.ToScreen(m.X, q3);
                var ptMax = context.Transform.ToScreen(m.X, max);

                // Draw Whiskers (vertical lines)
                context.Canvas.DrawLine((float)ptMin.X, (float)ptMin.Y, (float)ptQ1.X, (float)ptQ1.Y, boxBrush);
                context.Canvas.DrawLine((float)ptQ3.X, (float)ptQ3.Y, (float)ptMax.X, (float)ptMax.Y, boxBrush);

                // Whisker horizontal end-ticks
                double tickW = boxWidth * 0.4;
                context.Canvas.DrawLine(boxBrush, new Point(ptMin.X - tickW, ptMin.Y), new Point(ptMin.X + tickW, ptMin.Y));
                context.Canvas.DrawLine(boxBrush, new Point(ptMax.X - tickW, ptMax.Y), new Point(ptMax.X + tickW, ptMax.Y));

                // Draw Box (Q1 to Q3)
                double topY = Math.Min(ptQ1.Y, ptQ3.Y);
                double bottomY = Math.Max(ptQ1.Y, ptQ3.Y);
                double h = Math.Max(1.0, bottomY - topY);

                var boxRect = new Rect(ptMedian.X - boxWidth / 2.0, topY, boxWidth, h);
                context.Canvas.DrawRectangle(boxFill, boxBrush, boxRect);

                // Draw Median Line (horizontal line inside the box)
                context.Canvas.DrawLine(boxBrush, new Point(ptMedian.X - boxWidth / 2.0, ptMedian.Y), new Point(ptMedian.X + boxWidth / 2.0, ptMedian.Y));

                // Draw Outliers
                foreach (var outl in m.Outliers)
                {
                    double outlAnim = median + (outl - median) * progress;
                    var ptOutl = context.Transform.ToScreen(m.X, outlAnim);
                    var outlierBrush = new SolidColorBrush(Color.Parse("#EF4444")); // Red outlier dots
                    context.Canvas.DrawEllipse(outlierBrush, null, ptOutl, 3.0, 3.0);
                }
            }
        }
    }
}