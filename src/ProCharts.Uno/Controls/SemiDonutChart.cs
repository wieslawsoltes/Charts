using System;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Series;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public partial class SemiDonutChart : PieChart
    {
        public static readonly DependencyProperty CenterTextProperty =
            DependencyProperty.Register(nameof(CenterText), typeof(string), typeof(SemiDonutChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public string? CenterText { get => (string?)GetValue(CenterTextProperty);
            set => SetValue(CenterTextProperty, value);
        }

        public SemiDonutChart()
        {
            StartAngle = 180.0;
            TotalAngle = 180.0;
            HollowRadius = 0.65;

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

        protected override void RenderChart(DrawingContext context)
        {
            base.RenderChart(context);

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