using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class KpiCard : ChartBase
    {
        public static readonly StyledProperty<string?> ValueStringProperty =
            AvaloniaProperty.Register<KpiCard, string?>(nameof(ValueString));

        public static readonly StyledProperty<double> TrendValueProperty =
            AvaloniaProperty.Register<KpiCard, double>(nameof(TrendValue), 0.0);

        public static readonly StyledProperty<string?> TrendUnitProperty =
            AvaloniaProperty.Register<KpiCard, string?>(nameof(TrendUnit), "%");

        public static readonly StyledProperty<IEnumerable?> SparklineDataProperty =
            AvaloniaProperty.Register<KpiCard, IEnumerable?>(nameof(SparklineData));

        public string? ValueString
        {
            get => GetValue(ValueStringProperty);
            set => SetValue(ValueStringProperty, value);
        }

        public double TrendValue
        {
            get => GetValue(TrendValueProperty);
            set => SetValue(TrendValueProperty, value);
        }

        public string? TrendUnit
        {
            get => GetValue(TrendUnitProperty);
            set => SetValue(TrendUnitProperty, value);
        }

        public IEnumerable? SparklineData
        {
            get => GetValue(SparklineDataProperty);
            set => SetValue(SparklineDataProperty, value);
        }

        public KpiCard()
        {
            ValueStringProperty.Changed.AddClassHandler<KpiCard>((x, e) => x.InvalidateVisual());
            TrendValueProperty.Changed.AddClassHandler<KpiCard>((x, e) => x.InvalidateVisual());
            TrendUnitProperty.Changed.AddClassHandler<KpiCard>((x, e) => x.InvalidateVisual());
            SparklineDataProperty.Changed.AddClassHandler<KpiCard>((x, e) => x.InvalidateVisual());
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            // Give plenty of space inside the card
            double padding = 16;
            return new Rect(padding, padding, bounds.Width - padding * 2, bounds.Height - padding * 2);
        }

        protected override void RenderChart(DrawingContext context)
        {
            var area = EffectivePlotArea;

            // 1. Draw premium glassmorphic background
            var bgBrush = new SolidColorBrush(Color.Parse("#1E293B")); // slate-800
            var borderPen = new Pen(new SolidColorBrush(Color.Parse("#30FFFFFF")), 1.0);
            context.DrawRectangle(bgBrush, borderPen, new RoundedRect(Bounds, new CornerRadius(12.0)));

            // 2. Draw Title
            var titleBrush = new SolidColorBrush(Color.Parse("#94A3B8")); // slate-400
            var ftTitle = new FormattedText(
                Title ?? "KPI Indicator",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.SemiBold),
                12,
                titleBrush);
            context.DrawText(ftTitle, area.Position);

            // 3. Draw Main Value
            var valBrush = Brushes.White;
            var ftVal = new FormattedText(
                ValueString ?? "0.0",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                28,
                valBrush);
            context.DrawText(ftVal, new Point(area.Left, area.Top + 20));

            // 4. Draw Trend arrow and pill badge
            double trend = TrendValue;
            bool isPositive = trend >= 0;
            var badgeBg = isPositive ? new SolidColorBrush(Color.Parse("#2010B981")) : new SolidColorBrush(Color.Parse("#20EF4444"));
            var badgeBorder = isPositive ? new SolidColorBrush(Color.Parse("#8010B981")) : new SolidColorBrush(Color.Parse("#80EF4444"));
            var badgeTextBrush = isPositive ? new SolidColorBrush(Color.Parse("#10B981")) : new SolidColorBrush(Color.Parse("#EF4444"));

            string trendSign = isPositive ? "▲" : "▼";
            string trendText = $"{trendSign} {Math.Abs(trend):F1}{TrendUnit}";

            var ftTrend = new FormattedText(
                trendText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                10,
                badgeTextBrush);

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
                    var sparkStroke = isPositive ? new SolidColorBrush(Color.Parse("#10B981")) : new SolidColorBrush(Color.Parse("#EF4444"));
                    var sparkPen = new Pen(sparkStroke, 2.0, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

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
                    var geom = new StreamGeometry();
                    using (var ctx = geom.Open())
                    {
                        ctx.BeginFigure(pts[0], false);
                        for (int i = 1; i < pts.Count; i++)
                        {
                            ctx.LineTo(pts[i]);
                        }
                    }
                    context.DrawGeometry(null, sparkPen, geom);

                    // Render semitransparent gradient area below sparkline
                    var areaGeom = new StreamGeometry();
                    using (var ctx = areaGeom.Open())
                    {
                        ctx.BeginFigure(new Point(pts[0].X, area.Bottom), true);
                        foreach (var pt in pts)
                        {
                            ctx.LineTo(pt);
                        }
                        ctx.LineTo(new Point(pts.Last().X, area.Bottom));
                    }

                    Color sparkColor = isPositive ? Color.Parse("#10B981") : Color.Parse("#EF4444");
                    var fillGradient = new LinearGradientBrush
                    {
                        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                        GradientStops =
                        {
                            new GradientStop(Color.FromArgb(40, sparkColor.R, sparkColor.G, sparkColor.B), 0.0),
                            new GradientStop(Color.FromArgb(0, sparkColor.R, sparkColor.G, sparkColor.B), 1.0)
                        }
                    };
                    context.DrawGeometry(fillGradient, null, areaGeom);
                }
            }
        }
    }
}
