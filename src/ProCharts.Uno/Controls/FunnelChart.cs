using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public partial class FunnelChart : ChartBase
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(FunnelChart), new PropertyMetadata(default(IEnumerable?), OnPropertyChanged));

        public static readonly DependencyProperty ValuePathProperty =
            DependencyProperty.Register(nameof(ValuePath), typeof(string), typeof(FunnelChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty TitlePathProperty =
            DependencyProperty.Register(nameof(TitlePath), typeof(string), typeof(FunnelChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty SKColorPathProperty =
            DependencyProperty.Register(nameof(SKColorPath), typeof(string), typeof(FunnelChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty NeckWidthPercentProperty =
            DependencyProperty.Register(nameof(NeckWidthPercent), typeof(double), typeof(FunnelChart), new PropertyMetadata(0.35, OnPropertyChanged));

        public static readonly DependencyProperty SegmentGapProperty =
            DependencyProperty.Register(nameof(SegmentGap), typeof(double), typeof(FunnelChart), new PropertyMetadata(4.0, OnPropertyChanged));

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

        public string? SKColorPath { get => (string?)GetValue(SKColorPathProperty);
            set => SetValue(SKColorPathProperty, value);
        }

        public double NeckWidthPercent { get => (double)GetValue(NeckWidthPercentProperty);
            set => SetValue(NeckWidthPercentProperty, value);
        }

        public double SegmentGap { get => (double)GetValue(SegmentGapProperty);
            set => SetValue(SegmentGapProperty, value);
        }

        private class FunnelSegment
        {
            public string Title { get; set; } = string.Empty;
            public double Value { get; set; }
            public SKPaint? SKPaint { get; set; }
            public double TopY { get; set; }
            public double BottomY { get; set; }
            public double TopWidth { get; set; }
            public double BottomWidth { get; set; }
        }

        private List<FunnelSegment> _segments = new List<FunnelSegment>();
        private int _hoveredSegmentIndex = -1;

        public FunnelChart()
        {
        }

        protected override void RenderChart(SKCanvas context)
        {
            _segments.Clear();
            if (ItemsSource == null) return;

            var rawItems = new List<(string Title, double Value, SKPaint? SKPaint)>();
            int idx = 0;
            var activePalette = Palette ?? Palette.Default;

            foreach (var rawItem in ItemsSource)
            {
                if (rawItem == null) continue;

                var valObj = ResolvePropertyValue(rawItem, ValuePath);
                var titleObj = ResolvePropertyValue(rawItem, TitlePath);
                var colorObj = ResolvePropertyValue(rawItem, SKColorPath);

                double val = ConvertToDouble(valObj);
                string title = titleObj?.ToString() ?? $"Stage {idx + 1}";
                SKPaint? brush = null;

                if (colorObj is SKPaint b) brush = b;
                else if (colorObj is SKColor c) brush = new SolidSKColorSKPaint(c);
                else if (colorObj is string colStr)
                {
                    try { brush = new SolidSKColorSKPaint(SKColor.Parse(colStr)); } catch { }
                }

                if (!double.IsNaN(val) && val > 0)
                {
                    rawItems.Add((title, val, brush ?? activePalette.GetSKPaint(idx)));
                    idx++;
                }
            }

            if (rawItems.Count == 0) return;

            // Sort descending so it looks like a funnel (though user may pass pre-sorted stages)
            // Typically funnels are plotted in the order received, but let's keep original order.
            var area = EffectivePlotArea;
            int n = rawItems.Count;
            double totalHeight = area.Height;
            double segmentH = totalHeight / n;
            double gap = SegmentGap;

            double maxVal = rawItems.Max(r => r.Value);
            if (maxVal <= 0) maxVal = 1.0;

            double progress = AnimationProgress;
            double neckPct = Math.Clamp(NeckWidthPercent, 0.05, 1.0);

            // Compute widths for each step
            var widths = new double[n + 1];
            for (int i = 0; i < n; i++)
            {
                widths[i] = area.Width * (rawItems[i].Value / maxVal);
            }
            // Bottom width of the last segment is tapered by neck percent
            widths[n] = widths[n - 1] * neckPct;

            for (int i = 0; i < n; i++)
            {
                double topY = area.Top + i * segmentH + gap / 2.0;
                double botY = area.Top + (i + 1) * segmentH - gap / 2.0;

                // Animate Y sizing (slide in from top or expand)
                double midY = (topY + botY) / 2.0;
                double animatedTopY = midY + (topY - midY) * progress;
                double animatedBotY = midY + (botY - midY) * progress;

                double topW = widths[i];
                double botW = widths[i + 1];

                // Interpolate bottom width so it blends beautifully with the next stage's top width
                if (i < n - 1)
                {
                    botW = widths[i + 1];
                }

                var seg = new FunnelSegment
                {
                    Title = rawItems[i].Title,
                    Value = rawItems[i].Value,
                    SKPaint = rawItems[i].SKPaint,
                    TopY = animatedTopY,
                    BottomY = animatedBotY,
                    TopWidth = topW,
                    BottomWidth = botW
                };

                _segments.Add(seg);

                // Draw Trapezoid
                var geometry = new SKPath();
                using (var ctx = geometry.Open())
                {
                    geometry.MoveTo(new Point(area.Center.X - topW / 2.0, animatedTopY), true);
                    geometry.LineTo(new Point(area.Center.X + topW / 2.0, animatedTopY));
                    geometry.LineTo(new Point(area.Center.X + botW / 2.0, animatedBotY));
                    geometry.LineTo(new Point(area.Center.X - botW / 2.0, animatedBotY));
                }

                var fillSKPaint = seg.SKPaint ?? activePalette.GetSKPaint(i);
                if (i == _hoveredSegmentIndex)
                {
                    // Create beautiful neon hover glow by using white outline or overlay
                    fillSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#FFFFFF"));
                }

                var borderSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#40FFFFFF")), 1.0);
                context.DrawGeometry(fillSKPaint, borderSKPaint, geometry);

                // Draw Center Text Label inside the segment
                if (animatedBotY - animatedTopY > 18)
                {
                    string label = $"{seg.Title}: {seg.Value:N0}";
                    var font = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
                    var textSKPaint = SKPaintes.White;

                    var ft = new FormattedText(label, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, 11, textSKPaint);
                    if (ft.Width < Math.Min(topW, botW) - 10)
                    {
                        context.DrawText(ft, new Point(area.Center.X - ft.Width / 2.0, midY - ft.Height / 2.0));
                    }
                    else
                    {
                        // Draw label on the right side if too narrow
                        var rightLabelSKPaint = SystemSKPaint;
                        var ftSide = new FormattedText(label, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, 10, rightLabelSKPaint);
                        context.DrawText(ftSide, new Point(area.Center.X + Math.Max(topW, botW) / 2.0 + 8, midY - ftSide.Height / 2.0));
                    }
                }
            }
        }

        protected override void UpdateHoverState(Point? mousePoint)
        {
            if (!mousePoint.HasValue || _segments.Count == 0)
            {
                _hoveredSegmentIndex = -1;
                return;
            }

            var area = EffectivePlotArea;
            int hoverIdx = -1;

            for (int i = 0; i < _segments.Count; i++)
            {
                var seg = _segments[i];
                if (mousePoint.Value.Y >= seg.TopY && mousePoint.Value.Y <= seg.BottomY)
                {
                    // Mathematically determine if mouse is inside the tapered trapezoid width
                    double t = (mousePoint.Value.Y - seg.TopY) / (seg.BottomY - seg.TopY);
                    double wAtY = seg.TopWidth + (seg.BottomWidth - seg.TopWidth) * t;

                    if (Math.Abs(mousePoint.Value.X - area.Center.X) <= wAtY / 2.0)
                    {
                        hoverIdx = i;
                        break;
                    }
                }
            }

            _hoveredSegmentIndex = hoverIdx;
        }

        protected override void DrawTooltip(SKCanvas context, Point mousePoint)
        {
            if (_hoveredSegmentIndex == -1) return;

            var hovered = _segments[_hoveredSegmentIndex];
            double maxVal = _segments.Max(s => s.Value);
            double pctOfMax = (hovered.Value / maxVal) * 100.0;

            double padding = 10;
            double textHeight = 18;
            double tooltipWidth = 160;
            double tooltipHeight = padding * 2 + textHeight * 2;

            var fontTitle = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            var fontText = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var textSKPaint = SKPaintes.White;

            var ftTitle = new FormattedText(hovered.Title, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontTitle, 11, textSKPaint);
            var ftVal = new FormattedText($"Value: {hovered.Value:N0} ({pctOfMax:F1}% of Max)", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontText, 11, textSKPaint);

            tooltipWidth = Math.Max(tooltipWidth, Math.Max(ftTitle.Width, ftVal.Width) + padding * 2 + 10);

            double tx = mousePoint.X + 15;
            double ty = mousePoint.Y + 15;

            if (tx + tooltipWidth > Bounds.Width) tx = mousePoint.X - tooltipWidth - 15;
            if (ty + tooltipHeight > Bounds.Height) ty = mousePoint.Y - tooltipHeight - 15;

            tx = Math.Max(0, tx);
            ty = Math.Max(0, ty);

            var tooltipRect = new Rect(tx, ty, tooltipWidth, tooltipHeight);
            var bgSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#EC1F242E"));
            var borderSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#30FFFFFF")), 1.0);

            context.DrawRectangle(bgSKPaint, borderSKPaint, new RoundedRect(tooltipRect, new CornerRadius(6.0)));

            context.DrawEllipse(hovered.SKPaint, null, new Point(tx + padding + 4, ty + padding + 6), 3.5, 3.5);
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