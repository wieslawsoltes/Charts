using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Series;
using ProCharts.Uno.Styles;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Controls
{
    /// <summary>
    /// Represents an engineering trade-off or parameter Carpet Plot chart showing curved, intersecting grid lines.
    /// </summary>
    public partial class CarpetPlotChart : ChartBase
    {
        // --- DEPENDENCY PROPERTIES ---

        /// <summary>
        /// Gets the collection of carpet plot series.
        /// </summary>
        public ObservableCollection<CarpetSeries> Series { get; } = new ObservableCollection<CarpetSeries>();

        private Point? _hoverPoint;
        private int _hoverRow = -1;
        private int _hoverCol = -1;
        private CarpetSeries? _hoverSeries;

        // --- CONSTRUCTOR ---

        /// <summary>
        /// Initializes a new instance of the CarpetPlotChart class.
        /// </summary>
        public CarpetPlotChart()
        {
            Series.CollectionChanged += OnSeriesCollectionChanged;
        }

        private void OnSeriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (CarpetSeries oldSeries in e.OldItems)
                {
                    oldSeries.SeriesChanged -= OnSeriesChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (CarpetSeries newSeries in e.NewItems)
                {
                    newSeries.SeriesChanged += OnSeriesChanged;
                }
            }

            if (IsAnimationEnabled) StartEntryAnimation();
            else InvalidateVisual();
        }

        private void OnSeriesChanged(object? sender, EventArgs e)
        {
            InvalidateVisual();
        }

        /// <inheritdoc />
        protected override void RenderChart(DrawingContext context)
        {
            var visibleSeries = Series.Where(s => s.IsVisible).ToList();
            if (visibleSeries.Count == 0)
            {
                RenderEmptyState(context);
                return;
            }

            // Find absolute bounding boxes
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (var series in visibleSeries)
            {
                if (series.XValues == null || series.YValues == null) continue;
                for (int idx = 0; idx < series.XValues.Length; idx++)
                {
                    if (idx >= series.YValues.Length) break;
                    double xVal = series.XValues[idx];
                    double yVal = series.YValues[idx];

                    if (xVal < minX) minX = xVal;
                    if (xVal > maxX) maxX = xVal;
                    if (yVal < minY) minY = yVal;
                    if (yVal > maxY) maxY = yVal;
                }
            }

            if (minX == double.MaxValue || maxX == double.MinValue || minY == double.MaxValue || maxY == double.MinValue)
            {
                RenderEmptyState(context);
                return;
            }

            double xRange = maxX - minX;
            double yRange = maxY - minY;
            if (xRange <= 0) xRange = 1.0;
            if (yRange <= 0) yRange = 1.0;

            var area = EffectivePlotArea;
            var activePalette = Palette ?? Palette.Default;

            // Draw Cartesian Axes reference guides
            DrawAxesGuides(context, minX, maxX, minY, maxY);

            // Draw Carpet Grids
            for (int sIdx = 0; sIdx < visibleSeries.Count; sIdx++)
            {
                var series = visibleSeries[sIdx];
                if (series.XValues == null || series.YValues == null || series.Rows <= 0 || series.Columns <= 0) continue;

                int actualColorIdx = Series.IndexOf(series);
                var defaultBrush = activePalette.GetBrush(actualColorIdx >= 0 ? actualColorIdx : sIdx);
                var strokePen = new Pen(series.Stroke ?? defaultBrush, series.StrokeThickness);

                // --- 1. DRAW ROW CURVES (Column coordinate connections for each row) ---
                for (int r = 0; r < series.Rows; r++)
                {
                    var geom = new StreamGeometry();
                    using (var ctx = geom.Open())
                    {
                        bool isFirst = true;
                        for (int c = 0; c < series.Columns; c++)
                        {
                            int flatIdx = r * series.Columns + c;
                            if (flatIdx >= series.XValues.Length || flatIdx >= series.YValues.Length) break;

                            double xVal = series.XValues[flatIdx];
                            double yVal = series.YValues[flatIdx];

                            // Translate coordinates
                            double px = area.Left + (xVal - minX) / xRange * area.Width;
                            double py = area.Bottom - (yVal - minY) / yRange * area.Height; // Invert Y

                            // Apply animation offset starting from horizontal centerline
                            double centerY = area.Top + area.Height / 2.0;
                            py = centerY + (py - centerY) * AnimationProgress;

                            var pt = new Point(px, py);
                            if (isFirst)
                            {
                                ctx.BeginFigure(pt, false);
                                isFirst = false;
                            }
                            else
                            {
                                ctx.LineTo(pt);
                            }
                        }
                        ctx.EndFigure(false);
                    }
                    context.DrawGeometry(Brushes.Transparent, strokePen, geom);
                }

                // --- 2. DRAW COLUMN CURVES (Row coordinate connections for each column) ---
                for (int c = 0; c < series.Columns; c++)
                {
                    var geom = new StreamGeometry();
                    using (var ctx = geom.Open())
                    {
                        bool isFirst = true;
                        for (int r = 0; r < series.Rows; r++)
                        {
                            int flatIdx = r * series.Columns + c;
                            if (flatIdx >= series.XValues.Length || flatIdx >= series.YValues.Length) break;

                            double xVal = series.XValues[flatIdx];
                            double yVal = series.YValues[flatIdx];

                            // Translate coordinates
                            double px = area.Left + (xVal - minX) / xRange * area.Width;
                            double py = area.Bottom - (yVal - minY) / yRange * area.Height;

                            double centerY = area.Top + area.Height / 2.0;
                            py = centerY + (py - centerY) * AnimationProgress;

                            var pt = new Point(px, py);
                            if (isFirst)
                            {
                                ctx.BeginFigure(pt, false);
                                isFirst = false;
                            }
                            else
                            {
                                ctx.LineTo(pt);
                            }
                        }
                        ctx.EndFigure(false);
                    }
                    context.DrawGeometry(Brushes.Transparent, strokePen, geom);
                }
            }

            // Draw interactive highlighted intersection marker
            if (_hoverPoint.HasValue && _hoverSeries != null)
            {
                var markerBrush = _hoverSeries.Fill ?? activePalette.GetBrush(Series.IndexOf(_hoverSeries));
                var strokePen = new Pen(Brushes.White, 1.5);
                context.DrawEllipse(markerBrush, strokePen, _hoverPoint.Value, 5.0, 5.0);
            }
        }

        private void DrawAxesGuides(DrawingContext context, double minX, double maxX, double minY, double maxY)
        {
            var area = EffectivePlotArea;
            var textBrush = LabelForeground ?? SystemBrush;
            var fontLabel = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var linePen = new Pen(new SolidColorBrush(Color.Parse("#12FFFFFF")), 1.0);

            // Draw X coordinates along bottom boundary
            int intervals = 5;
            for (int i = 0; i <= intervals; i++)
            {
                double ratio = (double)i / intervals;
                double val = minX + ratio * (maxX - minX);
                double px = area.Left + ratio * area.Width;

                // Draw thin grid lines
                context.DrawLine(linePen, new Point(px, area.Top), new Point(px, area.Bottom));

                var ft = new FormattedText(val.ToString("N1"), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontLabel, 9, textBrush);
                context.DrawText(ft, new Point(px - ft.Width / 2.0, area.Bottom + 6));
            }

            // Draw Y coordinates along left boundary
            for (int i = 0; i <= intervals; i++)
            {
                double ratio = (double)i / intervals;
                double val = minY + ratio * (maxY - minY);
                double py = area.Bottom - ratio * area.Height;

                context.DrawLine(linePen, new Point(area.Left, py), new Point(area.Right, py));

                var ft = new FormattedText(val.ToString("N1"), System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontLabel, 9, textBrush);
                context.DrawText(ft, new Point(area.Left - ft.Width - 8, py - ft.Height / 2.0));
            }
        }

        private void RenderEmptyState(DrawingContext context)
        {
            var ft = new FormattedText(
                "Configure Carpet Matrix Grid Values",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Italic, FontWeight.SemiBold),
                13,
                SystemBrush);

            double tx = EffectivePlotArea.Left + (EffectivePlotArea.Width - ft.Width) / 2.0;
            double ty = EffectivePlotArea.Top + (EffectivePlotArea.Height - ft.Height) / 2.0;
            context.DrawText(ft, new Point(tx, ty));
        }

        /// <inheritdoc />
        protected override void UpdateHoverState(Point? mousePoint)
        {
            var visibleSeries = Series.Where(s => s.IsVisible).ToList();
            if (visibleSeries.Count == 0 || !mousePoint.HasValue)
            {
                ClearHover();
                return;
            }

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (var series in visibleSeries)
            {
                if (series.XValues == null || series.YValues == null) continue;
                for (int idx = 0; idx < series.XValues.Length; idx++)
                {
                    if (idx >= series.YValues.Length) break;
                    double xVal = series.XValues[idx];
                    double yVal = series.YValues[idx];

                    if (xVal < minX) minX = xVal;
                    if (xVal > maxX) maxX = xVal;
                    if (yVal < minY) minY = yVal;
                    if (yVal > maxY) maxY = yVal;
                }
            }

            if (minX == double.MaxValue || maxX == double.MinValue || minY == double.MaxValue || maxY == double.MinValue)
            {
                ClearHover();
                return;
            }

            double xRange = maxX - minX;
            double yRange = maxY - minY;
            var area = EffectivePlotArea;

            // Find nearest grid intersection point
            double threshold = 20.0;
            double nearestDist = double.MaxValue;
            Point? bestPoint = null;
            int bestRow = -1;
            int bestCol = -1;
            CarpetSeries? bestSeries = null;

            foreach (var series in visibleSeries)
            {
                if (series.XValues == null || series.YValues == null || series.Rows <= 0 || series.Columns <= 0) continue;

                for (int r = 0; r < series.Rows; r++)
                {
                    for (int c = 0; c < series.Columns; c++)
                    {
                        int flatIdx = r * series.Columns + c;
                        if (flatIdx >= series.XValues.Length || flatIdx >= series.YValues.Length) break;

                        double xVal = series.XValues[flatIdx];
                        double yVal = series.YValues[flatIdx];

                        double px = area.Left + (xVal - minX) / xRange * area.Width;
                        double py = area.Bottom - (yVal - minY) / yRange * area.Height;

                        double dist = Math.Sqrt((px - mousePoint.Value.X) * (px - mousePoint.Value.X) + (py - mousePoint.Value.Y) * (py - mousePoint.Value.Y));
                        if (dist < nearestDist && dist <= threshold)
                        {
                            nearestDist = dist;
                            bestPoint = new Point(px, py);
                            bestRow = r;
                            bestCol = c;
                            bestSeries = series;
                        }
                    }
                }
            }

            _hoverPoint = bestPoint;
            _hoverRow = bestRow;
            _hoverCol = bestCol;
            _hoverSeries = bestSeries;
        }

        private void ClearHover()
        {
            _hoverPoint = null;
            _hoverRow = -1;
            _hoverCol = -1;
            _hoverSeries = null;
        }

        /// <inheritdoc />
        protected override void DrawTooltip(DrawingContext context, Point mousePoint)
        {
            if (!_hoverPoint.HasValue || _hoverSeries == null || _hoverRow < 0 || _hoverCol < 0) return;

            int flatIdx = _hoverRow * _hoverSeries.Columns + _hoverCol;
            if (flatIdx >= _hoverSeries.XValues.Length || flatIdx >= _hoverSeries.YValues.Length) return;

            double xVal = _hoverSeries.XValues[flatIdx];
            double yVal = _hoverSeries.YValues[flatIdx];

            double padding = 10;
            double rowHeight = 16;
            double tooltipWidth = 160;
            double tooltipHeight = padding * 2 + rowHeight * 3;

            var fontTitle = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            var fontText = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var textBrush = Brushes.White;

            var ftTitle = new FormattedText(
                string.IsNullOrEmpty(_hoverSeries.Title) ? "Carpet Point" : _hoverSeries.Title,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                fontTitle,
                11,
                textBrush);

            var ftIndices = new FormattedText($"Grid: (Row {_hoverRow}, Col {_hoverCol})", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontText, 10, textBrush);
            var ftCoords = new FormattedText($"X: {xVal:N2}, Y: {yVal:N2}", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontText, 10, textBrush);

            tooltipWidth = Math.Max(tooltipWidth, Math.Max(ftTitle.Width, Math.Max(ftIndices.Width, ftCoords.Width)) + padding * 2 + 10);

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

            context.DrawText(ftTitle, new Point(tx + padding, ty + padding));
            context.DrawText(ftIndices, new Point(tx + padding, ty + padding + rowHeight));
            context.DrawText(ftCoords, new Point(tx + padding, ty + padding + rowHeight * 2));
        }

        /// <inheritdoc />
        public override System.Collections.Generic.IEnumerable<ChartSeries> GetSeries() => Series;
    }
}
