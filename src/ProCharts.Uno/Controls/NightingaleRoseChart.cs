using System;
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
    /// Represents a Nightingale Rose polar area chart where each segment has an equal sweep angle but varying radius.
    /// </summary>
    public partial class NightingaleRoseChart : ChartBase
    {
        // --- DEPENDENCY PROPERTIES ---

        /// <summary>
        /// Defines the StartAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(NightingaleRoseChart), new PropertyMetadata(0.0, OnPropertyChanged));

        /// <summary>
        /// Defines the TotalAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty TotalAngleProperty =
            DependencyProperty.Register(nameof(TotalAngle), typeof(double), typeof(NightingaleRoseChart), new PropertyMetadata(360.0, OnPropertyChanged));

        /// <summary>
        /// Defines the HollowRadius dependency property. Mapped as a fraction of total outer radius.
        /// </summary>
        public static readonly DependencyProperty HollowRadiusProperty =
            DependencyProperty.Register(nameof(HollowRadius), typeof(double), typeof(NightingaleRoseChart), new PropertyMetadata(0.0, OnPropertyChanged));

        // --- PROPERTIES ---

        /// <summary>
        /// Gets or sets the starting angle of the first segment.
        /// </summary>
        public double StartAngle
        {
            get => (double)GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        /// <summary>
        /// Gets or sets the total angle budget of the chart (typically 360 degrees).
        /// </summary>
        public double TotalAngle
        {
            get => (double)GetValue(TotalAngleProperty);
            set => SetValue(TotalAngleProperty, value);
        }

        /// <summary>
        /// Gets or sets the hollow ratio factor for the center, between 0.0 and 0.95.
        /// </summary>
        public double HollowRadius
        {
            get => (double)GetValue(HollowRadiusProperty);
            set => SetValue(HollowRadiusProperty, value);
        }

        /// <summary>
        /// Gets the collection of rose series segments.
        /// </summary>
        public ObservableCollection<RoseSeries> Series { get; } = new ObservableCollection<RoseSeries>();

        private RoseSeries? _hoveredSeries;

        // --- CONSTRUCTOR ---

        /// <summary>
        /// Initializes a new instance of the NightingaleRoseChart class.
        /// </summary>
        public NightingaleRoseChart()
        {
            Series.CollectionChanged += OnSeriesCollectionChanged;
        }

        private void OnSeriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (RoseSeries oldSeries in e.OldItems)
                {
                    oldSeries.SeriesChanged -= OnSeriesChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (RoseSeries newSeries in e.NewItems)
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
        protected override Rect CalculatePlotArea(Size bounds)
        {
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
                cy += 14;
            }

            return new Rect(cx, cy, side, side);
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

            double maxValue = visibleSeries.Max(s => s.Value);
            if (maxValue <= 0)
            {
                RenderEmptyState(context);
                return;
            }

            var center = EffectivePlotArea.Center;
            double maxOuterRadius = EffectivePlotArea.Width / 2.0;
            double innerRadius = maxOuterRadius * Math.Clamp(HollowRadius, 0.0, 0.95);

            double angleStep = TotalAngle / visibleSeries.Count;
            double currentAngle = StartAngle;
            var activePalette = Palette ?? Palette.Default;

            for (int i = 0; i < visibleSeries.Count; i++)
            {
                var series = visibleSeries[i];
                double segmentRadius = innerRadius + (maxOuterRadius - innerRadius) * (series.Value / maxValue) * AnimationProgress;

                int colorIdx = Series.IndexOf(series);
                var defaultBrush = activePalette.GetBrush(colorIdx >= 0 ? colorIdx : i);
                var fillBrush = series.Fill ?? defaultBrush;

                if (series == _hoveredSeries)
                {
                    fillBrush = new SolidColorBrush(Color.Parse("#40FFFFFF"));
                }

                var strokeBrush = series.Stroke ?? Brushes.Transparent;
                var strokePen = new Pen(strokeBrush, series.StrokeThickness);

                // Draw polar ring sector using StreamGeometry
                var geom = new StreamGeometry();
                using (var ctx = geom.Open())
                {
                    double radStart = currentAngle * Math.PI / 180.0;
                    double radEnd = (currentAngle + angleStep) * Math.PI / 180.0;

                    Point pos = center + new Point(segmentRadius * Math.Cos(radStart), segmentRadius * Math.Sin(radStart));
                    Point poe = center + new Point(segmentRadius * Math.Cos(radEnd), segmentRadius * Math.Sin(radEnd));

                    bool isLargeArc = angleStep > 180.0;

                    if (innerRadius <= 0.0)
                    {
                        ctx.BeginFigure(center, true);
                        ctx.LineTo(pos);
                        ctx.ArcTo(poe, new Size(segmentRadius, segmentRadius), 0.0, isLargeArc, SweepDirection.Clockwise);
                        ctx.EndFigure(true);
                    }
                    else
                    {
                        Point pis = center + new Point(innerRadius * Math.Cos(radStart), innerRadius * Math.Sin(radStart));
                        Point pie = center + new Point(innerRadius * Math.Cos(radEnd), innerRadius * Math.Sin(radEnd));

                        ctx.BeginFigure(pis, true);
                        ctx.LineTo(pos);
                        ctx.ArcTo(poe, new Size(segmentRadius, segmentRadius), 0.0, isLargeArc, SweepDirection.Clockwise);
                        ctx.LineTo(pie);
                        ctx.ArcTo(pis, new Size(innerRadius, innerRadius), 0.0, isLargeArc, SweepDirection.CounterClockwise);
                        ctx.EndFigure(true);
                    }
                }

                context.DrawGeometry(fillBrush, strokePen, geom);
                currentAngle += angleStep;
            }
        }

        private void RenderEmptyState(DrawingContext context)
        {
            var textBrush = LabelForeground ?? SystemBrush;
            var ft = new FormattedText(
                "Configure Nightingale Slices",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Italic, FontWeight.SemiBold),
                14,
                textBrush);

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
                _hoveredSeries = null;
                return;
            }

            double maxValue = visibleSeries.Max(s => s.Value);
            if (maxValue <= 0)
            {
                _hoveredSeries = null;
                return;
            }

            var center = EffectivePlotArea.Center;
            double maxOuterRadius = EffectivePlotArea.Width / 2.0;
            double innerRadius = maxOuterRadius * Math.Clamp(HollowRadius, 0.0, 0.95);

            var diff = new Point(mousePoint.Value.X - center.X, mousePoint.Value.Y - center.Y);
            double dist = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);

            RoseSeries? currentHover = null;

            if (dist >= innerRadius && dist <= maxOuterRadius)
            {
                double angle = Math.Atan2(diff.Y, diff.X) * 180.0 / Math.PI;
                if (angle < 0) angle += 360.0;

                double adjustedAngle = (angle - StartAngle) % 360.0;
                if (adjustedAngle < 0) adjustedAngle += 360.0;

                double limitAngle = Math.Abs(TotalAngle);

                if (adjustedAngle <= limitAngle)
                {
                    double angleStep = limitAngle / visibleSeries.Count;
                    int index = (int)(adjustedAngle / angleStep);
                    if (index >= 0 && index < visibleSeries.Count)
                    {
                        var candidate = visibleSeries[index];
                        double segmentRadius = innerRadius + (maxOuterRadius - innerRadius) * (candidate.Value / maxValue);
                        if (dist <= segmentRadius)
                        {
                            currentHover = candidate;
                        }
                    }
                }
            }

            _hoveredSeries = currentHover;
        }

        /// <inheritdoc />
        protected override void DrawTooltip(DrawingContext context, Point mousePoint)
        {
            if (_hoveredSeries == null) return;

            var visibleSeries = Series.Where(s => s.IsVisible).ToList();
            int idx = Series.IndexOf(_hoveredSeries);
            var activePalette = Palette ?? Palette.Default;
            var seriesBrush = _hoveredSeries.Fill ?? activePalette.GetBrush(idx >= 0 ? idx : 0);

            double padding = 12;
            double titleHeight = 20;
            double rowHeight = 18;
            double tooltipWidth = 160;
            double tooltipHeight = padding * 2 + titleHeight + rowHeight;

            var titleFont = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            var textFont = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var textBrush = Brushes.White;

            var ftTitle = new FormattedText(
                string.IsNullOrEmpty(_hoveredSeries.Title) ? $"Series {idx + 1}" : _hoveredSeries.Title,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                titleFont,
                11,
                textBrush);

            var ftValue = new FormattedText(
                $"Value: {_hoveredSeries.Value:N2}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                textFont,
                11,
                textBrush);

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
            var bgBrush = new SolidColorBrush(Color.Parse("#E81F242E"));
            var borderPen = new Pen(new SolidColorBrush(Color.Parse("#30FFFFFF")), 1.0);

            context.DrawRectangle(bgBrush, borderPen, new RoundedRect(tooltipRect, new CornerRadius(6.0)));

            double curX = tx + padding;
            double curY = ty + padding;

            context.DrawText(ftTitle, new Point(curX, curY));
            curY += titleHeight;

            context.DrawEllipse(seriesBrush, null, new Point(curX + 4, curY + 6), 3.0, 3.0);
            context.DrawText(ftValue, new Point(curX + 14, curY));
        }

        /// <inheritdoc />
        public override System.Collections.Generic.IEnumerable<ChartSeries> GetSeries() => Series;
    }
}
