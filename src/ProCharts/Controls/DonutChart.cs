using System;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using ProCharts.Series;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class DonutChart : PieChart
    {
        public static readonly StyledProperty<string?> CenterTextProperty =
            AvaloniaProperty.Register<DonutChart, string?>(nameof(CenterText));

        public static readonly StyledProperty<string?> CenterSubTextProperty =
            AvaloniaProperty.Register<DonutChart, string?>(nameof(CenterSubText));

        public string? CenterText
        {
            get => GetValue(CenterTextProperty);
            set => SetValue(CenterTextProperty, value);
        }

        public string? CenterSubText
        {
            get => GetValue(CenterSubTextProperty);
            set => SetValue(CenterSubTextProperty, value);
        }

        public DonutChart()
        {
            HollowRadius = 0.65; // Set beautiful default donut inner radius
            CenterTextProperty.Changed.AddClassHandler<DonutChart>((x, e) => x.InvalidateVisual());
            CenterSubTextProperty.Changed.AddClassHandler<DonutChart>((x, e) => x.InvalidateVisual());
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (Series.Count == 0) return;

            var area = EffectivePlotArea;
            var center = area.Center;

            // Render Center Digital Metric
            double totalValue = Series.Where(s => s.IsVisible).Sum(s => s.Value);
            string mainText = CenterText ?? totalValue.ToString("N0");
            string subText = CenterSubText ?? "TOTAL";

            var textBrush = SystemBrush;
            
            // Draw Center Main Value
            var ftMain = new FormattedText(
                mainText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                Math.Clamp(area.Width * 0.12, 16, 40),
                textBrush);

            double mx = center.X - ftMain.Width / 2.0;
            double my = center.Y - ftMain.Height / 2.0;
            if (!string.IsNullOrEmpty(subText))
            {
                my -= 10;
            }

            context.DrawText(ftMain, new Point(mx, my));

            // Draw Center Subtext Label
            if (!string.IsNullOrEmpty(subText))
            {
                var ftSub = new FormattedText(
                    subText,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal),
                    Math.Clamp(area.Width * 0.05, 10, 14),
                    new SolidColorBrush(Color.Parse("#94A3B8"))); // cool gray-400

                double sx = center.X - ftSub.Width / 2.0;
                double sy = my + ftMain.Height + 2;
                context.DrawText(ftSub, new Point(sx, sy));
            }
        }
    }
}
