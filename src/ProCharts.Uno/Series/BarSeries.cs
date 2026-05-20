using System;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Controls;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Series
{
    public class BarSeries : CartesianSeries
    {
        // --- DEPENDENCY PROPERTIES ---

        public static readonly DependencyProperty IsHorizontalProperty =
            DependencyProperty.Register(nameof(IsHorizontal), typeof(bool), typeof(BarSeries), new PropertyMetadata(false, OnPropertyChanged));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(BarSeries), new PropertyMetadata(4.0, OnPropertyChanged));

        // --- PROPERTIES ---

        public bool IsHorizontal { get => (bool)GetValue(IsHorizontalProperty);
            set => SetValue(IsHorizontalProperty, value);
        }

        public double CornerRadius { get => (double)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public BarSeries()
        {
        }

        public override void RenderSeries(SeriesRenderContext context)
        {
            var rawPoints = GetDataPoints();
            if (rawPoints.Count == 0) return;

            var parent = context.Chart as CartesianChart;
            if (parent == null) return;

            // Find peer BarSeries to calculate clustered bar layout
            var barSeriesList = parent.Series
                .OfType<BarSeries>()
                .Where(s => s.IsVisible && s.IsHorizontal == IsHorizontal)
                .ToList();

            int totalBarSeries = barSeriesList.Count;
            int barSeriesIndex = barSeriesList.IndexOf(this);
            if (barSeriesIndex < 0) barSeriesIndex = 0;

            var barFill = Fill ?? context.DefaultSKPaint;
            var barStroke = Stroke ?? barFill;
            var barSKPaint = new Pen(barStroke, StrokeThickness);

            double progress = context.AnimationProgress;

            if (!IsHorizontal)
            {
                // --- VERTICAL COLUMN CHART ---
                double slotWidth = context.PlotArea.Width / (context.Transform.XMax - context.Transform.XMin + 1.0);
                double spacing = 0.20; // 20% slot margins
                double usableWidth = slotWidth * (1.0 - spacing);
                double singleBarWidth = Math.Max(2.0, usableWidth / totalBarSeries);
                double leftOffset = -usableWidth / 2.0 + barSeriesIndex * singleBarWidth;

                double baselineY = context.Transform.YMin;
                if (baselineY < 0 && context.Transform.YMax > 0) baselineY = 0.0;

                foreach (var pt in rawPoints)
                {
                    if (double.IsNaN(pt.Y)) continue;

                    var centerPt = context.Transform.ToScreen(pt.X, 0.0);
                    double rectX = centerPt.X + leftOffset;

                    double screenBaselineY = context.Transform.ToScreen(pt.X, baselineY).Y;
                    double screenValueY = context.Transform.ToScreen(pt.X, pt.Y).Y;

                    // Animate heights
                    double animatedY = screenBaselineY + (screenValueY - screenBaselineY) * progress;

                    double rectY = Math.Min(screenBaselineY, animatedY);
                    double rectHeight = Math.Max(1.0, Math.Abs(screenBaselineY - animatedY));

                    var barRect = new Rect(rectX, rectY, singleBarWidth, rectHeight);

                    if (CornerRadius > 0)
                    {
                        context.Canvas.DrawRectangle(barFill, barSKPaint, new RoundedRect(barRect, new CornerRadius(CornerRadius)));
                    }
                    else
                    {
                        context.Canvas.DrawRectangle(barFill, barSKPaint, barRect);
                    }
                }
            }
            else
            {
                // --- HORIZONTAL BAR CHART ---
                double slotHeight = context.PlotArea.Height / (context.Transform.XMax - context.Transform.XMin + 1.0);
                double spacing = 0.20; // 20% slot margins
                double usableHeight = slotHeight * (1.0 - spacing);
                double singleBarHeight = Math.Max(2.0, usableHeight / totalBarSeries);
                
                // For vertical screen rendering, slot starts at top and goes down, so leftOffset is topOffset
                double topOffset = -usableHeight / 2.0 + barSeriesIndex * singleBarHeight;

                double baselineX = context.Transform.YMin; // Value axis is Y
                if (baselineX < 0 && context.Transform.YMax > 0) baselineX = 0.0;

                foreach (var pt in rawPoints)
                {
                    if (double.IsNaN(pt.Y)) continue;

                    // Project category (pt.X) on vertical screen coordinate (Y), and value (pt.Y) on horizontal coordinate (X)
                    var centerPt = context.Transform.ToScreen(baselineX, pt.X);
                    double rectY = centerPt.Y + topOffset;

                    double screenBaselineX = context.Transform.ToScreen(baselineX, pt.X).X;
                    double screenValueX = context.Transform.ToScreen(pt.Y, pt.X).X;

                    // Animate widths
                    double animatedX = screenBaselineX + (screenValueX - screenBaselineX) * progress;

                    double rectX = Math.Min(screenBaselineX, animatedX);
                    double rectWidth = Math.Max(1.0, Math.Abs(screenBaselineX - animatedX));

                    var barRect = new Rect(rectX, rectY, rectWidth, singleBarHeight);

                    if (CornerRadius > 0)
                    {
                        context.Canvas.DrawRectangle(barFill, barSKPaint, new RoundedRect(barRect, new CornerRadius(CornerRadius)));
                    }
                    else
                    {
                        context.Canvas.DrawRectangle(barFill, barSKPaint, barRect);
                    }
                }
            }
        }
    }
}