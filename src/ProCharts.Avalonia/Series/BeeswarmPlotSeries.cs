using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Avalonia.Maths;

namespace ProCharts.Avalonia.Series
{
    public class BeeswarmPlotSeries : CartesianSeries
    {
        public static readonly StyledProperty<double> CircleRadiusProperty =
            AvaloniaProperty.Register<BeeswarmPlotSeries, double>(nameof(CircleRadius), 4.5);

        public double CircleRadius
        {
            get => GetValue(CircleRadiusProperty);
            set => SetValue(CircleRadiusProperty, value);
        }

        public BeeswarmPlotSeries()
        {
            CircleRadiusProperty.Changed.AddClassHandler<BeeswarmPlotSeries>((x, e) => x.RaiseSeriesChanged());
        }

        public override void RenderSeries(in SeriesRenderContext context)
        {
            var rawPoints = GetDataPoints();
            if (rawPoints.Count == 0) return;

            var activeBrush = Stroke ?? Fill ?? context.DefaultBrush;
            var circlePen = new Pen(new SolidColorBrush(Color.Parse("#40FFFFFF")), 0.5);

            double r = CircleRadius;
            double diameter = r * 2.0;
            double progress = context.AnimationProgress;

            // Group points by X (Category) to swarm them independently
            var grouped = rawPoints.GroupBy(p => p.X).ToList();

            foreach (var group in grouped)
            {
                double categoryX = group.Key;
                var valuesY = group.Select(p => p.Y).Where(y => !double.IsNaN(y)).OrderBy(y => y).ToList();

                var placedPoints = new List<Point>();

                foreach (var yVal in valuesY)
                {
                    // Animate value from center/baseline
                    double baseline = context.Transform.YMin;
                    if (baseline < 0 && context.Transform.YMax > 0) baseline = 0.0;
                    double animatedY = baseline + (yVal - baseline) * progress;

                    var ptScreen = context.Transform.ToScreen(categoryX, animatedY);

                    // Find non-overlapping X screen coordinate outwards from the center track (ptScreen.X)
                    double bestX = ptScreen.X;
                    double step = 1.0;
                    double dx = 0.0;

                    while (HasCollision(bestX, ptScreen.Y, placedPoints, diameter))
                    {
                        dx += step;
                        // Search left and right alternatively
                        if (!HasCollision(ptScreen.X + dx, ptScreen.Y, placedPoints, diameter))
                        {
                            bestX = ptScreen.X + dx;
                            break;
                        }
                        if (!HasCollision(ptScreen.X - dx, ptScreen.Y, placedPoints, diameter))
                        {
                            bestX = ptScreen.X - dx;
                            break;
                        }
                    }

                    var swarmPt = new Point(bestX, ptScreen.Y);
                    placedPoints.Add(swarmPt);

                    // Draw the swarm point circle
                    context.DrawingContext.DrawEllipse(activeBrush, circlePen, swarmPt, r, r);
                }
            }
        }

        private bool HasCollision(double x, double y, List<Point> placed, double minDistance)
        {
            double minDistanceSq = minDistance * minDistance;
            foreach (var p in placed)
            {
                double dx = x - p.X;
                double dy = y - p.Y;
                if (dx * dx + dy * dy < minDistanceSq)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
