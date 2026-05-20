using System;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Avalonia.Collections;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Series;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public partial class PieChart : ChartBase
    {
        // --- DEPENDENCY PROPERTIES ---

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(PieChart), new PropertyMetadata(0.0, OnPropertyChanged));

        public static readonly DependencyProperty TotalAngleProperty =
            DependencyProperty.Register(nameof(TotalAngle), typeof(double), typeof(PieChart), new PropertyMetadata(360.0, OnPropertyChanged));

        public static readonly DependencyProperty HollowRadiusProperty =
            DependencyProperty.Register(nameof(HollowRadius), typeof(double), typeof(PieChart), new PropertyMetadata(0.0, OnPropertyChanged)); // 0.0 for Pie, > 0.0 for Donut

        // --- PROPERTIES ---

        public double StartAngle { get => (double)GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        public double TotalAngle { get => (double)GetValue(TotalAngleProperty);
            set => SetValue(TotalAngleProperty, value);
        }

        public double HollowRadius { get => (double)GetValue(HollowRadiusProperty);
            set => SetValue(HollowRadiusProperty, value);
        }

        public AvaloniaList<PieSeries> Series { get; } = new AvaloniaList<PieSeries>();

        public PieChart()
        {
            Series.CollectionChanged += OnSeriesCollectionChanged;

            // Redraw when properties change
        }

        private void OnSeriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (PieSeries oldSeries in e.OldItems)
                {
                    oldSeries.SeriesChanged -= OnSeriesChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (PieSeries newSeries in e.NewItems)
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

        protected override Rect CalculatePlotArea(Size bounds)
        {
            // Pie charts should have a perfectly square aspect ratio for their plotting area
            double padding = 24;
            if (!string.IsNullOrEmpty(Title))
            {
                padding += 28;
            }

            double side = Math.Min(bounds.Width, bounds.Height) - padding * 2;
            side = Math.Max(10, side);

            double cx = (bounds.Width - side) / 2.0;
            double cy = (bounds.Height - side) / 2.0;

            if (!string.IsNullOrEmpty(Title))
            {
                cy += 14; // Push down slightly if there is a title
            }

            return new Rect(cx, cy, side, side);
        }

        protected override void RenderChart(SKCanvas context)
        {
            if (Series.Count == 0)
            {
                RenderEmptyState(context);
                return;
            }

            // Calculate total sum of slices
            double totalValue = Series.Where(s => s.IsVisible).Sum(s => s.Value);
            if (totalValue <= 0)
            {
                RenderEmptyState(context);
                return;
            }

            var center = EffectivePlotArea.Center;
            double outerRadius = EffectivePlotArea.Width / 2.0;
            double innerRadius = outerRadius * Math.Clamp(HollowRadius, 0.0, 0.95);

            double currentAngle = StartAngle;
            var activePalette = Palette ?? Palette.Default;

            for (int i = 0; i < Series.Count; i++)
            {
                var series = Series[i];
                if (!series.IsVisible) continue;

                double sliceSweep = TotalAngle * (series.Value / totalValue);
                var defaultSKPaint = activePalette.GetSKPaint(i);

                // Let the PieSeries render itself in circular coordinates
                series.RenderSlice(
                    context,
                    center,
                    outerRadius,
                    innerRadius,
                    currentAngle,
                    sliceSweep * AnimationProgress, // Scale sweep by animation progress
                    defaultSKPaint);

                currentAngle += sliceSweep;
            }
        }

        private void RenderEmptyState(SKCanvas context)
        {
            var textSKPaint = LabelForeground ?? SystemSKPaint;
            var ft = new FormattedText(
                "No Pie Slices Configured",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Italic, FontWeight.SemiBold),
                14,
                textSKPaint);

            double tx = EffectivePlotArea.Left + (EffectivePlotArea.Width - ft.Width) / 2.0;
            double ty = EffectivePlotArea.Top + (EffectivePlotArea.Height - ft.Height) / 2.0;
            context.DrawText(ft, new Point(tx, ty));
        }

        public override System.Collections.Generic.IEnumerable<ChartSeries> GetSeries() => Series;

        private PieSeries? _hoveredSeries;

        protected override void UpdateHoverState(Point? mousePoint)
        {
            if (Series.Count == 0 || !mousePoint.HasValue)
            {
                ClearHover();
                return;
            }

            double totalValue = Series.Where(s => s.IsVisible).Sum(s => s.Value);
            if (totalValue <= 0)
            {
                ClearHover();
                return;
            }

            var center = EffectivePlotArea.Center;
            double outerRadius = EffectivePlotArea.Width / 2.0;
            double innerRadius = outerRadius * Math.Clamp(HollowRadius, 0.0, 0.95);

            var diff = mousePoint.Value - center;
            double dist = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);

            PieSeries? currentHover = null;

            if (dist >= innerRadius && dist <= outerRadius)
            {
                double angle = Math.Atan2(diff.Y, diff.X) * 180.0 / Math.PI;
                if (angle < 0) angle += 360.0;

                double adjustedAngle = (angle - StartAngle) % 360.0;
                if (adjustedAngle < 0) adjustedAngle += 360.0;

                double limitAngle = Math.Abs(TotalAngle);

                if (adjustedAngle <= limitAngle)
                {
                    double currentAngle = 0.0;
                    var visibleSeries = Series.Where(s => s.IsVisible).ToList();

                    foreach (var series in visibleSeries)
                    {
                        double sweep = limitAngle * (series.Value / totalValue);
                        if (adjustedAngle >= currentAngle && adjustedAngle < currentAngle + sweep)
                        {
                            currentHover = series;
                            break;
                        }
                        currentAngle += sweep;
                    }
                }
            }

            if (currentHover != _hoveredSeries)
            {
                if (_hoveredSeries != null)
                {
                    _hoveredSeries.Exploded = false;
                }

                _hoveredSeries = currentHover;

                if (_hoveredSeries != null)
                {
                    _hoveredSeries.Exploded = true;
                }
            }
        }

        private void ClearHover()
        {
            if (_hoveredSeries != null)
            {
                _hoveredSeries.Exploded = false;
                _hoveredSeries = null;
            }
        }

        protected override void DrawTooltip(SKCanvas context, Point mousePoint)
        {
            if (_hoveredSeries == null) return;

            double totalValue = Series.Where(s => s.IsVisible).Sum(s => s.Value);
            if (totalValue <= 0) return;

            var activePalette = Palette ?? Palette.Default;
            int idx = Series.IndexOf(_hoveredSeries);
            var seriesSKPaint = _hoveredSeries.Fill ?? activePalette.GetSKPaint(idx >= 0 ? idx : 0);

            double percentage = (_hoveredSeries.Value / totalValue) * 100.0;

            double padding = 12;
            double titleHeight = 20;
            double rowHeight = 18;
            double tooltipWidth = 160;
            double tooltipHeight = padding * 2 + titleHeight + rowHeight;

            var titleFont = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            var textFont = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var textSKPaint = SKPaintes.White;

            var ftTitle = new FormattedText(
                string.IsNullOrEmpty(_hoveredSeries.Title) ? $"Series {idx + 1}" : _hoveredSeries.Title,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                titleFont,
                11,
                textSKPaint);

            var ftValue = new FormattedText(
                $"Value: {_hoveredSeries.Value:N2} ({percentage:F1}%)",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                textFont,
                11,
                textSKPaint);

            tooltipWidth = Math.Max(tooltipWidth, Math.Max(ftTitle.Width, ftValue.Width) + padding * 2 + 15);

            double tx = mousePoint.X + 15;
            double ty = mousePoint.Y + 15;

            if (tx + tooltipWidth > Bounds.Width)
            {
                tx = mousePoint.X - tooltipWidth - 15;
            }
            if (ty + tooltipHeight > Bounds.Height)
            {
                ty = mousePoint.Y - tooltipHeight - 15;
            }

            tx = Math.Max(0, tx);
            ty = Math.Max(0, ty);

            var tooltipRect = new Rect(tx, ty, tooltipWidth, tooltipHeight);
            var bgSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#E81F242E"));
            var borderSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#30FFFFFF")), 1.0);

            context.DrawRectangle(bgSKPaint, borderSKPaint, new RoundedRect(tooltipRect, new CornerRadius(6.0)));

            double curX = tx + padding;
            double curY = ty + padding;

            context.DrawText(ftTitle, new Point(curX, curY));
            curY += titleHeight;

            context.DrawEllipse(seriesSKPaint, null, new Point(curX + 4, curY + 6), 3.0, 3.0);
            context.DrawText(ftValue, new Point(curX + 14, curY));
        }
    }
}