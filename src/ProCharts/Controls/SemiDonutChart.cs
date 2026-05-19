using System;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Series;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class SemiDonutChart : PieChart
    {
        public static readonly StyledProperty<string?> CenterTextProperty =
            AvaloniaProperty.Register<SemiDonutChart, string?>(nameof(CenterText));

        public string? CenterText
        {
            get => GetValue(CenterTextProperty);
            set => SetValue(CenterTextProperty, value);
        }

        public SemiDonutChart()
        {
            StartAngle = 180.0;
            TotalAngle = 180.0;
            HollowRadius = 0.65;

            CenterTextProperty.Changed.AddClassHandler<SemiDonutChart>((x, e) => x.InvalidateVisual());
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            // A semi-donut chart is wide and half as high (aspect 2:1)
            double padding = 24;
            if (!string.IsNullOrEmpty(Title))
            {
                padding += 28;
            }

            double usableWidth = bounds.Width - padding * 2;
            double usableHeight = bounds.Height - padding * 2;

            double radius = Math.Min(usableWidth / 2.0, usableHeight);
            radius = Math.Max(10, radius);

            double cx = (bounds.Width - radius * 2) / 2.0;
            double cy = padding;

            if (!string.IsNullOrEmpty(Title))
            {
                cy += 20;
            }

            // Return a boundary box representing the top half-circle
            return new Rect(cx, cy, radius * 2, radius * 2);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (Series.Count == 0) return;

            var area = EffectivePlotArea;
            // Center is at the bottom center of the half circle arc
            var center = new Point(area.Left + area.Width / 2.0, area.Top + area.Height / 2.0);

            double totalValue = Series.Where(s => s.IsVisible).Sum(s => s.Value);
            string mainText = CenterText ?? totalValue.ToString("N0");

            var textBrush = SystemBrush;
            var ftMain = new FormattedText(
                mainText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                Math.Clamp(area.Width * 0.10, 14, 32),
                textBrush);

            // Draw center text right above the baseline center
            double mx = center.X - ftMain.Width / 2.0;
            double my = center.Y - ftMain.Height - 4;

            context.DrawText(ftMain, new Point(mx, my));
        }
    }
}
