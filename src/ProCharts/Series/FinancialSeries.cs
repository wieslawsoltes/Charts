using System;
using System.Collections.Generic;
using Avalonia;
using ProCharts.Maths;

namespace ProCharts.Series
{
    public abstract class FinancialSeries : CartesianSeries
    {
        public static readonly StyledProperty<string?> HighPathProperty =
            AvaloniaProperty.Register<FinancialSeries, string?>(nameof(HighPath));

        public static readonly StyledProperty<string?> LowPathProperty =
            AvaloniaProperty.Register<FinancialSeries, string?>(nameof(LowPath));

        public static readonly StyledProperty<string?> OpenPathProperty =
            AvaloniaProperty.Register<FinancialSeries, string?>(nameof(OpenPath));

        public static readonly StyledProperty<string?> ClosePathProperty =
            AvaloniaProperty.Register<FinancialSeries, string?>(nameof(ClosePath));

        public string? HighPath
        {
            get => GetValue(HighPathProperty);
            set => SetValue(HighPathProperty, value);
        }

        public string? LowPath
        {
            get => GetValue(LowPathProperty);
            set => SetValue(LowPathProperty, value);
        }

        public string? OpenPath
        {
            get => GetValue(OpenPathProperty);
            set => SetValue(OpenPathProperty, value);
        }

        public string? ClosePath
        {
            get => GetValue(ClosePathProperty);
            set => SetValue(ClosePathProperty, value);
        }

        protected FinancialSeries()
        {
            HighPathProperty.Changed.AddClassHandler<FinancialSeries>((x, e) => x.RaiseSeriesChanged());
            LowPathProperty.Changed.AddClassHandler<FinancialSeries>((x, e) => x.RaiseSeriesChanged());
            OpenPathProperty.Changed.AddClassHandler<FinancialSeries>((x, e) => x.RaiseSeriesChanged());
            ClosePathProperty.Changed.AddClassHandler<FinancialSeries>((x, e) => x.RaiseSeriesChanged());
        }

        public struct FinancialPoint
        {
            public double X { get; set; }
            public double Open { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
            public double Close { get; set; }
            public bool IsValid => !double.IsNaN(Open) && !double.IsNaN(High) && !double.IsNaN(Low) && !double.IsNaN(Close);
        }

        public virtual IList<FinancialPoint> GetFinancialPoints()
        {
            var list = new List<FinancialPoint>();
            if (ItemsSource == null) return list;

            int index = 0;
            foreach (var item in ItemsSource)
            {
                if (item == null)
                {
                    list.Add(new FinancialPoint { X = index, Open = double.NaN, High = double.NaN, Low = double.NaN, Close = double.NaN });
                    index++;
                    continue;
                }

                if (item is FinancialPoint fp)
                {
                    list.Add(fp);
                    index++;
                    continue;
                }

                object? xObj = ResolvePropertyValue(item, CategoryPath);
                object? oObj = ResolvePropertyValue(item, OpenPath);
                object? hObj = ResolvePropertyValue(item, HighPath);
                object? lObj = ResolvePropertyValue(item, LowPath);
                object? cObj = ResolvePropertyValue(item, ClosePath);

                double xVal = CategoryPath != null ? ConvertToDouble(xObj) : index;
                if (double.IsNaN(xVal)) xVal = index;

                double openVal = ConvertToDouble(oObj);
                double highVal = ConvertToDouble(hObj);
                double lowVal = ConvertToDouble(lObj);
                double closeVal = ConvertToDouble(cObj);

                list.Add(new FinancialPoint
                {
                    X = xVal,
                    Open = openVal,
                    High = highVal,
                    Low = lowVal,
                    Close = closeVal
                });
                index++;
            }

            return list;
        }

        public override IList<Point> GetDataPoints()
        {
            var list = new List<Point>();
            foreach (var fp in GetFinancialPoints())
            {
                if (fp.IsValid)
                {
                    list.Add(new Point(fp.X, fp.High));
                    list.Add(new Point(fp.X, fp.Low));
                }
            }
            return list;
        }
    }
}
