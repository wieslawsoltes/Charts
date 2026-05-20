using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

using ProCharts.Uno.Series;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public partial class Legend : StackPanel
    {
        public static readonly DependencyProperty ChartProperty =
            DependencyProperty.Register(nameof(Chart), typeof(ChartBase), typeof(Legend), new PropertyMetadata(default(ChartBase), OnPropertyChanged));

        public ChartBase? Chart
        {
            get => (ChartBase?)GetValue(ChartProperty);
            set => SetValue(ChartProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Legend legend)
            {
                if (e.Property == ChartProperty)
                {
                    legend.OnChartChanged(e.OldValue as ChartBase, e.NewValue as ChartBase);
                }
            }
        }

        public Legend()
        {
            Orientation = Orientation.Horizontal;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            Margin = new Thickness(8);

            ActualThemeChanged += (s, e) => RebuildItems(); // Rebuild when theme changes
        }

        private void OnChartChanged(ChartBase? oldChart, ChartBase? newChart)
        {
            RebuildItems();
        }

        public void RebuildItems()
        {
            Children.Clear();
            if (Chart == null) return;

            var seriesList = Chart.GetSeries().ToList();
            var activePalette = Chart.Palette ?? Palette.Default;

            bool isDark = ActualTheme == ElementTheme.Default 
                ? Application.Current.RequestedTheme == ApplicationTheme.Dark 
                : ActualTheme == ElementTheme.Dark;

            var textBrush = isDark 
                ? new SolidColorBrush(Microsoft.UI.Colors.White) 
                : new SolidColorBrush(Microsoft.UI.Colors.Black);

            for (int i = 0; i < seriesList.Count; i++)
            {
                var series = seriesList[i];
                SKPaint seriesSKPaint = series.Fill != null ? new SKPaint(series.Fill) : activePalette.GetSKPaint(i);
                
                // Convert SKPaint/SKColor to WinUI SolidColorBrush
                var indicatorBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(
                    seriesSKPaint.Color.Alpha,
                    seriesSKPaint.Color.Red,
                    seriesSKPaint.Color.Green,
                    seriesSKPaint.Color.Blue));

                var checkBox = new CheckBox
                {
                    IsChecked = series.IsVisible,
                    Margin = new Thickness(12, 4, 12, 4),
                    Foreground = textBrush
                };

                checkBox.Click += (s, e) =>
                {
                    series.IsVisible = checkBox.IsChecked ?? false;
                    Chart?.InvalidateVisual();
                };

                // CheckBox content with colored indicator and series title
                var contentPanel = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal, 
                    Spacing = 8,
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                var marker = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = indicatorBrush,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var textLabel = new TextBlock
                {
                    Text = string.IsNullOrEmpty(series.Title) ? $"Series {i + 1}" : series.Title,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = new FontFamily("Inter, Roboto, Segoe UI, sans-serif"),
                    FontSize = 12,
                    Foreground = textBrush
                };

                contentPanel.Children.Add(marker);
                contentPanel.Children.Add(textLabel);

                checkBox.Content = contentPanel;
                Children.Add(checkBox);
            }
        }
    }
}
