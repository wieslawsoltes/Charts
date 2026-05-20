using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Maths;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class TornadoChart : ChartBase
    {
        public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
            AvaloniaProperty.Register<TornadoChart, IEnumerable?>(nameof(ItemsSource));

        public static readonly StyledProperty<string?> CategoryPathProperty =
            AvaloniaProperty.Register<TornadoChart, string?>(nameof(CategoryPath));

        public static readonly StyledProperty<string?> LowValuePathProperty =
            AvaloniaProperty.Register<TornadoChart, string?>(nameof(LowValuePath));

        public static readonly StyledProperty<string?> HighValuePathProperty =
            AvaloniaProperty.Register<TornadoChart, string?>(nameof(HighValuePath));

        public static readonly StyledProperty<double> BaseValueProperty =
            AvaloniaProperty.Register<TornadoChart, double>(nameof(BaseValue), 0.0);

        public static readonly StyledProperty<IBrush?> LowBrushProperty =
            AvaloniaProperty.Register<TornadoChart, IBrush?>(nameof(LowBrush));

        public static readonly StyledProperty<IBrush?> HighBrushProperty =
            AvaloniaProperty.Register<TornadoChart, IBrush?>(nameof(HighBrush));

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

        public string? LowValuePath
        {
            get => GetValue(LowValuePathProperty);
            set => SetValue(LowValuePathProperty, value);
        }

        public string? HighValuePath
        {
            get => GetValue(HighValuePathProperty);
            set => SetValue(HighValuePathProperty, value);
        }

        public double BaseValue
        {
            get => GetValue(BaseValueProperty);
            set => SetValue(BaseValueProperty, value);
        }

        public IBrush? LowBrush
        {
            get => GetValue(LowBrushProperty);
            set => SetValue(LowBrushProperty, value);
        }

        public IBrush? HighBrush
        {
            get => GetValue(HighBrushProperty);
            set => SetValue(HighBrushProperty, value);
        }

        public TornadoChart()
        {
            ItemsSourceProperty.Changed.AddClassHandler<TornadoChart>((x, e) => x.InvalidateVisual());
            CategoryPathProperty.Changed.AddClassHandler<TornadoChart>((x, e) => x.InvalidateVisual());
            LowValuePathProperty.Changed.AddClassHandler<TornadoChart>((x, e) => x.InvalidateVisual());
            HighValuePathProperty.Changed.AddClassHandler<TornadoChart>((x, e) => x.InvalidateVisual());
            BaseValueProperty.Changed.AddClassHandler<TornadoChart>((x, e) => x.InvalidateVisual());
            LowBrushProperty.Changed.AddClassHandler<TornadoChart>((x, e) => x.InvalidateVisual());
            HighBrushProperty.Changed.AddClassHandler<TornadoChart>((x, e) => x.InvalidateVisual());
        }

        private class TornadoItem
        {
            public string Category { get; set; } = string.Empty;
            public double LowValue { get; set; }
            public double HighValue { get; set; }
            public double Spread => Math.Abs(HighValue - LowValue);
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double top = 24;
            double bottom = 24;
            double left = 100; // Extra left margin for variable labels
            double right = 32;

            if (!string.IsNullOrEmpty(Title)) top += 28;

            double w = Math.Max(10, bounds.Width - left - right);
            double h = Math.Max(10, bounds.Height - top - bottom);

            return new Rect(left, top, w, h);
        }

        protected override void RenderChart(DrawingContext context)
        {
            if (ItemsSource == null) return;

            var rawItems = new List<TornadoItem>();
            double baseVal = BaseValue;

            int index = 0;
            foreach (var rawItem in ItemsSource)
            {
                if (rawItem == null) continue;

                var catObj = ResolvePropertyValue(rawItem, CategoryPath);
                var lowObj = ResolvePropertyValue(rawItem, LowValuePath);
                var highObj = ResolvePropertyValue(rawItem, HighValuePath);

                string category = catObj?.ToString() ?? $"Variable {index}";
                double lowVal = ConvertToDouble(lowObj);
                double highVal = ConvertToDouble(highObj);

                if (double.IsNaN(lowVal)) lowVal = baseVal;
                if (double.IsNaN(highVal)) highVal = baseVal;

                rawItems.Add(new TornadoItem
                {
                    Category = category,
                    LowValue = lowVal,
                    HighValue = highVal
                });
                index++;
            }

            if (rawItems.Count == 0) return;

            // Sort descending by spread to form the signature "Tornado funnel"
            var items = rawItems.OrderByDescending(x => x.Spread).ToList();

            double minVal = Math.Min(baseVal, items.Min(x => Math.Min(x.LowValue, x.HighValue)));
            double maxVal = Math.Max(baseVal, items.Max(x => Math.Max(x.LowValue, x.HighValue)));
            double valRange = maxVal - minVal;
            if (Math.Abs(valRange) < 1e-9) valRange = 1.0;

            var plot = EffectivePlotArea;
            double slotHeight = plot.Height / items.Count;
            double barHeight = slotHeight * 0.70;
            double topOffset = slotHeight * 0.15;

            var lowBarFill = LowBrush ?? new SolidColorBrush(Color.Parse("#F43F5E")); // Rose Red
            var highBarFill = HighBrush ?? new SolidColorBrush(Color.Parse("#10B981")); // Emerald Green

            var borderPen = new Pen(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)), 1.0);
            var baseLinePen = new Pen(new SolidColorBrush(Color.Parse("#E2E8F0")), 1.5); // Slate gray

            double progress = AnimationProgress;

            // Compute screen X of baseline
            double basePctX = (baseVal - minVal) / valRange;
            double baseScreenX = plot.Left + basePctX * plot.Width;

            // Draw center base case line
            context.DrawLine(baseLinePen, new Point(baseScreenX, plot.Top), new Point(baseScreenX, plot.Bottom));

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                double y = plot.Top + i * slotHeight + topOffset;

                // Values mapped to percentage positions
                double lowPct = (item.LowValue - minVal) / valRange;
                double highPct = (item.HighValue - minVal) / valRange;

                double lowScreenX = plot.Left + lowPct * plot.Width;
                double highScreenX = plot.Left + highPct * plot.Width;

                // Animate expansion from the baseScreenX baseline outward
                double animLowX = baseScreenX + (lowScreenX - baseScreenX) * progress;
                double animHighX = baseScreenX + (highScreenX - baseScreenX) * progress;

                // Low bar (from baseline to animated low value)
                double lowLeft = Math.Min(baseScreenX, animLowX);
                double lowWidth = Math.Max(1.0, Math.Abs(baseScreenX - animLowX));
                var lowRect = new Rect(lowLeft, y, lowWidth, barHeight);

                // High bar (from baseline to animated high value)
                double highLeft = Math.Min(baseScreenX, animHighX);
                double highWidth = Math.Max(1.0, Math.Abs(baseScreenX - animHighX));
                var highRect = new Rect(highLeft, y, highWidth, barHeight);

                // Draw bars
                context.DrawRectangle(lowBarFill, borderPen, lowRect);
                context.DrawRectangle(highBarFill, borderPen, highRect);

                // Draw category sensitivity label
                var textBrush = SystemBrush;
                var ftLabel = new FormattedText(
                    item.Category,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                    10,
                    textBrush);
                
                double tx = plot.Left - ftLabel.Width - 10;
                double ty = y + (barHeight - ftLabel.Height) / 2.0;
                context.DrawText(ftLabel, new Point(tx, ty));

                // Draw values on extreme ends
                var ftLow = new FormattedText(
                    item.LowValue.ToString("F1"),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                    9,
                    textBrush);
                context.DrawText(ftLow, new Point(Math.Min(animLowX, animHighX) - ftLow.Width - 6, ty));

                var ftHigh = new FormattedText(
                    item.HighValue.ToString("F1"),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                    9,
                    textBrush);
                context.DrawText(ftHigh, new Point(Math.Max(animLowX, animHighX) + 6, ty));
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
