using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class HeatmapChart : ChartBase
    {
        public class HeatmapCell
        {
            public int X { get; set; }
            public int Y { get; set; }
            public double Value { get; set; }
        }

        public static readonly StyledProperty<IList<HeatmapCell>?> CellsProperty =
            AvaloniaProperty.Register<HeatmapChart, IList<HeatmapCell>?>(nameof(Cells));

        public static readonly StyledProperty<IList<string>?> XLabelsProperty =
            AvaloniaProperty.Register<HeatmapChart, IList<string>?>(nameof(XLabels));

        public static readonly StyledProperty<IList<string>?> YLabelsProperty =
            AvaloniaProperty.Register<HeatmapChart, IList<string>?>(nameof(YLabels));

        public IList<HeatmapCell>? Cells
        {
            get => GetValue(CellsProperty);
            set => SetValue(CellsProperty, value);
        }

        public IList<string>? XLabels
        {
            get => GetValue(XLabelsProperty);
            set => SetValue(XLabelsProperty, value);
        }

        public IList<string>? YLabels
        {
            get => GetValue(YLabelsProperty);
            set => SetValue(YLabelsProperty, value);
        }

        public HeatmapChart()
        {
            CellsProperty.Changed.AddClassHandler<HeatmapChart>((x, e) => x.InvalidateVisual());
            XLabelsProperty.Changed.AddClassHandler<HeatmapChart>((x, e) => x.InvalidateVisual());
            YLabelsProperty.Changed.AddClassHandler<HeatmapChart>((x, e) => x.InvalidateVisual());
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double top = 16;
            double bottom = 32;
            double left = 48;
            double right = 16;

            if (!string.IsNullOrEmpty(Title)) top += 24;

            double w = Math.Max(10, bounds.Width - left - right);
            double h = Math.Max(10, bounds.Height - top - bottom);

            return new Rect(left, top, w, h);
        }

        protected override void RenderChart(DrawingContext context)
        {
            if (Cells == null || Cells.Count == 0 || XLabels == null || XLabels.Count == 0 || YLabels == null || YLabels.Count == 0)
            {
                RenderEmptyState(context);
                return;
            }

            var area = EffectivePlotArea;
            int numX = XLabels.Count;
            int numY = YLabels.Count;

            double cellW = area.Width / numX;
            double cellH = area.Height / numY;

            double minVal = Cells.Min(c => c.Value);
            double maxVal = Cells.Max(c => c.Value);
            if (Math.Abs(maxVal - minVal) < 1e-9) maxVal = minVal + 1.0;

            double progress = AnimationProgress;

            // Sleek color scale: dark slate (0.0) -> cyan (0.5) -> purple (1.0)
            Color colLow = Color.Parse("#1E293B");
            Color colMid = Color.Parse("#06B6D4");
            Color colHigh = Color.Parse("#A855F7");

            var cellBorderPen = new Pen(new SolidColorBrush(Color.Parse("#20FFFFFF")), 1.0);

            // Draw Cells
            foreach (var cell in Cells)
            {
                if (cell.X < 0 || cell.X >= numX || cell.Y < 0 || cell.Y >= numY) continue;

                double norm = (cell.Value - minVal) / (maxVal - minVal);
                norm = Math.Clamp(norm * progress, 0.0, 1.0);

                // Interpolate color along our gradient scale
                Color cellCol;
                if (norm < 0.5)
                {
                    double t = norm * 2.0;
                    cellCol = Color.FromArgb(
                        (byte)(colLow.A + (colMid.A - colLow.A) * t),
                        (byte)(colLow.R + (colMid.R - colLow.R) * t),
                        (byte)(colLow.G + (colMid.G - colLow.G) * t),
                        (byte)(colLow.B + (colMid.B - colLow.B) * t));
                }
                else
                {
                    double t = (norm - 0.5) * 2.0;
                    cellCol = Color.FromArgb(
                        (byte)(colMid.A + (colHigh.A - colMid.A) * t),
                        (byte)(colMid.R + (colHigh.R - colMid.R) * t),
                        (byte)(colMid.G + (colHigh.G - colMid.G) * t),
                        (byte)(colMid.B + (colHigh.B - colMid.B) * t));
                }

                var cellBrush = new SolidColorBrush(cellCol);

                // Screen coordinates: invert Y index so index 0 is at bottom (standard Cartesian)
                double cx = area.Left + cell.X * cellW;
                double cy = area.Bottom - (cell.Y + 1) * cellH;

                var cellRect = new Rect(cx, cy, cellW, cellH);
                context.DrawRectangle(cellBrush, cellBorderPen, cellRect);
            }

            // Draw Grid Labels
            var labelFont = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.SemiBold);
            var labelBrush = new SolidColorBrush(Color.Parse("#94A3B8"));

            // X Axis Labels
            for (int i = 0; i < numX; i++)
            {
                var ft = new FormattedText(
                    XLabels[i],
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    labelFont,
                    9,
                    labelBrush);
                double lx = area.Left + i * cellW + (cellW - ft.Width) / 2.0;
                double ly = area.Bottom + 6.0;
                context.DrawText(ft, new Point(lx, ly));
            }

            // Y Axis Labels
            for (int i = 0; i < numY; i++)
            {
                var ft = new FormattedText(
                    YLabels[i],
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    labelFont,
                    9,
                    labelBrush);
                double lx = area.Left - ft.Width - 6.0;
                double ly = area.Bottom - (i + 1) * cellH + (cellH - ft.Height) / 2.0;
                context.DrawText(ft, new Point(lx, ly));
            }
        }

        private void RenderEmptyState(DrawingContext context)
        {
            var ft = new FormattedText(
                "Configure Heatmap Cells and Labels",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Italic, FontWeight.SemiBold),
                13,
                SystemBrush);

            double tx = EffectivePlotArea.Left + (EffectivePlotArea.Width - ft.Width) / 2.0;
            double ty = EffectivePlotArea.Top + (EffectivePlotArea.Height - ft.Height) / 2.0;
            context.DrawText(ft, new Point(tx, ty));
        }
    }
}
