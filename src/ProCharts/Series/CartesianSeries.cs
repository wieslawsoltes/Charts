using System;
using System.Collections.Generic;
using Avalonia;
using ProCharts.Maths;

namespace ProCharts.Series
{
    public abstract class CartesianSeries : ChartSeries
    {
        public static readonly StyledProperty<string?> CategoryPathProperty =
            AvaloniaProperty.Register<CartesianSeries, string?>(nameof(CategoryPath));

        public string? CategoryPath
        {
            get => GetValue(CategoryPathProperty);
            set => SetValue(CategoryPathProperty, value);
        }

        protected CartesianSeries()
        {
            CategoryPathProperty.Changed.AddClassHandler<CartesianSeries>((x, e) => x.RaiseSeriesChanged());
        }

        public abstract void RenderSeries(in SeriesRenderContext context);

        /// <summary>
        /// Scans the ItemsSource and returns a clean, mapped list of Points.
        /// </summary>
        public virtual IList<Point> GetDataPoints()
        {
            var list = new List<Point>();
            if (ItemsSource == null) return list;

            int index = 0;
            foreach (var item in ItemsSource)
            {
                if (item == null)
                {
                    list.Add(new Point(index, double.NaN));
                    index++;
                    continue;
                }

                // Case 1: Point directly
                if (item is Point pt)
                {
                    list.Add(pt);
                    index++;
                    continue;
                }

                // Case 2: Direct numeric types
                if (IsNumericType(item.GetType()))
                {
                    double yVal = ConvertToDouble(item);
                    list.Add(new Point(index, yVal));
                    index++;
                    continue;
                }

                // Case 3: Custom object data bindings
                object? xObj = ResolvePropertyValue(item, CategoryPath);
                object? yObj = ResolvePropertyValue(item, ValuePath);

                double xVal = CategoryPath != null ? ConvertToDouble(xObj) : index;
                if (double.IsNaN(xVal)) xVal = index; // Fallback to index if category is not numeric (e.g. strings)

                double yValResolved = ConvertToDouble(yObj);

                list.Add(new Point(xVal, yValResolved));
                index++;
            }

            return list;
        }

        private static bool IsNumericType(Type type)
        {
            return type == typeof(double) ||
                   type == typeof(float) ||
                   type == typeof(int) ||
                   type == typeof(long) ||
                   type == typeof(decimal) ||
                   type == typeof(short) ||
                   type == typeof(byte);
        }
    }
}
