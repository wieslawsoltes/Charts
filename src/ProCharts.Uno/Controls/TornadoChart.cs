using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public partial class TornadoChart : ChartBase
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TornadoChart), new PropertyMetadata(default(IEnumerable?), OnPropertyChanged));

        public static readonly DependencyProperty CategoryPathProperty =
            DependencyProperty.Register(nameof(CategoryPath), typeof(string), typeof(TornadoChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty LowValuePathProperty =
            DependencyProperty.Register(nameof(LowValuePath), typeof(string), typeof(TornadoChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty HighValuePathProperty =
            DependencyProperty.Register(nameof(HighValuePath), typeof(string), typeof(TornadoChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty BaseValueProperty =
            DependencyProperty.Register(nameof(BaseValue), typeof(double), typeof(TornadoChart), new PropertyMetadata(0.0, OnPropertyChanged));

        public static readonly DependencyProperty LowBrushProperty =
            DependencyProperty.Register(nameof(LowBrush), typeof(Brush), typeof(TornadoChart), new PropertyMetadata(default(Brush?), OnPropertyChanged));

        public static readonly DependencyProperty HighBrushProperty =
            DependencyProperty.Register(nameof(HighBrush), typeof(Brush), typeof(TornadoChart), new PropertyMetadata(default(Brush?), OnPropertyChanged));

        public IEnumerable? ItemsSource
        {
            get => (System.Collections.IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string? CategoryPath { get => (string?)GetValue(CategoryPathProperty);
            set => SetValue(CategoryPathProperty, value);
        }

        public string? LowValuePath { get => (string?)GetValue(LowValuePathProperty);
            set => SetValue(LowValuePathProperty, value);
        }

        public string? HighValuePath { get => (string?)GetValue(HighValuePathProperty);
            set => SetValue(HighValuePathProperty, value);
        }

        public double BaseValue { get => (double)GetValue(BaseValueProperty);
            set => SetValue(BaseValueProperty, value);
        }

        public Brush? LowBrush { get => (Brush?)GetValue(LowBrushProperty);
            set => SetValue(LowBrushProperty, value);
        }

        public Brush? HighBrush { get => (Brush?)GetValue(HighBrushProperty);
            set => SetValue(HighBrushProperty, value);
        }

        public TornadoChart()
        {
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

            var borderBrush = new Pen(new SolidColorBrush(new Color((byte)(255), (byte)(255), (byte)(255), (byte)(40))), 1.0);
            var baseLineBrush = new Pen(new SolidColorBrush(Color.Parse("#E2E8F0")), 1.5); // Slate gray

            double progress = AnimationProgress;

            // Compute screen X of baseline
            double basePctX = (baseVal - minVal) / valRange;
            double baseScreenX = plot.Left + basePctX * plot.Width;

            // Draw center base case line
            context.DrawLine(baseLineBrush, new Point(baseScreenX, plot.Top), new Point(baseScreenX, plot.Bottom));

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
                context.DrawRectangle(lowBarFill, borderBrush, lowRect);
                context.DrawRectangle(highBarFill, borderBrush, highRect);

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