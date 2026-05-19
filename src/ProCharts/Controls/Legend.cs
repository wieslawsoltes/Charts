using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ProCharts.Series;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class Legend : WrapPanel
    {
        public static readonly StyledProperty<ChartBase?> ChartProperty =
            AvaloniaProperty.Register<Legend, ChartBase?>(nameof(Chart));

        public ChartBase? Chart
        {
            get => GetValue(ChartProperty);
            set => SetValue(ChartProperty, value);
        }

        public Legend()
        {
            Orientation = Orientation.Horizontal;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            Margin = new Thickness(8);

            ChartProperty.Changed.AddClassHandler<Legend>((x, e) => x.OnChartChanged(e.OldValue as ChartBase, e.NewValue as ChartBase));
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

            for (int i = 0; i < seriesList.Count; i++)
            {
                var series = seriesList[i];
                var seriesBrush = series.Fill ?? activePalette.GetBrush(i);

                var checkBox = new CheckBox
                {
                    IsChecked = series.IsVisible,
                    Margin = new Thickness(12, 4, 12, 4),
                    Foreground = Chart.LabelForeground ?? (ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Brushes.White : Brushes.Black),
                };

                checkBox.IsCheckedChanged += (s, e) =>
                {
                    series.IsVisible = checkBox.IsChecked ?? false;
                };

                // CheckBox content with colored indicator and series title
                var contentPanel = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal, 
                    Spacing = 8,
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                var marker = new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = seriesBrush,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var textLabel = new TextBlock
                {
                    Text = string.IsNullOrEmpty(series.Title) ? $"Series {i + 1}" : series.Title,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = new FontFamily("Inter, Roboto, Segoe UI, sans-serif"),
                    FontSize = 12
                };

                contentPanel.Children.Add(marker);
                contentPanel.Children.Add(textLabel);

                checkBox.Content = contentPanel;
                Children.Add(checkBox);
            }
        }
    }
}
