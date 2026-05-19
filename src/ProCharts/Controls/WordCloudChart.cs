using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class WordCloudChart : ChartBase
    {
        public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
            AvaloniaProperty.Register<WordCloudChart, IEnumerable?>(nameof(ItemsSource));

        public static readonly StyledProperty<string?> TextPathProperty =
            AvaloniaProperty.Register<WordCloudChart, string?>(nameof(TextPath));

        public static readonly StyledProperty<string?> WeightPathProperty =
            AvaloniaProperty.Register<WordCloudChart, string?>(nameof(WeightPath));

        public static readonly StyledProperty<double> MinFontSizeProperty =
            AvaloniaProperty.Register<WordCloudChart, double>(nameof(MinFontSize), 10.0);

        public static readonly StyledProperty<double> MaxFontSizeProperty =
            AvaloniaProperty.Register<WordCloudChart, double>(nameof(MaxFontSize), 42.0);

        public IEnumerable? ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string? TextPath
        {
            get => GetValue(TextPathProperty);
            set => SetValue(TextPathProperty, value);
        }

        public string? WeightPath
        {
            get => GetValue(WeightPathProperty);
            set => SetValue(WeightPathProperty, value);
        }

        public double MinFontSize
        {
            get => GetValue(MinFontSizeProperty);
            set => SetValue(MinFontSizeProperty, value);
        }

        public double MaxFontSize
        {
            get => GetValue(MaxFontSizeProperty);
            set => SetValue(MaxFontSizeProperty, value);
        }

        private class WordCloudItem
        {
            public string Text { get; set; } = string.Empty;
            public double Weight { get; set; }
            public IBrush? Brush { get; set; }
            public double FontSize { get; set; }
            public Rect BoundingBox { get; set; }
            public FormattedText? Formatted { get; set; }
        }

        private List<WordCloudItem> _placedWords = new List<WordCloudItem>();
        private int _hoveredWordIndex = -1;

        public WordCloudChart()
        {
            ItemsSourceProperty.Changed.AddClassHandler<WordCloudChart>((x, e) => x.InvalidateVisual());
            TextPathProperty.Changed.AddClassHandler<WordCloudChart>((x, e) => x.InvalidateVisual());
            WeightPathProperty.Changed.AddClassHandler<WordCloudChart>((x, e) => x.InvalidateVisual());
            MinFontSizeProperty.Changed.AddClassHandler<WordCloudChart>((x, e) => x.InvalidateVisual());
            MaxFontSizeProperty.Changed.AddClassHandler<WordCloudChart>((x, e) => x.InvalidateVisual());
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double padding = 16;
            if (!string.IsNullOrEmpty(Title)) padding += 24;

            double w = Math.Max(10, bounds.Width - padding * 2);
            double h = Math.Max(10, bounds.Height - padding * 2);
            double cy = padding;
            if (!string.IsNullOrEmpty(Title)) cy += 12;

            return new Rect(padding, cy, w, h - padding);
        }

        protected override void RenderChart(DrawingContext context)
        {
            _placedWords.Clear();
            if (ItemsSource == null) return;

            // 1. Parse raw inputs
            var rawItems = new List<(string Text, double Weight, IBrush? Brush)>();
            int idx = 0;
            var activePalette = Palette ?? Palette.Default;

            foreach (var rawItem in ItemsSource)
            {
                if (rawItem == null) continue;

                var textObj = ResolvePropertyValue(rawItem, TextPath);
                var weightObj = ResolvePropertyValue(rawItem, WeightPath);

                string text = textObj?.ToString() ?? string.Empty;
                double weight = ConvertToDouble(weightObj);

                if (!string.IsNullOrEmpty(text) && !double.IsNaN(weight) && weight > 0)
                {
                    rawItems.Add((text, weight, activePalette.GetBrush(idx++)));
                }
            }

            if (rawItems.Count == 0) return;

            // Sort words by weight descending (largest first)
            var sortedItems = rawItems.OrderByDescending(r => r.Weight).ToList();

            double minW = sortedItems.Min(s => s.Weight);
            double maxW = sortedItems.Max(s => s.Weight);
            double deltaW = maxW - minW;
            if (deltaW <= 0) deltaW = 1.0;

            double minFont = MinFontSize;
            double maxFont = MaxFontSize;
            if (maxFont <= minFont) maxFont = minFont + 5.0;

            var area = EffectivePlotArea;
            var center = area.Center;

            var font = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);

            // 2. Lay out words using a spiral algorithm
            foreach (var item in sortedItems)
            {
                // Calculate font size proportional to weight
                double fontSize = minFont + (maxFont - minFont) * ((item.Weight - minW) / deltaW);

                var ft = new FormattedText(
                    item.Text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    font,
                    fontSize,
                    item.Brush ?? Brushes.White);

                double wWidth = ft.Width;
                double wHeight = ft.Height;

                // Archimedean spiral search: r = a * theta
                double theta = 0.0;
                double rStep = 3.0; // Radial expansion step
                double thetaStep = 0.15; // Angle increment step
                bool placed = false;
                Rect bestRect = default;

                // Max spiral iterations
                for (int i = 0; i < 400; i++)
                {
                    double r = rStep * theta / (2.0 * Math.PI);
                    double cx = center.X + r * Math.Cos(theta) - wWidth / 2.0;
                    double cy = center.Y + r * Math.Sin(theta) - wHeight / 2.0;

                    var candidate = new Rect(cx, cy, wWidth, wHeight);

                    // Check boundaries
                    if (candidate.Left < area.Left || candidate.Right > area.Right ||
                        candidate.Top < area.Top || candidate.Bottom > area.Bottom)
                    {
                        theta += thetaStep;
                        continue;
                    }

                    // Check overlap
                    bool intersects = false;
                    foreach (var placedWord in _placedWords)
                    {
                        if (candidate.Intersects(placedWord.BoundingBox))
                        {
                            intersects = true;
                            break;
                        }
                    }

                    if (!intersects)
                    {
                        bestRect = candidate;
                        placed = true;
                        break;
                    }

                    theta += thetaStep;
                }

                if (placed)
                {
                    _placedWords.Add(new WordCloudItem
                    {
                        Text = item.Text,
                        Weight = item.Weight,
                        Brush = item.Brush,
                        FontSize = fontSize,
                        BoundingBox = bestRect,
                        Formatted = ft
                    });
                }
            }

            // 3. Render placed words with scale animation
            double progress = AnimationProgress;

            for (int i = 0; i < _placedWords.Count; i++)
            {
                var word = _placedWords[i];
                var box = word.BoundingBox;

                // Smooth scale-in from center of the word's bounding box
                var centerPt = box.Center;
                double animWidth = box.Width * progress;
                double animHeight = box.Height * progress;

                // FormattedText redraw with animated scale
                double currentFontSize = word.FontSize * progress;
                if (currentFontSize < 1.0) continue;

                var textBrush = word.Brush ?? Brushes.White;
                if (i == _hoveredWordIndex)
                {
                    textBrush = new SolidColorBrush(Color.Parse("#FFFFFF")); // Highlight hovered word in pure white
                }

                var ftAnim = new FormattedText(
                    word.Text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    font,
                    currentFontSize,
                    textBrush);

                double tx = centerPt.X - ftAnim.Width / 2.0;
                double ty = centerPt.Y - ftAnim.Height / 2.0;

                // Draw word text
                context.DrawText(ftAnim, new Point(tx, ty));
            }
        }

        protected override void UpdateHoverState(Point? mousePoint)
        {
            if (!mousePoint.HasValue || _placedWords.Count == 0)
            {
                _hoveredWordIndex = -1;
                return;
            }

            int hoveredIdx = -1;
            for (int i = 0; i < _placedWords.Count; i++)
            {
                if (_placedWords[i].BoundingBox.Contains(mousePoint.Value))
                {
                    hoveredIdx = i;
                    break;
                }
            }

            _hoveredWordIndex = hoveredIdx;
        }

        protected override void DrawTooltip(DrawingContext context, Point mousePoint)
        {
            if (_hoveredWordIndex == -1 || _hoveredWordIndex >= _placedWords.Count) return;

            var hovered = _placedWords[_hoveredWordIndex];

            double padding = 10;
            double textHeight = 18;
            double tooltipWidth = 140;
            double tooltipHeight = padding * 2 + textHeight * 2;

            var fontTitle = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            var fontText = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var textBrush = Brushes.White;

            var ftTitle = new FormattedText($"\"{hovered.Text}\"", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontTitle, 11, textBrush);
            var ftVal = new FormattedText($"Weight: {hovered.Weight:N0}", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontText, 11, textBrush);

            tooltipWidth = Math.Max(tooltipWidth, Math.Max(ftTitle.Width, ftVal.Width) + padding * 2 + 10);

            double tx = mousePoint.X + 15;
            double ty = mousePoint.Y + 15;

            if (tx + tooltipWidth > Bounds.Width) tx = mousePoint.X - tooltipWidth - 15;
            if (ty + tooltipHeight > Bounds.Height) ty = mousePoint.Y - tooltipHeight - 15;

            tx = Math.Max(0, tx);
            ty = Math.Max(0, ty);

            var tooltipRect = new Rect(tx, ty, tooltipWidth, tooltipHeight);
            var bgBrush = new SolidColorBrush(Color.Parse("#EC1F242E"));
            var borderPen = new Pen(new SolidColorBrush(Color.Parse("#30FFFFFF")), 1.0);

            context.DrawRectangle(bgBrush, borderPen, new RoundedRect(tooltipRect, new CornerRadius(6.0)));

            context.DrawEllipse(hovered.Brush, null, new Point(tx + padding + 4, ty + padding + 6), 3.5, 3.5);
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
