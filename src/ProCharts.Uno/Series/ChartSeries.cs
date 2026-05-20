using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.UI.Xaml;
using SkiaSharp;

namespace ProCharts.Uno.Series
{
    public abstract partial class ChartSeries : DependencyObject
    {
        // --- DEPENDENCY PROPERTIES ---

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ChartSeries), new PropertyMetadata(default(string), OnPropertyChanged));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ChartSeries), new PropertyMetadata(default(IEnumerable), OnPropertyChanged));

        public static readonly DependencyProperty ValuePathProperty =
            DependencyProperty.Register(nameof(ValuePath), typeof(string), typeof(ChartSeries), new PropertyMetadata(default(string), OnPropertyChanged));

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register(nameof(Fill), typeof(SKPaint), typeof(ChartSeries), new PropertyMetadata(default(SKPaint), OnPropertyChanged));

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(nameof(Stroke), typeof(SKPaint), typeof(ChartSeries), new PropertyMetadata(default(SKPaint), OnPropertyChanged));

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(ChartSeries), new PropertyMetadata(1.0, OnPropertyChanged));

        public static readonly DependencyProperty PointSKPaintPathProperty =
            DependencyProperty.Register(nameof(PointSKPaintPath), typeof(string), typeof(ChartSeries), new PropertyMetadata(default(string), OnPropertyChanged));

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register(nameof(IsVisible), typeof(bool), typeof(ChartSeries), new PropertyMetadata(true, OnPropertyChanged));

        // --- PROPERTIES ---

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string? ValuePath
        {
            get => (string?)GetValue(ValuePathProperty);
            set => SetValue(ValuePathProperty, value);
        }

        public SKPaint? Fill
        {
            get => (SKPaint?)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public SKPaint? Stroke
        {
            get => (SKPaint?)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public string? PointSKPaintPath
        {
            get => (string?)GetValue(PointSKPaintPathProperty);
            set => SetValue(PointSKPaintPathProperty, value);
        }

        public bool IsVisible
        {
            get => (bool)GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        // --- EVENTS ---
        
        public event EventHandler? SeriesChanged;

        protected static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChartSeries series)
            {
                series.RaiseSeriesChanged();
            }
        }

        protected ChartSeries()
        {
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
