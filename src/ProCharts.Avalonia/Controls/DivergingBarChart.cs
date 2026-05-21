using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using ProCharts.Avalonia.Maths;
using ProCharts.Avalonia.Styles;

namespace ProCharts.Avalonia.Controls
{
    public class DivergingBarChart : ChartBase
    {
        public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
            AvaloniaProperty.Register<DivergingBarChart, IEnumerable?>(nameof(ItemsSource));

        public static readonly StyledProperty<string?> CategoryPathProperty =
            AvaloniaProperty.Register<DivergingBarChart, string?>(nameof(CategoryPath));

        public static readonly StyledProperty<string?> LeftValuePathProperty =
            AvaloniaProperty.Register<DivergingBarChart, string?>(nameof(LeftValuePath));

        public static readonly StyledProperty<string?> RightValuePathProperty =
            AvaloniaProperty.Register<DivergingBarChart, string?>(nameof(RightValuePath));

        public static readonly StyledProperty<IBrush?> LeftBrushProperty =
            AvaloniaProperty.Register<DivergingBarChart, IBrush?>(nameof(LeftBrush));

        public static readonly StyledProperty<IBrush?> RightBrushProperty =
            AvaloniaProperty.Register<DivergingBarChart, IBrush?>(nameof(RightBrush));

        public IEnumerable? ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string? CategoryPath
        {
            get => GetValue(CategoryPathProperty);
            set => SetValue(CategoryPathProperty, value);
        }

        public string? LeftValuePath
        {
            get => GetValue(LeftValuePathProperty);
            set => SetValue(LeftValuePathProperty, value);
        }

        public string? RightValuePath
        {
            get => GetValue(RightValuePathProperty);
            set => SetValue(RightValuePathProperty, value);
        }

        public IBrush? LeftBrush
        {
            get => GetValue(LeftBrushProperty);
            set => SetValue(LeftBrushProperty, value);
        }

        public IBrush? RightBrush
        {
            get => GetValue(RightBrushProperty);
            set => SetValue(RightBrushProperty, value);
        }

        public DivergingBarChart()
        {
            ItemsSourceProperty.Changed.AddClassHandler<DivergingBarChart>((x, e) => x.InvalidateVisual());
            CategoryPathProperty.Changed.AddClassHandler<DivergingBarChart>((x, e) => x.InvalidateVisual());
            LeftValuePathProperty.Changed.AddClassHandler<DivergingBarChart>((x, e) => x.InvalidateVisual());
            RightValuePathProperty.Changed.AddClassHandler<DivergingBarChart>((x, e) => x.InvalidateVisual());
            LeftBrushProperty.Changed.AddClassHandler<DivergingBarChart>((x, e) => x.InvalidateVisual());
            RightBrushProperty.Changed.AddClassHandler<DivergingBarChart>((x, e) => x.InvalidateVisual());
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

        protected override void RenderChart(DrawingContext context)
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

            var leftBarFill = LeftBrush ?? new SolidColorBrush(Color.Parse("#A855F7")); // Purple
            var rightBarFill = RightBrush ?? new SolidColorBrush(Color.Parse("#06B6D4")); // Cyan

            var borderPen = new Pen(new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)), 1.0);
            var centerLinePen = new Pen(new SolidColorBrush(Color.Parse("#94A3B8")), 1.5); // Slate gray

            double progress = AnimationProgress;

            // Draw center baseline
            context.DrawLine(centerLinePen, new Point(centerX, plot.Top), new Point(centerX, plot.Bottom));

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
                context.DrawRectangle(leftBarFill, borderPen, leftRect);
                context.DrawRectangle(rightBarFill, borderPen, rightRect);

                // Draw category label on the left margin
                var textBrush = SystemBrush;
                var ftLabel = new FormattedText(
                    item.Category,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                    11,
                    textBrush);
                
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
                        Brushes.White);
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
                        Brushes.White);
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
