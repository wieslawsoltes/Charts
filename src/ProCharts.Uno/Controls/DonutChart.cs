using System;
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
    public partial class DonutChart : PieChart
    {
        public static readonly DependencyProperty CenterTextProperty =
            DependencyProperty.Register(nameof(CenterText), typeof(string), typeof(DonutChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty CenterSubTextProperty =
            DependencyProperty.Register(nameof(CenterSubText), typeof(string), typeof(DonutChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public string? CenterText { get => (string?)GetValue(CenterTextProperty);
            set => SetValue(CenterTextProperty, value);
        }

        public string? CenterSubText { get => (string?)GetValue(CenterSubTextProperty);
            set => SetValue(CenterSubTextProperty, value);
        }

        public DonutChart()
        {
            HollowRadius = 0.65; // Set beautiful default donut inner radius
        }

        protected override void RenderChart(SKCanvas context)
        {
            base.RenderChart(context);

            if (Series.Count == 0) return;

            var area = EffectivePlotArea;
            var center = area.Center;

            // Render Center Digital Metric
            double totalValue = Series.Where(s => s.IsVisible).Sum(s => s.Value);
            string mainText = CenterText ?? totalValue.ToString("N0");
            string subText = CenterSubText ?? "TOTAL";

            var textSKPaint = SystemSKPaint;
            
            // Draw Center Main Value
            var ftMain = new FormattedText(
                mainText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                Math.Clamp(area.Width * 0.12, 16, 40),
                textSKPaint);

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
                    new SolidSKColorSKPaint(SKColor.Parse("#94A3B8"))); // cool gray-400

                double sx = center.X - ftSub.Width / 2.0;
                double sy = my + ftMain.Height + 2;
                context.DrawText(ftSub, new Point(sx, sy));
            }
        }
    }
}