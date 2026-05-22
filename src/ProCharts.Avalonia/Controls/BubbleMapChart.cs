using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Avalonia.Components;
using ProCharts.Avalonia.Styles;

namespace ProCharts.Avalonia.Controls
{
    /// <summary>
    /// Represents a geospatial map overlay chart plotting bubble markers representing coordinate densities.
    /// </summary>
    public class BubbleMapChart : ChartBase
    {
        // --- GEOGRAPHIC CONTINENTAL BOUNDARIES (Latitude, Longitude) ---
        private static readonly double[][] ContinentPolygons = new double[][]
        {
            // North America
            new double[] {
                72.0, -168.0,  83.0, -74.0,  75.0, -60.0,  53.0, -55.0,  47.0, -65.0,
                25.0, -80.0,  15.0, -90.0,  7.0, -78.0,  16.0, -95.0,  30.0, -115.0,
                48.0, -125.0,  60.0, -145.0,  65.0, -168.0
            },
            // South America
            new double[] {
                12.0, -72.0,  6.0, -53.0,  -8.0, -35.0,  -23.0, -43.0,  -56.0, -67.0,
                -52.0, -75.0,  -18.0, -70.0,  -5.0, -81.0,  9.0, -79.0
            },
            // Eurasia
            new double[] {
                77.0, 10.0,  75.0, 60.0,  77.0, 104.0,  70.0, 170.0,  60.0, 166.0,
                35.0, 140.0,  22.0, 114.0,  8.0, 77.0,  25.0, 60.0,  12.0, 44.0,
                30.0, 32.0,  36.0, 6.0,  60.0, 5.0
            },
            // Africa
            new double[] {
                37.0, 11.0,  30.0, 32.0,  12.0, 43.0,  5.0, 50.0,  -34.0, 20.0,
                -30.0, 15.0,  -6.0, 12.0,  5.0, 9.0,  15.0, -17.0,  32.0, -9.0
            },
            // Australia
            new double[] {
                -11.0, 131.0,  -10.0, 142.0,  -22.0, 150.0,  -38.0, 145.0,  -35.0, 117.0,
                -21.0, 114.0,  -14.0, 125.0
            }
        };

        // --- DEPENDENCY PROPERTIES ---

        /// <summary>
        /// Defines the Bubbles dependency property.
        /// </summary>
        public static readonly StyledProperty<IList<MapBubbleItem>?> BubblesProperty =
            AvaloniaProperty.Register<BubbleMapChart, IList<MapBubbleItem>?>(nameof(Bubbles));

        // --- PROPERTIES ---

        /// <summary>
        /// Gets or sets the collection of plotted geospatial bubbles.
        /// </summary>
        public IList<MapBubbleItem>? Bubbles
        {
            get => GetValue(BubblesProperty);
            set => SetValue(BubblesProperty, value);
        }

        private MapBubbleItem? _hoveredItem;

        // --- CONSTRUCTOR ---

        /// <summary>
        /// Initializes a new instance of the BubbleMapChart class.
        /// </summary>
        public BubbleMapChart()
        {
            BubblesProperty.Changed.AddClassHandler<BubbleMapChart>((x, e) => x.InvalidateVisual());
        }

        /// <inheritdoc />
        protected override Rect CalculatePlotArea(Size bounds)
        {
            // Maps maintain standard 2:1 aspect ratio under Equirectangular projection
            double padding = 20;
            if (!string.IsNullOrEmpty(Title)) padding += 24;

            double width = bounds.Width - padding * 2;
            double height = width / 2.0;

            if (height > bounds.Height - padding * 2.0)
            {
                height = bounds.Height - padding * 2.0;
                width = height * 2.0;
            }

            width = Math.Max(10, width);
            height = Math.Max(10, height);

            double cx = (bounds.Width - width) / 2.0;
            double cy = (bounds.Height - height) / 2.0;
            if (!string.IsNullOrEmpty(Title)) cy += 12;

            return new Rect(cx, cy, width, height);
        }

        /// <inheritdoc />
        protected override void RenderChart(DrawingContext context)
        {
            var area = EffectivePlotArea;

            // 1. Draw Base Map Continents in Dark Mode aesthetic
            var landBrush = new SolidColorBrush(Color.Parse("#1A2436"));
            var oceanBrush = new SolidColorBrush(Color.Parse("#090D16"));
            var borderPen = new Pen(new SolidColorBrush(Color.Parse("#30FFFFFF")), 1.0);

            context.FillRectangle(oceanBrush, area);

            foreach (var poly in ContinentPolygons)
            {
                var geom = new StreamGeometry();
                using (var ctx = geom.Open())
                {
                    bool isFirst = true;
                    for (int i = 0; i < poly.Length; i += 2)
                    {
                        double lat = poly[i];
                        double lon = poly[i + 1];

                        // Project coordinate
                        double px = area.Left + (lon + 180.0) / 360.0 * area.Width;
                        double py = area.Top + (90.0 - lat) / 180.0 * area.Height;

                        if (isFirst)
                        {
                            ctx.BeginFigure(new Point(px, py), true);
                            isFirst = false;
                        }
                        else
                        {
                            ctx.LineTo(new Point(px, py));
                        }
                    }
                }
                context.DrawGeometry(landBrush, borderPen, geom);
            }

            // 2. Draw Bubbles
            if (Bubbles == null || Bubbles.Count == 0) return;

            double maxVal = Bubbles.Max(b => b.Value);
            if (maxVal <= 0) maxVal = 1.0;

            var activePalette = Palette ?? Palette.Default;

            for (int i = 0; i < Bubbles.Count; i++)
            {
                var bubble = Bubbles[i];
                if (bubble.Value <= 0) continue;

                // Project bubble center point
                double bx = area.Left + (bubble.Longitude + 180.0) / 360.0 * area.Width;
                double by = area.Top + (90.0 - bubble.Latitude) / 180.0 * area.Height;

                // Scaling factor for bubble radius (cap maximum diameter at 24px)
                double maxRadius = 16.0;
                double radius = 4.0 + (bubble.Value / maxVal) * (maxRadius - 4.0) * AnimationProgress;

                int colorIdx = Bubbles.IndexOf(bubble);
                var defaultBrush = activePalette.GetBrush(colorIdx >= 0 ? colorIdx : i);
                var fillBrush = bubble.Color ?? defaultBrush;

                // Overlay high opacity glowing center and transparent halo
                var haloColor = Color.FromArgb(70, ((SolidColorBrush)fillBrush).Color.R, ((SolidColorBrush)fillBrush).Color.G, ((SolidColorBrush)fillBrush).Color.B);
                var centerColor = Color.FromArgb(220, ((SolidColorBrush)fillBrush).Color.R, ((SolidColorBrush)fillBrush).Color.G, ((SolidColorBrush)fillBrush).Color.B);

                if (bubble == _hoveredItem)
                {
                    radius += 4.0; // Expand hovered item
                }

                context.DrawEllipse(new SolidColorBrush(haloColor), null, new Point(bx, by), radius, radius);
                context.DrawEllipse(new SolidColorBrush(centerColor), new Pen(Brushes.White, 1.0), new Point(bx, by), radius * 0.4, radius * 0.4);
            }
        }

        /// <inheritdoc />
        protected override void UpdateHoverState(Point? mousePoint)
        {
            if (Bubbles == null || Bubbles.Count == 0 || !mousePoint.HasValue)
            {
                _hoveredItem = null;
                return;
            }

            double maxVal = Bubbles.Max(b => b.Value);
            if (maxVal <= 0) maxVal = 1.0;

            var area = EffectivePlotArea;
            MapBubbleItem? bestHover = null;
            double nearestDist = double.MaxValue;

            for (int i = 0; i < Bubbles.Count; i++)
            {
                var bubble = Bubbles[i];
                double bx = area.Left + (bubble.Longitude + 180.0) / 360.0 * area.Width;
                double by = area.Top + (90.0 - bubble.Latitude) / 180.0 * area.Height;

                double maxRadius = 16.0;
                double radius = 4.0 + (bubble.Value / maxVal) * (maxRadius - 4.0);

                double dist = Math.Sqrt((bx - mousePoint.Value.X) * (bx - mousePoint.Value.X) + (by - mousePoint.Value.Y) * (by - mousePoint.Value.Y));
                if (dist <= radius + 6.0 && dist < nearestDist)
                {
                    nearestDist = dist;
                    bestHover = bubble;
                }
            }

            _hoveredItem = bestHover;
        }

        /// <inheritdoc />
        protected override void DrawTooltip(DrawingContext context, Point mousePoint)
        {
            if (_hoveredItem == null) return;

            double padding = 10;
            double rowHeight = 16;
            double tooltipWidth = 150;
            double tooltipHeight = padding * 2 + rowHeight * 3;

            var fontTitle = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            var fontText = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var textBrush = Brushes.White;

            var ftTitle = new FormattedText(_hoveredItem.Label, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontTitle, 11, textBrush);
            var ftCoords = new FormattedText($"Coords: {_hoveredItem.Latitude:F1}°N, {_hoveredItem.Longitude:F1}°E", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontText, 10, textBrush);
            var ftVal = new FormattedText($"Density: {_hoveredItem.Value:N1}", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontText, 10, textBrush);

            tooltipWidth = Math.Max(tooltipWidth, Math.Max(ftTitle.Width, Math.Max(ftCoords.Width, ftVal.Width)) + padding * 2 + 10);

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
            context.DrawText(ftCoords, new Point(tx + padding, ty + padding + rowHeight));
            context.DrawText(ftVal, new Point(tx + padding, ty + padding + rowHeight * 2));
        }
    }
}
