using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public class WaterfallChart : ChartBase
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(WaterfallChart), new PropertyMetadata(default(IEnumerable?), OnPropertyChanged));

        public static readonly DependencyProperty CategoryPathProperty =
            DependencyProperty.Register(nameof(CategoryPath), typeof(string), typeof(WaterfallChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty ValuePathProperty =
            DependencyProperty.Register(nameof(ValuePath), typeof(string), typeof(WaterfallChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty IsTotalPathProperty =
            DependencyProperty.Register(nameof(IsTotalPath), typeof(string), typeof(WaterfallChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty PositiveSKPaintProperty =
            DependencyProperty.Register(nameof(PositiveSKPaint), typeof(SKPaint), typeof(WaterfallChart), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public static readonly DependencyProperty NegativeSKPaintProperty =
            DependencyProperty.Register(nameof(NegativeSKPaint), typeof(SKPaint), typeof(WaterfallChart), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public static readonly DependencyProperty TotalSKPaintProperty =
            DependencyProperty.Register(nameof(TotalSKPaint), typeof(SKPaint), typeof(WaterfallChart), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public IEnumerable? ItemsSource
        {
            get => (System.Collections.IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string? CategoryPath { get => (string?)GetValue(CategoryPathProperty);
            set => SetValue(CategoryPathProperty, value);
        }

        public string? ValuePath { get => (string?)GetValue(ValuePathProperty);
            set => SetValue(ValuePathProperty, value);
        }

        public string? IsTotalPath { get => (string?)GetValue(IsTotalPathProperty);
            set => SetValue(IsTotalPathProperty, value);
        }

        public SKPaint? PositiveSKPaint { get => (SKPaint?)GetValue(PositiveSKPaintProperty);
            set => SetValue(PositiveSKPaintProperty, value);
        }

        public SKPaint? NegativeSKPaint { get => (SKPaint?)GetValue(NegativeSKPaintProperty);
            set => SetValue(NegativeSKPaintProperty, value);
        }

        public SKPaint? TotalSKPaint { get => (SKPaint?)GetValue(TotalSKPaintProperty);
            set => SetValue(TotalSKPaintProperty, value);
        }

        public WaterfallChart()
        {
        }

        private class WaterfallItem
        {
            public string Category { get; set; } = string.Empty;
            public double Value { get; set; }
            public bool IsTotal { get; set; }
            public double StartValue { get; set; }
            public double EndValue { get; set; }
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double top = 24;
            double bottom = 48;
            double left = 64;
            double right = 24;

            if (!string.IsNullOrEmpty(Title)) top += 28;

            double w = Math.Max(10, bounds.Width - left - right);
            double h = Math.Max(10, bounds.Height - top - bottom);

            return new Rect(left, top, w, h);
        }

        protected override void RenderChart(SKCanvas context)
        {
            if (ItemsSource == null) return;

            var items = new List<WaterfallItem>();
            double runningSum = 0.0;

            int index = 0;
            foreach (var rawItem in ItemsSource)
            {
                if (rawItem == null) continue;

                var catObj = ResolvePropertyValue(rawItem, CategoryPath);
                var valObj = ResolvePropertyValue(rawItem, ValuePath);
                var totObj = ResolvePropertyValue(rawItem, IsTotalPath);

                string category = catObj?.ToString() ?? $"Item {index}";
                double value = ConvertToDouble(valObj);
                if (double.IsNaN(value)) value = 0.0;

                bool isTotal = totObj is bool b && b;

                var item = new WaterfallItem
                {
                    Category = category,
                    Value = value,
                    IsTotal = isTotal
                };

                if (isTotal)
                {
                    item.StartValue = 0.0;
                    item.EndValue = runningSum;
                    item.Value = runningSum;
                }
                else
                {
                    item.StartValue = runningSum;
                    item.EndValue = runningSum + value;
                    runningSum += value;
                }

                items.Add(item);
                index++;
            }

            if (items.Count == 0) return;

            var plot = EffectivePlotArea;

            // Compute bounds
            double minVal = 0.0;
            double maxVal = 0.0;
            foreach (var item in items)
            {
                minVal = Math.Min(minVal, Math.Min(item.StartValue, item.EndValue));
                maxVal = Math.Max(maxVal, Math.Max(item.StartValue, item.EndValue));
            }
            double valRange = maxVal - minVal;
            if (Math.Abs(valRange) < 1e-9) valRange = 1.0;

            // Extra buffer
            maxVal += valRange * 0.1;
            minVal -= valRange * 0.1;
            valRange = maxVal - minVal;

            double slotWidth = plot.Width / items.Count;
            double barWidth = slotWidth * 0.70;
            double leftOffset = slotWidth * 0.15;

            var posFill = PositiveSKPaint ?? new SolidSKColorSKPaint(SKColor.Parse("#10B981")); // Emerald
            var negFill = NegativeSKPaint ?? new SolidSKColorSKPaint(SKColor.Parse("#EF4444")); // Rose
            var totFill = TotalSKPaint ?? new SolidSKColorSKPaint(SKColor.Parse("#3B82F6")); // Blue

            var borderSKPaint = new Pen(new SolidSKColorSKPaint(new SKColor((byte)(255), (byte)(255), (byte)(255), (byte)(80))), 1.0);
            var connectorSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#64748B")), 1.0, DashStyle.Dash);

            double progress = AnimationProgress;

            // Draw baseline zero gridline
            double zeroY = plot.Top + plot.Height * (1.0 - (0.0 - minVal) / valRange);
            if (zeroY >= plot.Top && zeroY <= plot.Bottom)
            {
                context.DrawLine(new Pen(new SolidSKColorSKPaint(SKColor.Parse("#475569")), 1.0), new Point(plot.Left, zeroY), new Point(plot.Right, zeroY));
            }

            Point? lastBarCorner = null;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                double x = plot.Left + i * slotWidth + leftOffset;

                // Animate entry: columns grow from start value to end value
                double animEnd = item.StartValue + (item.EndValue - item.StartValue) * progress;

                double yStart = plot.Top + plot.Height * (1.0 - (item.StartValue - minVal) / valRange);
                double yEnd = plot.Top + plot.Height * (1.0 - (animEnd - minVal) / valRange);

                double rectY = Math.Min(yStart, yEnd);
                double rectHeight = Math.Max(2.0, Math.Abs(yStart - yEnd));

                var barRect = new Rect(x, rectY, barWidth, rectHeight);

                SKPaint fillSKPaint = totFill;
                if (!item.IsTotal)
                {
                    fillSKPaint = item.Value >= 0 ? posFill : negFill;
                }

                // Draw column
                context.DrawRectangle(fillSKPaint, borderSKPaint, barRect);

                // Draw connector line from previous column
                if (lastBarCorner.HasValue)
                {
                    context.DrawLine(connectorSKPaint, lastBarCorner.Value, new Point(x, yStart));
                }

                // Update corner for next connector
                lastBarCorner = new Point(x + barWidth, yEnd);

                // Draw Category Label
                var textSKPaint = SystemSKPaint;
                var ft = new FormattedText(
                    item.Category,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal),
                    10,
                    textSKPaint);
                
                double tx = x + (barWidth - ft.Width) / 2.0;
                double ty = plot.Bottom + 8;
                context.DrawText(ft, new Point(tx, ty));

                // Draw Value Label on top
                var valFt = new FormattedText(
                    $"{(item.IsTotal ? "" : (item.Value >= 0 ? "+" : ""))}{item.Value:F0}",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                    10,
                    textSKPaint);

                double vx = x + (barWidth - valFt.Width) / 2.0;
                double vy = rectY - 14;
                context.DrawText(valFt, new Point(vx, vy));
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