using System;
using System.Collections;
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
    public partial class KpiCard : ChartBase
    {
        public static readonly DependencyProperty ValueStringProperty =
            DependencyProperty.Register(nameof(ValueString), typeof(string), typeof(KpiCard), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty TrendValueProperty =
            DependencyProperty.Register(nameof(TrendValue), typeof(double), typeof(KpiCard), new PropertyMetadata(0.0, OnPropertyChanged));

        public static readonly DependencyProperty TrendUnitProperty =
            DependencyProperty.Register(nameof(TrendUnit), typeof(string), typeof(KpiCard), new PropertyMetadata("%", OnPropertyChanged));

        public static readonly DependencyProperty SparklineDataProperty =
            DependencyProperty.Register(nameof(SparklineData), typeof(IEnumerable), typeof(KpiCard), new PropertyMetadata(default(IEnumerable?), OnPropertyChanged));

        public string? ValueString { get => (string?)GetValue(ValueStringProperty);
            set => SetValue(ValueStringProperty, value);
        }

        public double TrendValue { get => (double)GetValue(TrendValueProperty);
            set => SetValue(TrendValueProperty, value);
        }

        public string? TrendUnit { get => (string?)GetValue(TrendUnitProperty);
            set => SetValue(TrendUnitProperty, value);
        }

        public IEnumerable? SparklineData { get => (IEnumerable?)GetValue(SparklineDataProperty);
            set => SetValue(SparklineDataProperty, value);
        }

        public KpiCard()
        {
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            // Give plenty of space inside the card
            double padding = 16;
            return new Rect(padding, padding, bounds.Width - padding * 2, bounds.Height - padding * 2);
        }

        protected override void RenderChart(SKCanvas context)
        {
            var area = EffectivePlotArea;

            // 1. Draw premium glassmorphic background
            var bgSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#1E293B")); // slate-800
            var borderSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#30FFFFFF")), 1.0);
            context.DrawRectangle(bgSKPaint, borderSKPaint, new RoundedRect(Bounds, new CornerRadius(12.0)));

            // 2. Draw Title
            var titleSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#94A3B8")); // slate-400
            var ftTitle = new FormattedText(
                Title ?? "KPI Indicator",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.SemiBold),
                12,
                titleSKPaint);
            context.DrawText(ftTitle, area.Position);

            // 3. Draw Main Value
            var valSKPaint = SKPaintes.White;
            var ftVal = new FormattedText(
                ValueString ?? "0.0",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                28,
                valSKPaint);
            context.DrawText(ftVal, new Point(area.Left, area.Top + 20));

            // 4. Draw Trend arrow and pill badge
            double trend = TrendValue;
            bool isPositive = trend >= 0;
            var badgeBg = isPositive ? new SolidSKColorSKPaint(SKColor.Parse("#2010B981")) : new SolidSKColorSKPaint(SKColor.Parse("#20EF4444"));
            var badgeBorder = isPositive ? new SolidSKColorSKPaint(SKColor.Parse("#8010B981")) : new SolidSKColorSKPaint(SKColor.Parse("#80EF4444"));
            var badgeTextSKPaint = isPositive ? new SolidSKColorSKPaint(SKColor.Parse("#10B981")) : new SolidSKColorSKPaint(SKColor.Parse("#EF4444"));

            string trendSign = isPositive ? "▲" : "▼";
            string trendText = $"{trendSign} {Math.Abs(trend):F1}{TrendUnit}";

            var ftTrend = new FormattedText(
                trendText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                10,
                badgeTextSKPaint);

            double badgeX = area.Right - ftTrend.Width - 12.0;
            double badgeY = area.Top;
            var badgeRect = new Rect(badgeX - 6.0, badgeY, ftTrend.Width + 12.0, ftTrend.Height + 4.0);

            context.DrawRectangle(badgeBg, new Pen(badgeBorder, 1.0), new RoundedRect(badgeRect, new CornerRadius(6.0)));
            context.DrawText(ftTrend, new Point(badgeX, badgeY + 2.0));

            // 5. Draw inline sparkline at the bottom
            if (SparklineData != null)
            {
                var rawPoints = new List<double>();
                foreach (var item in SparklineData)
                {
                    if (item == null) continue;
                    try
                    {
                        rawPoints.Add(Convert.ToDouble(item, System.Globalization.CultureInfo.InvariantCulture));
                    }
                    catch { }
                }

                if (rawPoints.Count >= 2)
                {
                    double sparkHeight = Math.Max(20.0, area.Height * 0.35);
                    double sparkY = area.Bottom - sparkHeight;
                    double sparkW = area.Width;
                    
                    double min = rawPoints.Min();
                    double max = rawPoints.Max();
                    if (Math.Abs(max - min) < 1e-9) max = min + 1.0;

                    double progress = AnimationProgress;
                    var sparkStroke = isPositive ? new SolidSKColorSKPaint(SKColor.Parse("#10B981")) : new SolidSKColorSKPaint(SKColor.Parse("#EF4444"));
                    var sparkSKPaint = new Pen(sparkStroke, 2.0, lineCap: SKStrokeCap.Round, lineJoin: SKStrokeJoin.Round);

                    // Build points
                    var pts = new List<Point>();
                    double step = sparkW / (rawPoints.Count - 1);
                    for (int i = 0; i < rawPoints.Count; i++)
                    {
                        double px = area.Left + i * step;
                        double pctY = (rawPoints[i] - min) / (max - min);
                        double py = (sparkY + sparkHeight) - (sparkHeight * pctY * progress);
                        pts.Add(new Point(px, py));
                    }

                    // Render Sparkline
                    var geometry = new SKPath();
                    using (var ctx = geometry.Open())
                    {
                        geometry.MoveTo(pts[0], false);
                        for (int i = 1; i < pts.Count; i++)
                        {
                            geometry.LineTo(pts[i]);
                        }
                    }
                    context.DrawGeometry(null, sparkSKPaint, geometry);

                    // Render semitransparent gradient area below sparkline
                    var areaGeom = new SKPath();
                    using (var ctx = areaGeom.Open())
                    {
                        areaGeom.MoveTo(new Point(pts[0].X, area.Bottom), true);
                        foreach (var pt in pts)
                        {
                            areaGeom.LineTo(pt);
                        }
                        areaGeom.LineTo(new Point(pts.Last().X, area.Bottom));
                    }

                    SKColor sparkSKColor = isPositive ? SKColor.Parse("#10B981") : SKColor.Parse("#EF4444");
                    var fillGradient = new LinearGradientSKPaint
                    {
                        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                        GradientStops =
                        {
                            new GradientStop(new SKColor((byte)(sparkSKColor.Red), (byte)(sparkSKColor.Green), (byte)(sparkSKColor.Blue), (byte)(40)), 0.0),
                            new GradientStop(new SKColor((byte)(sparkSKColor.Red), (byte)(sparkSKColor.Green), (byte)(sparkSKColor.Blue), (byte)(0)), 1.0)
                        }
                    };
                    context.DrawGeometry(fillGradient, null, areaGeom);
                }
            }
        }
    }
}