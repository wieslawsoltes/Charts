using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public partial class BulletChart : ChartBase
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(BulletChart), new PropertyMetadata(0.0, OnPropertyChanged));

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(nameof(Target), typeof(double), typeof(BulletChart), new PropertyMetadata(0.0, OnPropertyChanged));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(BulletChart), new PropertyMetadata(0.0, OnPropertyChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(BulletChart), new PropertyMetadata(100.0, OnPropertyChanged));

        public static readonly DependencyProperty BadRangeProperty =
            DependencyProperty.Register(nameof(BadRange), typeof(double), typeof(BulletChart), new PropertyMetadata(40.0, OnPropertyChanged));

        public static readonly DependencyProperty SatisfactoryRangeProperty =
            DependencyProperty.Register(nameof(SatisfactoryRange), typeof(double), typeof(BulletChart), new PropertyMetadata(70.0, OnPropertyChanged));

        public double Value { get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double Target { get => (double)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        public double Minimum { get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double Maximum { get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public double BadRange { get => (double)GetValue(BadRangeProperty);
            set => SetValue(BadRangeProperty, value);
        }

        public double SatisfactoryRange { get => (double)GetValue(SatisfactoryRangeProperty);
            set => SetValue(SatisfactoryRangeProperty, value);
        }

        public BulletChart()
        {
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double top = 20;
            double bottom = 20;
            double left = 20;
            double right = 20;

            if (!string.IsNullOrEmpty(Title)) top += 24;

            double w = Math.Max(10, bounds.Width - left - right);
            double h = Math.Max(10, bounds.Height - top - bottom);

            return new Rect(left, top, w, h);
        }

        protected override void RenderChart(SKCanvas context)
        {
            var area = EffectivePlotArea;

            double min = Minimum;
            double max = Maximum;
            if (max <= min) max = min + 1.0;

            double progress = AnimationProgress;
            double targetVal = min + (Value - min) * progress;
            targetVal = Math.Clamp(targetVal, min, max);

            double pctValue = (targetVal - min) / (max - min);
            double pctTarget = (Target - min) / (max - min);

            double barHeight = Math.Clamp(area.Height * 0.5, 12.0, 48.0);
            double by = area.Top + (area.Height - barHeight) / 2.0;

            // 1. Draw Qualitative Range Bands (Background Layers)
            // Bad Range (Darkest)
            double badPct = Math.Clamp((BadRange - min) / (max - min), 0.0, 1.0);
            var badSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#1A202C")); // slate-900 / dark
            var badRect = new Rect(area.Left, by, area.Width * badPct, barHeight);
            context.DrawRectangle(badSKPaint, null, badRect);

            // Satisfactory Range (Medium Dark)
            double satPct = Math.Clamp((SatisfactoryRange - min) / (max - min), 0.0, 1.0);
            var satSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#2D3748")); // slate-700
            double satW = Math.Max(0, area.Width * (satPct - badPct));
            var satRect = new Rect(area.Left + area.Width * badPct, by, satW, barHeight);
            context.DrawRectangle(satSKPaint, null, satRect);

            // Good Range (Lightest Background)
            var goodSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#4A5568")); // slate-600
            double goodW = Math.Max(0, area.Width * (1.0 - satPct));
            var goodRect = new Rect(area.Left + area.Width * satPct, by, goodW, barHeight);
            context.DrawRectangle(goodSKPaint, null, goodRect);

            // 2. Draw Actual Value Bar (Thick central bar inside bands)
            double actualHeight = barHeight * 0.35;
            double ay = by + (barHeight - actualHeight) / 2.0;
            var actualSKPaint = Palette?.GetSKPaint(0) ?? new SolidSKColorSKPaint(SKColor.Parse("#06B6D4")); // Cyan actual

            if (pctValue > 0.001)
            {
                var valRect = new Rect(area.Left, ay, area.Width * pctValue, actualHeight);
                context.DrawRectangle(actualSKPaint, null, valRect);
            }

            // 3. Draw Target Marker (Distinct vertical line crossing bands)
            if (pctTarget >= 0.0 && pctTarget <= 1.0)
            {
                double tx = area.Left + area.Width * pctTarget;
                var targetSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#E2E8F0")), 3.0); // Thick Slate White Line
                double markerMargin = 3.0;
                context.DrawLine(targetSKPaint, new Point(tx, by - markerMargin), new Point(tx, by + barHeight + markerMargin));
            }

            // 4. Draw Digital Readouts
            var textSKPaint = SystemSKPaint;
            var ft = new FormattedText(
                $"Actual: {targetVal:F1}  |  Target: {Target:F1}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                11,
                textSKPaint);

            double vx = area.Left + (area.Width - ft.Width) / 2.0;
            double vy = by + barHeight + 6.0;
            context.DrawText(ft, new Point(vx, vy));
        }
    }
}