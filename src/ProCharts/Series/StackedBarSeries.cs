using System;
using System.Linq;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using ProCharts.Controls;
using ProCharts.Maths;

namespace ProCharts.Series
{
    public class StackedBarSeries : CartesianSeries
    {
        public static readonly StyledProperty<bool> IsHorizontalProperty =
            AvaloniaProperty.Register<StackedBarSeries, bool>(nameof(IsHorizontal), false);

        public static readonly StyledProperty<double> CornerRadiusProperty =
            AvaloniaProperty.Register<StackedBarSeries, double>(nameof(CornerRadius), 4.0);

        public bool IsHorizontal
        {
            get => GetValue(IsHorizontalProperty);
            set => SetValue(IsHorizontalProperty, value);
        }

        public double CornerRadius
        {
            get => GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public StackedBarSeries()
        {
            IsHorizontalProperty.Changed.AddClassHandler<StackedBarSeries>((x, e) => x.RaiseSeriesChanged());
            CornerRadiusProperty.Changed.AddClassHandler<StackedBarSeries>((x, e) => x.RaiseSeriesChanged());
        }

        public override void RenderSeries(in SeriesRenderContext context)
        {
            var rawPoints = GetDataPoints();
            if (rawPoints.Count == 0) return;

            var parent = context.Chart as CartesianChart;
            if (parent == null) return;

            // Find all peer StackedBarSeries
            var stackedSeriesList = parent.Series
                .OfType<StackedBarSeries>()
                .Where(s => s.IsVisible && s.IsHorizontal == IsHorizontal)
                .ToList();

            int seriesIndex = stackedSeriesList.IndexOf(this);
            if (seriesIndex < 0) return;

            var barFill = Fill ?? context.DefaultBrush;
            var barStroke = Stroke ?? barFill;
            var barPen = new Pen(barStroke, StrokeThickness);

            double progress = context.AnimationProgress;

            if (!IsHorizontal)
            {
                // --- VERTICAL STACKED COLUMN ---
                double slotWidth = context.PlotArea.Width / (context.Transform.XMax - context.Transform.XMin + 1.0);
                double usableWidth = slotWidth * 0.70; // 70% slot width, 30% spacing
                double leftOffset = -usableWidth / 2.0;

                for (int i = 0; i < rawPoints.Count; i++)
                {
                    var pt = rawPoints[i];
                    if (double.IsNaN(pt.Y)) continue;

                    // Calculate accumulated baseline from preceding series
                    double stackBaseline = 0.0;
                    for (int s = 0; s < seriesIndex; s++)
                    {
                        var sPoints = stackedSeriesList[s].GetDataPoints();
                        if (i < sPoints.Count && !double.IsNaN(sPoints[i].Y))
                        {
                            stackBaseline += sPoints[i].Y;
                        }
                    }

                    double stackTop = stackBaseline + pt.Y;

                    var centerPt = context.Transform.ToScreen(pt.X, 0.0);
                    double rectX = centerPt.X + leftOffset;

                    double screenBaselineY = context.Transform.ToScreen(pt.X, stackBaseline).Y;
                    double screenValueY = context.Transform.ToScreen(pt.X, stackTop).Y;

                    // Animate stack height growth
                    double animatedY = screenBaselineY + (screenValueY - screenBaselineY) * progress;

                    double rectY = Math.Min(screenBaselineY, animatedY);
                    double rectHeight = Math.Max(1.0, Math.Abs(screenBaselineY - animatedY));

                    var barRect = new Rect(rectX, rectY, usableWidth, rectHeight);

                    if (CornerRadius > 0)
                    {
                        context.DrawingContext.DrawRectangle(barFill, barPen, new RoundedRect(barRect, new CornerRadius(CornerRadius)));
                    }
                    else
                    {
                        context.DrawingContext.DrawRectangle(barFill, barPen, barRect);
                    }
                }
            }
            else
            {
                // --- HORIZONTAL STACKED BAR ---
                double slotHeight = context.PlotArea.Height / (context.Transform.XMax - context.Transform.XMin + 1.0);
                double usableHeight = slotHeight * 0.70;
                double topOffset = -usableHeight / 2.0;

                for (int i = 0; i < rawPoints.Count; i++)
                {
                    var pt = rawPoints[i];
                    if (double.IsNaN(pt.Y)) continue;

                    double stackBaseline = 0.0;
                    for (int s = 0; s < seriesIndex; s++)
                    {
                        var sPoints = stackedSeriesList[s].GetDataPoints();
                        if (i < sPoints.Count && !double.IsNaN(sPoints[i].Y))
                        {
                            stackBaseline += sPoints[i].Y;
                        }
                    }

                    double stackTop = stackBaseline + pt.Y;

                    var centerPt = context.Transform.ToScreen(0.0, pt.X);
                    double rectY = centerPt.Y + topOffset;

                    double screenBaselineX = context.Transform.ToScreen(stackBaseline, pt.X).X;
                    double screenValueX = context.Transform.ToScreen(stackTop, pt.X).X;

                    double animatedX = screenBaselineX + (screenValueX - screenBaselineX) * progress;

                    double rectX = Math.Min(screenBaselineX, animatedX);
                    double rectWidth = Math.Max(1.0, Math.Abs(screenBaselineX - animatedX));

                    var barRect = new Rect(rectX, rectY, rectWidth, usableHeight);

                    if (CornerRadius > 0)
                    {
                        context.DrawingContext.DrawRectangle(barFill, barPen, new RoundedRect(barRect, new CornerRadius(CornerRadius)));
                    }
                    else
                    {
                        context.DrawingContext.DrawRectangle(barFill, barPen, barRect);
                    }
                }
            }
        }
    }
}
