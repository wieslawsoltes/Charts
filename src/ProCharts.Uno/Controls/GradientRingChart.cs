using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public partial class GradientRingChart : ChartBase
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(GradientRingChart), new PropertyMetadata(default(IEnumerable?), OnPropertyChanged));

        public static readonly DependencyProperty ValuePathProperty =
            DependencyProperty.Register(nameof(ValuePath), typeof(string), typeof(GradientRingChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty TitlePathProperty =
            DependencyProperty.Register(nameof(TitlePath), typeof(string), typeof(GradientRingChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty ColorPathProperty =
            DependencyProperty.Register(nameof(ColorPath), typeof(string), typeof(GradientRingChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty RingThicknessProperty =
            DependencyProperty.Register(nameof(RingThickness), typeof(double), typeof(GradientRingChart), new PropertyMetadata(12.0, OnPropertyChanged));

        public static readonly DependencyProperty RingSpacingProperty =
            DependencyProperty.Register(nameof(RingSpacing), typeof(double), typeof(GradientRingChart), new PropertyMetadata(6.0, OnPropertyChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(GradientRingChart), new PropertyMetadata(100.0, OnPropertyChanged));

        public IEnumerable? ItemsSource
        {
            get => (System.Collections.IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string? ValuePath { get => (string?)GetValue(ValuePathProperty);
            set => SetValue(ValuePathProperty, value);
        }

        public string? TitlePath { get => (string?)GetValue(TitlePathProperty);
            set => SetValue(TitlePathProperty, value);
        }

        public string? ColorPath { get => (string?)GetValue(ColorPathProperty);
            set => SetValue(ColorPathProperty, value);
        }

        public double RingThickness { get => (double)GetValue(RingThicknessProperty);
            set => SetValue(RingThicknessProperty, value);
        }

        public double RingSpacing { get => (double)GetValue(RingSpacingProperty);
            set => SetValue(RingSpacingProperty, value);
        }

        public double Maximum { get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        private class GradientRingItem
        {
            public string Title { get; set; } = string.Empty;
            public double Value { get; set; }
            public Brush? CustomBrush { get; set; }
            public double OuterRadius { get; set; }
            public double InnerRadius { get; set; }
        }

        private List<GradientRingItem> _processedItems = new List<GradientRingItem>();
        private int _hoveredItemIndex = -1;

        public GradientRingChart()
        {
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double padding = 20;
            if (!string.IsNullOrEmpty(Title)) padding += 24;

            double side = Math.Min(bounds.Width, bounds.Height) - padding * 2;
            side = Math.Max(10, side);

            double cx = (bounds.Width - side) / 2.0;
            double cy = (bounds.Height - side) / 2.0;
            if (!string.IsNullOrEmpty(Title)) cy += 12;

            return new Rect(cx, cy, side, side);
        }

        protected override void RenderChart(DrawingContext context)
        {
            _processedItems.Clear();
            if (ItemsSource == null) return;

            // 1. Process Items
            var items = new List<GradientRingItem>();
            int idx = 0;
            var activePalette = Palette ?? Palette.Default;

            foreach (var rawItem in ItemsSource)
            {
                if (rawItem == null) continue;

                var valObj = ResolvePropertyValue(rawItem, ValuePath);
                var titleObj = ResolvePropertyValue(rawItem, TitlePath);
                var colorObj = ResolvePropertyValue(rawItem, ColorPath);

                double val = ConvertToDouble(valObj);
                string title = titleObj?.ToString() ?? $"Ring {idx + 1}";
                Brush? customBrush = null;

                if (colorObj is Brush brush) customBrush = brush;
                else if (colorObj is Color color) customBrush = new SolidColorBrush(color);
                else if (colorObj is string colStr)
                {
                    try { customBrush = new SolidColorBrush(Color.Parse(colStr)); } catch { }
                }

                if (double.IsNaN(val)) val = 0;

                items.Add(new GradientRingItem
                {
                    Title = title,
                    Value = val,
                    CustomBrush = customBrush ?? activePalette.GetBrush(idx)
                });
                idx++;
            }

            if (items.Count == 0) return;

            var area = EffectivePlotArea;
            var center = area.Center;

            double thickness = Math.Clamp(RingThickness, 2.0, 30.0);
            double spacing = Math.Clamp(RingSpacing, 0.0, 20.0);
            double maxR = area.Width / 2.0;

            double currentR = maxR - thickness / 2.0;

            // 2. Render Rings Concentrically (Outermost to Innermost)
            double maxVal = Maximum <= 0 ? 100.0 : Maximum;
            double progress = AnimationProgress;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (currentR <= thickness / 2.0) break; // Too small to render

                item.OuterRadius = currentR + thickness / 2.0;
                item.InnerRadius = currentR - thickness / 2.0;
                _processedItems.Add(item);

                double pct = Math.Clamp(item.Value / maxVal, 0.0, 1.0) * progress;

                // 2a. Draw Track
                var trackBrush = new Pen(new SolidColorBrush(Color.Parse("#1E293B")) { Opacity = 0.4 }, thickness);
                context.DrawEllipse(null, trackBrush, center, currentR, currentR);

                // 2b. Draw Sweeping Progress Arc
                double sweepAngleDeg = pct * 360.0;
                if (sweepAngleDeg > 0.01)
                {
                    var ringBrush = item.CustomBrush ?? activePalette.GetBrush(i);
                    if (i == _hoveredItemIndex)
                    {
                        // Add glow/highlight effect for hovered ring
                        var highlightColor = new Color((byte)(255), (byte)(255), (byte)(255), (byte)(220));
                        ringBrush = new SolidColorBrush(Color.Parse("#FFFFFF"));
                    }

                    var penBrush = new Pen(ringBrush, thickness, lineCap: PenLineCap.Round);

                    if (sweepAngleDeg >= 359.9)
                    {
                        context.DrawEllipse(null, penBrush, center, currentR, currentR);
                    }
                    else
                    {
                        var geometry = new StreamGeometry();
                        using (var ctx = geometry.Open())
                        {
                            double startAngleDeg = -90.0;
                            double radStart = startAngleDeg * Math.PI / 180.0;
                            double radEnd = (startAngleDeg + sweepAngleDeg) * Math.PI / 180.0;

                            Point startPt = center + new Point(currentR * Math.Cos(radStart), currentR * Math.Sin(radStart));
                            Point endPt = center + new Point(currentR * Math.Cos(radEnd), currentR * Math.Sin(radEnd));

                            geometry.MoveTo(startPt, false);
                            ctx.ArcTo(endPt, new Size(currentR, currentR), 0.0, sweepAngleDeg > 180.0, SweepDirection.Clockwise);
                        }
                        context.DrawGeometry(null, penBrush, geometry);
                    }
                }

                currentR -= (thickness + spacing);
            }
        }

        protected override void UpdateHoverState(Point? mousePoint)
        {
            if (!mousePoint.HasValue || _processedItems.Count == 0)
            {
                _hoveredItemIndex = -1;
                return;
            }

            var center = EffectivePlotArea.Center;
            var diff = mousePoint.Value - center;
            double dist = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);

            int hoverIdx = -1;
            for (int i = 0; i < _processedItems.Count; i++)
            {
                var item = _processedItems[i];
                if (dist >= item.InnerRadius && dist <= item.OuterRadius)
                {
                    hoverIdx = i;
                    break;
                }
            }

            _hoveredItemIndex = hoverIdx;
        }

        protected override void DrawTooltip(DrawingContext context, Point mousePoint)
        {
            if (_hoveredItemIndex == -1) return;

            var hovered = _processedItems[_hoveredItemIndex];
            double maxVal = Maximum <= 0 ? 100.0 : Maximum;
            double pct = Math.Clamp(hovered.Value / maxVal, 0.0, 1.0) * 100.0;

            double padding = 10;
            double textHeight = 18;
            double tooltipWidth = 150;
            double tooltipHeight = padding * 2 + textHeight * 2;

            var fontTitle = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            var fontText = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var textBrush = Brushes.White;

            var ftTitle = new FormattedText(hovered.Title, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontTitle, 11, textBrush);
            var ftVal = new FormattedText($"Value: {hovered.Value:N1} ({pct:F0}%)", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontText, 11, textBrush);

            tooltipWidth = Math.Max(tooltipWidth, Math.Max(ftTitle.Width, ftVal.Width) + padding * 2 + 10);

            double tx = mousePoint.X + 15;
            double ty = mousePoint.Y + 15;

            if (tx + tooltipWidth > Bounds.Width) tx = mousePoint.X - tooltipWidth - 15;
            if (ty + tooltipHeight > Bounds.Height) ty = mousePoint.Y - tooltipHeight - 15;

            tx = Math.Max(0, tx);
            ty = Math.Max(0, ty);

            var tooltipRect = new Rect(tx, ty, tooltipWidth, tooltipHeight);
            var bgBrush = new SolidColorBrush(Color.Parse("#EC1E293B")); // glassmorphic dark slate
            var borderBrush = new Pen(new SolidColorBrush(Color.Parse("#30FFFFFF")), 1.0);

            context.DrawRectangle(bgBrush, borderBrush, new RoundedRect(tooltipRect, new CornerRadius(6.0)));

            context.DrawEllipse(hovered.CustomBrush, null, new Point(tx + padding + 4, ty + padding + 6), 3.5, 3.5);
            context.DrawText(ftTitle, new Point(tx + padding + 12, ty + padding));
            context.DrawText(ftVal, new Point(tx + padding + 12, ty + padding + textHeight));
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