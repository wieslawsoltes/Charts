using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Avalonia;
using Avalonia.Media;

namespace ProCharts.Series
{
    public abstract class ChartSeries : AvaloniaObject
    {
        public static readonly StyledProperty<string?> TitleProperty =
            AvaloniaProperty.Register<ChartSeries, string?>(nameof(Title));

        public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
            AvaloniaProperty.Register<ChartSeries, IEnumerable?>(nameof(ItemsSource));

        public static readonly StyledProperty<string?> ValuePathProperty =
            AvaloniaProperty.Register<ChartSeries, string?>(nameof(ValuePath));

        public static readonly StyledProperty<IBrush?> FillProperty =
            AvaloniaProperty.Register<ChartSeries, IBrush?>(nameof(Fill));

        public static readonly StyledProperty<IBrush?> StrokeProperty =
            AvaloniaProperty.Register<ChartSeries, IBrush?>(nameof(Stroke));

        public static readonly StyledProperty<double> StrokeThicknessProperty =
            AvaloniaProperty.Register<ChartSeries, double>(nameof(StrokeThickness), 1.0);

        public static readonly StyledProperty<string?> PointBrushPathProperty =
            AvaloniaProperty.Register<ChartSeries, string?>(nameof(PointBrushPath));

        public static readonly StyledProperty<bool> IsVisibleProperty =
            AvaloniaProperty.Register<ChartSeries, bool>(nameof(IsVisible), true);

        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public IEnumerable? ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string? ValuePath
        {
            get => GetValue(ValuePathProperty);
            set => SetValue(ValuePathProperty, value);
        }

        public IBrush? Fill
        {
            get => GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public IBrush? Stroke
        {
            get => GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public double StrokeThickness
        {
            get => GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public string? PointBrushPath
        {
            get => GetValue(PointBrushPathProperty);
            set => SetValue(PointBrushPathProperty, value);
        }

        public bool IsVisible
        {
            get => GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        // --- EVENTS ---
        
        public event EventHandler? SeriesChanged;

        protected ChartSeries()
        {
            // Trigger redrawing on property modifications
            TitleProperty.Changed.AddClassHandler<ChartSeries>((x, e) => x.RaiseSeriesChanged());
            ItemsSourceProperty.Changed.AddClassHandler<ChartSeries>((x, e) => x.RaiseSeriesChanged());
            ValuePathProperty.Changed.AddClassHandler<ChartSeries>((x, e) => x.RaiseSeriesChanged());
            FillProperty.Changed.AddClassHandler<ChartSeries>((x, e) => x.RaiseSeriesChanged());
            StrokeProperty.Changed.AddClassHandler<ChartSeries>((x, e) => x.RaiseSeriesChanged());
            StrokeThicknessProperty.Changed.AddClassHandler<ChartSeries>((x, e) => x.RaiseSeriesChanged());
            PointBrushPathProperty.Changed.AddClassHandler<ChartSeries>((x, e) => x.RaiseSeriesChanged());
            IsVisibleProperty.Changed.AddClassHandler<ChartSeries>((x, e) => x.RaiseSeriesChanged());
        }

        protected void RaiseSeriesChanged()
        {
            SeriesChanged?.Invoke(this, EventArgs.Empty);
        }

        // --- PROPERTY EVALUATION UTILITIES ---

        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> PropertyCache = new();

        protected static double ConvertToDouble(object? value)
        {
            if (value == null) return double.NaN;
            
            try
            {
                return Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return double.NaN;
            }
        }

        protected object? ResolvePropertyValue(object? item, string? path)
        {
            if (item == null) return null;
            if (string.IsNullOrEmpty(path)) return item;

            var type = item.GetType();
            var cacheKey = (type, path);

            var prop = PropertyCache.GetOrAdd(cacheKey, key =>
            {
                return key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            });

            return prop?.GetValue(item);
        }
    }
}
