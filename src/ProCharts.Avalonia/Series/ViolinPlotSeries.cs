using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Avalonia.Maths;

namespace ProCharts.Avalonia.Series
{
    public class ViolinPlotSeries : CartesianSeries
    {
        public static readonly StyledProperty<double> ViolinWidthPercentProperty =
            AvaloniaProperty.Register<ViolinPlotSeries, double>(nameof(ViolinWidthPercent), 0.7);

        public static readonly StyledProperty<int> DensityPointsProperty =
            AvaloniaProperty.Register<ViolinPlotSeries, int>(nameof(DensityPoints), 40);

        public double ViolinWidthPercent
        {
            get => GetValue(ViolinWidthPercentProperty);
            set => SetValue(ViolinWidthPercentProperty, value);
        }

        public int DensityPoints
        {
            get => GetValue(DensityPointsProperty);
            set => SetValue(DensityPointsProperty, value);
        }

        public ViolinPlotSeries()
        {
            ViolinWidthPercentProperty.Changed.AddClassHandler<ViolinPlotSeries>((x, e) => x.RaiseSeriesChanged());
            DensityPointsProperty.Changed.AddClassHandler<ViolinPlotSeries>((x, e) => x.RaiseSeriesChanged());
        }

        public class ViolinMetrics
        {
            public double X { get; set; }
            public double Min { get; set; }
            public double Max { get; set; }
            public double Median { get; set; }
            public List<double> SamplePoints { get; } = new List<double>();
            public List<double> Densities { get; } = new List<double>();
            public bool IsValid => Densities.Count > 0 && !double.IsNaN(Min);
        }

        public static ViolinMetrics Calculate(double x, List<double> values, int numPoints)
        {
            var metrics = new ViolinMetrics { X = x };

            if (values == null || values.Count == 0)
            {
                metrics.Min = double.NaN;
                return metrics;
            }

            var cleanValues = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToList();
            int n = cleanValues.Count;
            if (n == 0)
            {
                metrics.Min = double.NaN;
                return metrics;
            }

            metrics.Min = cleanValues[0];
            metrics.Max = cleanValues[n - 1];
            metrics.Median = GetMedian(cleanValues);

            // Compute Bandwidth using Silverman's Rule of Thumb: h = 1.06 * stdDev * n^(-1/5)
            double stdDev = GetStandardDeviation(cleanValues);
            double h = 1.06 * stdDev * Math.Pow(n, -0.2);
            if (h <= 0.0) h = 1.0;

            // Generate sample points evenly spaced from Min to Max
            double minRange = metrics.Min - h * 1.5; // pad slightly
            double maxRange = metrics.Max + h * 1.5;
            double step = (maxRange - minRange) / Math.Max(1, numPoints - 1);

            for (int i = 0; i < numPoints; i++)
            {
                double sampleY = minRange + i * step;
                double density = EstimateKernelDensity(sampleY, cleanValues, h);
                metrics.SamplePoints.Add(sampleY);
                metrics.Densities.Add(density);
            }

            return metrics;
        }

        private static double GetMedian(List<double> sorted)
        {
            int count = sorted.Count;
            if (count == 0) return 0.0;
            int mid = count / 2;
            if (count % 2 != 0) return sorted[mid];
            return (sorted[mid - 1] + sorted[mid]) / 2.0;
        }

        private static double GetStandardDeviation(List<double> values)
        {
            int count = values.Count;
            if (count <= 1) return 0.0;
            double avg = values.Average();
            double sum = values.Sum(d => (d - avg) * (d - avg));
            return Math.Sqrt(sum / (count - 1));
        }

        private static double EstimateKernelDensity(double y, List<double> values, double h)
        {
            double sum = 0.0;
            int n = values.Count;

            foreach (var val in values)
            {
                double u = (y - val) / h;
                sum += Math.Exp(-0.5 * u * u) / Math.Sqrt(2.0 * Math.PI);
            }

            return sum / (n * h);
        }

        public override IList<Point> GetDataPoints()
        {
            var list = new List<Point>();
            var violinList = GetViolinList();
            foreach (var v in violinList)
            {
                if (v.IsValid)
                {
                    // Span auto scaling boundaries
                    list.Add(new Point(v.X, v.Min));
                    list.Add(new Point(v.X, v.Max));
                }
            }
            return list;
        }

        private List<ViolinMetrics> GetViolinList()
        {
            var list = new List<ViolinMetrics>();
            if (ItemsSource == null) return list;

            int index = 0;
            int numPoints = Math.Clamp(DensityPoints, 10, 100);

            foreach (var item in ItemsSource)
            {
                if (item == null)
                {
                    index++;
                    continue;
                }

                if (item is ViolinMetrics vm)
                {
                    list.Add(vm);
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
                    list.Add(Calculate(xVal, numValues, numPoints));
                }
                index++;
            }

            return list;
        }

        public override void RenderSeries(in SeriesRenderContext context)
        {
            var violinList = GetViolinList();
            if (violinList.Count == 0) return;

            var fillBrush = Fill ?? new SolidColorBrush(Color.Parse("#508B5CF6")); // Transparent Lavender
            var strokeColor = Stroke ?? new SolidColorBrush(Color.Parse("#8B5CF6"));
            var strokePen = new Pen(strokeColor, StrokeThickness > 0 ? StrokeThickness : 1.5);

            double slotWidth = context.PlotArea.Width / Math.Max(1, violinList.Count);
            double maxViolinWidth = Math.Clamp(slotWidth * ViolinWidthPercent, 10.0, 120.0);

            double progress = context.AnimationProgress;

            foreach (var vm in violinList)
            {
                if (!vm.IsValid) continue;

                double maxDensity = vm.Densities.Max();
                if (maxDensity <= 0.0) continue;

                // Build a closed mirrored geometry path
                var geom = new StreamGeometry();
                using (var ctx = geom.Open())
                {
                    // Right-side coordinates
                    var rightPoints = new List<Point>();
                    // Left-side coordinates (bottom to top)
                    var leftPoints = new List<Point>();

                    for (int i = 0; i < vm.SamplePoints.Count; i++)
                    {
                        double yVal = vm.SamplePoints[i];
                        double density = vm.Densities[i];

                        // Normalize density
                        double horizontalOffset = (maxViolinWidth / 2.0) * (density / maxDensity) * progress;

                        var centerPt = context.Transform.ToScreen(vm.X, yVal);
                        
                        rightPoints.Add(new Point(centerPt.X + horizontalOffset, centerPt.Y));
                        leftPoints.Insert(0, new Point(centerPt.X - horizontalOffset, centerPt.Y));
                    }

                    if (rightPoints.Count > 0)
                    {
                        ctx.BeginFigure(rightPoints[0], true);
                        for (int i = 1; i < rightPoints.Count; i++)
                        {
                            ctx.LineTo(rightPoints[i]);
                        }
                        foreach (var lp in leftPoints)
                        {
                            ctx.LineTo(lp);
                        }
                    }
                }

                // Render violin density body
                context.DrawingContext.DrawGeometry(fillBrush, strokePen, geom);

                // Draw central axis line (Median / Range)
                var ptMin = context.Transform.ToScreen(vm.X, vm.Min);
                var ptMax = context.Transform.ToScreen(vm.X, vm.Max);
                var ptMedian = context.Transform.ToScreen(vm.X, vm.Median);

                var axisPen = new Pen(new SolidColorBrush(Color.Parse("#FFFFFF"), 0.6), 1.5);
                context.DrawingContext.DrawLine(axisPen, ptMin, ptMax);

                // Median point marker
                var medianBrush = Brushes.White;
                context.DrawingContext.DrawEllipse(medianBrush, null, ptMedian, 3.5, 3.5);
            }
        }
    }
}
