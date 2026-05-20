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
    public class WaffleChart : ChartBase
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(WaffleChart), new PropertyMetadata(default(IEnumerable?), OnPropertyChanged));

        public static readonly DependencyProperty ValuePathProperty =
            DependencyProperty.Register(nameof(ValuePath), typeof(string), typeof(WaffleChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty TitlePathProperty =
            DependencyProperty.Register(nameof(TitlePath), typeof(string), typeof(WaffleChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty SKColorPathProperty =
            DependencyProperty.Register(nameof(SKColorPath), typeof(string), typeof(WaffleChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty CellSpacingProperty =
            DependencyProperty.Register(nameof(CellSpacing), typeof(double), typeof(WaffleChart), new PropertyMetadata(3.0, OnPropertyChanged));

        public static readonly DependencyProperty CellCornerRadiusProperty =
            DependencyProperty.Register(nameof(CellCornerRadius), typeof(double), typeof(WaffleChart), new PropertyMetadata(3.0, OnPropertyChanged));

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

        public double CellSpacing { get => (double)GetValue(CellSpacingProperty);
            set => SetValue(CellSpacingProperty, value);
        }

        public double CellCornerRadius { get => (double)GetValue(CellCornerRadiusProperty);
            set => SetValue(CellCornerRadiusProperty, value);
        }

        private class WaffleCategory
        {
            public int Index { get; set; }
            public string Title { get; set; } = string.Empty;
            public double Value { get; set; }
            public int CellCount { get; set; }
            public SKPaint? SKPaint { get; set; }
        }

        private List<WaffleCategory> _categories = new List<WaffleCategory>();
        private int[] _gridCells = new int[100]; // Category index for each cell
        private int _hoveredCategoryIndex = -1;
        private Point? _rawMousePos;

        public WaffleChart()
        {
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double padding = 24;
            if (!string.IsNullOrEmpty(Title)) padding += 28;

            double side = Math.Min(bounds.Width, bounds.Height) - padding * 2;
            side = Math.Max(10, side);

            // Center square layout
            double cx = (bounds.Width - side) / 2.0;
            double cy = (bounds.Height - side) / 2.0;
            if (!string.IsNullOrEmpty(Title)) cy += 12;

            return new Rect(cx, cy, side, side);
        }

        protected override void RenderChart(SKCanvas context)
        {
            _categories.Clear();
            Array.Clear(_gridCells, 0, _gridCells.Length);

            if (ItemsSource == null) return;

            // 1. Process categories
            var items = new List<(string Title, double Value, SKPaint? SKPaint)>();
            int idx = 0;
            var activePalette = Palette ?? Palette.Default;

            foreach (var rawItem in ItemsSource)
            {
                if (rawItem == null) continue;

                var valObj = ResolvePropertyValue(rawItem, ValuePath);
                var titleObj = ResolvePropertyValue(rawItem, TitlePath);
                var colorObj = ResolvePropertyValue(rawItem, SKColorPath);

                double val = ConvertToDouble(valObj);
                string title = titleObj?.ToString() ?? $"Category {idx + 1}";
                SKPaint? brush = null;

                if (colorObj is SKPaint b) brush = b;
                else if (colorObj is SKColor c) brush = new SolidSKColorSKPaint(c);
                else if (colorObj is string colStr)
                {
                    try { brush = new SolidSKColorSKPaint(SKColor.Parse(colStr)); } catch { }
                }

                if (!double.IsNaN(val) && val > 0)
                {
                    items.Add((title, val, brush ?? activePalette.GetSKPaint(idx)));
                    idx++;
                }
            }

            if (items.Count == 0) return;

            // 2. Allocate 100 squares using Largest Remainder Method
            double totalVal = items.Sum(i => i.Value);
            if (totalVal <= 0) return;

            var rawCounts = new double[items.Count];
            var counts = new int[items.Count];
            double sumCount = 0;

            for (int i = 0; i < items.Count; i++)
            {
                rawCounts[i] = (items[i].Value / totalVal) * 100.0;
                counts[i] = (int)Math.Floor(rawCounts[i]);
                sumCount += counts[i];
            }

            int remainder = 100 - (int)sumCount;
            if (remainder > 0)
            {
                var remainders = rawCounts
                    .Select((rc, i) => new { Index = i, Remainder = rc - Math.Floor(rc) })
                    .OrderByDescending(r => r.Remainder)
                    .Take(remainder)
                    .ToList();

                foreach (var r in remainders)
                {
                    counts[r.Index]++;
                }
            }

            for (int i = 0; i < items.Count; i++)
            {
                _categories.Add(new WaffleCategory
                {
                    Index = i,
                    Title = items[i].Title,
                    Value = items[i].Value,
                    CellCount = counts[i],
                    SKPaint = items[i].SKPaint
                });
            }

            // Populate the grid 100 cells. Column-by-column (from bottom-left upwards)
            int cellIdx = 0;
            for (int cat = 0; cat < _categories.Count; cat++)
            {
                for (int c = 0; c < _categories[cat].CellCount; c++)
                {
                    if (cellIdx >= 100) break;
                    _gridCells[cellIdx++] = cat;
                }
            }

            // Fill any remaining cells with background index
            while (cellIdx < 100)
            {
                _gridCells[cellIdx++] = -1;
            }

            // 3. Render 10x10 Grid
            var area = EffectivePlotArea;
            double spacing = CellSpacing;
            double cellSize = (area.Width - 9.0 * spacing) / 10.0;
            double cornerR = CellCornerRadius;

            double progress = AnimationProgress;

            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    int index = col * 10 + row; // Column by column
                    if (index >= 100) continue;

                    int catIdx = _gridCells[index];

                    // Cell bottom-left starts at bottom of plot area
                    double cx = area.Left + col * (cellSize + spacing);
                    double cy = area.Bottom - (row + 1) * (cellSize + spacing) + spacing;

                    var cellRect = new Rect(cx, cy, cellSize, cellSize);

                    // Animate: diagonal wave scale based on col + row distance
                    double diagDist = (col + row) / 18.0; // Normalized 0 to 1
                    double cellProgress = Math.Clamp((progress - diagDist * 0.4) * 2.5, 0.0, 1.0);

                    if (cellProgress <= 0.0) continue;

                    // Scale from center of cell
                    double w = cellSize * cellProgress;
                    double h = cellSize * cellProgress;
                    double dx = (cellSize - w) / 2.0;
                    double dy = (cellSize - h) / 2.0;
                    var animatedRect = new Rect(cellRect.X + dx, cellRect.Y + dy, w, h);

                    SKPaint? cellSKPaint = SKPaintes.Transparent;
                    if (catIdx >= 0 && catIdx < _categories.Count)
                    {
                        cellSKPaint = _categories[catIdx].SKPaint ?? activePalette.GetSKPaint(catIdx);
                    }
                    else
                    {
                        cellSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#1E293B"), 0.3); // Empty gray track
                    }

                    // Hover styling: dim non-hovered categories, highlight hovered
                    if (_hoveredCategoryIndex != -1)
                    {
                        if (catIdx != _hoveredCategoryIndex)
                        {
                            cellSKPaint = new SolidSKColorSKPaint(new SKColor((byte)(148), (byte)(163), (byte)(184), (byte)(40))); // Muted slate gray
                        }
                    }

                    var borderSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#20FFFFFF")), 1.0);
                    context.DrawRectangle(cellSKPaint, borderSKPaint, new RoundedRect(animatedRect, new CornerRadius(cornerR)));
                }
            }
        }

        protected override void UpdateHoverState(Point? mousePoint)
        {
            _rawMousePos = mousePoint;
            if (!mousePoint.HasValue || _categories.Count == 0)
            {
                _hoveredCategoryIndex = -1;
                return;
            }

            var area = EffectivePlotArea;
            double spacing = CellSpacing;
            double cellSize = (area.Width - 9.0 * spacing) / 10.0;
            if (cellSize <= 0) return;

            int hoveredCatIdx = -1;
            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    int index = col * 10 + row;
                    if (index >= 100) continue;

                    int catIdx = _gridCells[index];

                    double cx = area.Left + col * (cellSize + spacing);
                    double cy = area.Bottom - (row + 1) * (cellSize + spacing) + spacing;

                    var cellRect = new Rect(cx, cy, cellSize, cellSize);
                    if (cellRect.Contains(mousePoint.Value))
                    {
                        hoveredCatIdx = catIdx;
                        break;
                    }
                }
                if (hoveredCatIdx != -1) break;
            }

            _hoveredCategoryIndex = hoveredCatIdx;
        }

        protected override void DrawTooltip(SKCanvas context, Point mousePoint)
        {
            if (_hoveredCategoryIndex == -1 || _hoveredCategoryIndex >= _categories.Count) return;

            var hovered = _categories[_hoveredCategoryIndex];
            double totalVal = _categories.Sum(c => c.Value);
            double percentage = (hovered.Value / totalVal) * 100.0;

            double padding = 10;
            double textHeight = 18;
            double tooltipWidth = 150;
            double tooltipHeight = padding * 2 + textHeight * 2;

            var fontTitle = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            var fontText = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var textSKPaint = SKPaintes.White;

            var ftTitle = new FormattedText(hovered.Title, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontTitle, 11, textSKPaint);
            var ftVal = new FormattedText($"{hovered.CellCount} Blocks ({percentage:F1}%)", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontText, 11, textSKPaint);

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