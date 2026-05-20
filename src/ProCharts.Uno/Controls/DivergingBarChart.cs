using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public class DivergingBarChart : ChartBase
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(DivergingBarChart), new PropertyMetadata(default(IEnumerable?), OnPropertyChanged));

        public static readonly DependencyProperty CategoryPathProperty =
            DependencyProperty.Register(nameof(CategoryPath), typeof(string), typeof(DivergingBarChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty LeftValuePathProperty =
            DependencyProperty.Register(nameof(LeftValuePath), typeof(string), typeof(DivergingBarChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty RightValuePathProperty =
            DependencyProperty.Register(nameof(RightValuePath), typeof(string), typeof(DivergingBarChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty LeftSKPaintProperty =
            DependencyProperty.Register(nameof(LeftSKPaint), typeof(SKPaint), typeof(DivergingBarChart), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public static readonly DependencyProperty RightSKPaintProperty =
            DependencyProperty.Register(nameof(RightSKPaint), typeof(SKPaint), typeof(DivergingBarChart), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public IEnumerable? ItemsSource
        {
            get => (System.Collections.IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string? CategoryPath { get => (string?)GetValue(CategoryPathProperty);
            set => SetValue(CategoryPathProperty, value);
        }

        public string? LeftValuePath { get => (string?)GetValue(LeftValuePathProperty);
            set => SetValue(LeftValuePathProperty, value);
        }

        public string? RightValuePath { get => (string?)GetValue(RightValuePathProperty);
            set => SetValue(RightValuePathProperty, value);
        }

        public SKPaint? LeftSKPaint { get => (SKPaint?)GetValue(LeftSKPaintProperty);
            set => SetValue(LeftSKPaintProperty, value);
        }

        public SKPaint? RightSKPaint { get => (SKPaint?)GetValue(RightSKPaintProperty);
            set => SetValue(RightSKPaintProperty, value);
        }

        public DivergingBarChart()
        {
        }

        private class DivergingItem
        {
            public string Category { get; set; } = string.Empty;
            public double LeftValue { get; set; }
            public double RightValue { get; set; }
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double top = 24;
            double bottom = 24;
            double left = 80; // Larger left margin for category labels
            double right = 24;

            if (!string.IsNullOrEmpty(Title)) top += 28;

            double w = Math.Max(10, bounds.Width - left - right);
            double h = Math.Max(10, bounds.Height - top - bottom);

            return new Rect(left, top, w, h);
        }

        protected override void RenderChart(SKCanvas context)
        {
            if (ItemsSource == null) return;

            var items = new List<DivergingItem>();
            double maxVal = 0.0;

            int index = 0;
            foreach (var rawItem in ItemsSource)
            {
                if (rawItem == null) continue;

                var catObj = ResolvePropertyValue(rawItem, CategoryPath);
                var leftObj = ResolvePropertyValue(rawItem, LeftValuePath);
                var rightObj = ResolvePropertyValue(rawItem, RightValuePath);

                string category = catObj?.ToString() ?? $"Item {index}";
                double leftVal = Math.Abs(ConvertToDouble(leftObj));
                double rightVal = Math.Abs(ConvertToDouble(rightObj));

                if (double.IsNaN(leftVal)) leftVal = 0.0;
                if (double.IsNaN(rightVal)) rightVal = 0.0;

                items.Add(new DivergingItem
                {
                    Category = category,
                    LeftValue = leftVal,
                    RightValue = rightVal
                });

                maxVal = Math.Max(maxVal, Math.Max(leftVal, rightVal));
                index++;
            }

            if (items.Count == 0) return;
            if (maxVal <= 0) maxVal = 1.0;

            var plot = EffectivePlotArea;
            double slotHeight = plot.Height / items.Count;
            double barHeight = slotHeight * 0.70;
            double topOffset = slotHeight * 0.15;

            double centerX = plot.Left + plot.Width / 2.0;

            var leftBarFill = LeftSKPaint ?? new SolidSKColorSKPaint(SKColor.Parse("#A855F7")); // Purple
            var rightBarFill = RightSKPaint ?? new SolidSKColorSKPaint(SKColor.Parse("#06B6D4")); // Cyan

            var borderSKPaint = new Pen(new SolidSKColorSKPaint(new SKColor((byte)(255), (byte)(255), (byte)(255), (byte)(50))), 1.0);
            var centerLineSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#94A3B8")), 1.5); // Slate gray

            double progress = AnimationProgress;

            // Draw center baseline
            context.DrawLine(centerLineSKPaint, new Point(centerX, plot.Top), new Point(centerX, plot.Bottom));

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                double y = plot.Top + i * slotHeight + topOffset;

                // Left bar width (animating outwards)
                double leftW = (item.LeftValue / maxVal) * (plot.Width / 2.0) * progress;
                var leftRect = new Rect(centerX - leftW, y, leftW, barHeight);

                // Right bar width (animating outwards)
                double rightW = (item.RightValue / maxVal) * (plot.Width / 2.0) * progress;
                var rightRect = new Rect(centerX, y, rightW, barHeight);

                // Draw bars
                context.DrawRectangle(leftBarFill, borderSKPaint, leftRect);
                context.DrawRectangle(rightBarFill, borderSKPaint, rightRect);

                // Draw category label on the left margin
                var textSKPaint = SystemSKPaint;
                var ftLabel = new FormattedText(
                    item.Category,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                    11,
                    textSKPaint);
                
                double tx = plot.Left - ftLabel.Width - 10;
                double ty = y + (barHeight - ftLabel.Height) / 2.0;
                context.DrawText(ftLabel, new Point(tx, ty));

                // Draw values on the bars
                if (leftW > 25)
                {
                    var ftVal = new FormattedText(
                        item.LeftValue.ToString("F0"),
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                        9,
                        SKPaintes.White);
                    context.DrawText(ftVal, new Point(centerX - leftW + 6, ty));
                }

                if (rightW > 25)
                {
                    var ftVal = new FormattedText(
                        item.RightValue.ToString("F0"),
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                        9,
                        SKPaintes.White);
                    context.DrawText(ftVal, new Point(centerX + rightW - ftVal.Width - 6, ty));
                }
            }
        }

        private static double ConvertToDouble(object? value)
        {
            if (value == null) return double.NaN;
            try { return Convert.ToDouble(value); } catch { return double.NaN; }
        }

        private static object? ResolvePropertyValue(object item, string? path)
        {
            if (string.IsNullOrEmpty(path)) return item;
            var prop = item.GetType().GetProperty(path);
            return prop?.GetValue(item);
        }
    }
}