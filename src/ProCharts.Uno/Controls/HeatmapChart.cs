using System;
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
    public partial class HeatmapChart : ChartBase
    {
        public class HeatmapCell
        {
            public int X { get; set; }
            public int Y { get; set; }
            public double Value { get; set; }
        }

        public static readonly DependencyProperty CellsProperty =
            DependencyProperty.Register(nameof(Cells), typeof(IList<HeatmapCell>), typeof(HeatmapChart), new PropertyMetadata(default(IList<HeatmapCell>?), OnPropertyChanged));

        public static readonly DependencyProperty XLabelsProperty =
            DependencyProperty.Register(nameof(XLabels), typeof(IList<string>), typeof(HeatmapChart), new PropertyMetadata(default(IList<string>?), OnPropertyChanged));

        public static readonly DependencyProperty YLabelsProperty =
            DependencyProperty.Register(nameof(YLabels), typeof(IList<string>), typeof(HeatmapChart), new PropertyMetadata(default(IList<string>?), OnPropertyChanged));

        public IList<HeatmapCell>? Cells { get => (IList<HeatmapCell>?)GetValue(CellsProperty);
            set => SetValue(CellsProperty, value);
        }

        public IList<string>? XLabels { get => (IList<string>?)GetValue(XLabelsProperty);
            set => SetValue(XLabelsProperty, value);
        }

        public IList<string>? YLabels { get => (IList<string>?)GetValue(YLabelsProperty);
            set => SetValue(YLabelsProperty, value);
        }

        public HeatmapChart()
        {
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

        protected override void RenderChart(SKCanvas context)
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
            SKColor colLow = SKColor.Parse("#1E293B");
            SKColor colMid = SKColor.Parse("#06B6D4");
            SKColor colHigh = SKColor.Parse("#A855F7");

            var cellBorderSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#20FFFFFF")), 1.0);

            // Draw Cells
            foreach (var cell in Cells)
            {
                if (cell.X < 0 || cell.X >= numX || cell.Y < 0 || cell.Y >= numY) continue;

                double norm = (cell.Value - minVal) / (maxVal - minVal);
                norm = Math.Clamp(norm * progress, 0.0, 1.0);

                // Interpolate color along our gradient scale
                SKColor cellCol;
                if (norm < 0.5)
                {
                    double t = norm * 2.0;
                    cellCol = new SKColor((byte)((byte)(colLow.Red + (colMid.Red - colLow.Red) * t)), (byte)((byte)(colLow.Green + (colMid.Green - colLow.Green) * t)), (byte)((byte)(colLow.Blue + (colMid.Blue - colLow.Blue) * t)), (byte)((byte)(colLow.Alpha + (colMid.Alpha - colLow.Alpha) * t)));
                }
                else
                {
                    double t = (norm - 0.5) * 2.0;
                    cellCol = new SKColor((byte)((byte)(colMid.Red + (colHigh.Red - colMid.Red) * t)), (byte)((byte)(colMid.Green + (colHigh.Green - colMid.Green) * t)), (byte)((byte)(colMid.Blue + (colHigh.Blue - colMid.Blue) * t)), (byte)((byte)(colMid.Alpha + (colHigh.Alpha - colMid.Alpha) * t)));
                }

                var cellSKPaint = new SolidSKColorSKPaint(cellCol);

                // Screen coordinates: invert Y index so index 0 is at bottom (standard Cartesian)
                double cx = area.Left + cell.X * cellW;
                double cy = area.Bottom - (cell.Y + 1) * cellH;

                var cellRect = new Rect(cx, cy, cellW, cellH);
                context.DrawRectangle(cellSKPaint, cellBorderSKPaint, cellRect);
            }

            // Draw Grid Labels
            var labelFont = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.SemiBold);
            var labelSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#94A3B8"));

            // X Axis Labels
            for (int i = 0; i < numX; i++)
            {
                var ft = new FormattedText(
                    XLabels[i],
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    labelFont,
                    9,
                    labelSKPaint);
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
                    labelSKPaint);
                double lx = area.Left - ft.Width - 6.0;
                double ly = area.Bottom - (i + 1) * cellH + (cellH - ft.Height) / 2.0;
                context.DrawText(ft, new Point(lx, ly));
            }
        }

        private void RenderEmptyState(SKCanvas context)
        {
            var ft = new FormattedText(
                "Configure Heatmap Cells and Labels",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Italic, FontWeight.SemiBold),
                13,
                SystemSKPaint);

            double tx = EffectivePlotArea.Left + (EffectivePlotArea.Width - ft.Width) / 2.0;
            double ty = EffectivePlotArea.Top + (EffectivePlotArea.Height - ft.Height) / 2.0;
            context.DrawText(ft, new Point(tx, ty));
        }
    }
}