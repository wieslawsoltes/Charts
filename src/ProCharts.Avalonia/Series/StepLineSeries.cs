using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using ProCharts.Avalonia.Maths;

namespace ProCharts.Avalonia.Series
{
    public class StepLineSeries : CartesianSeries
    {
        public static readonly StyledProperty<bool> StepBeforeProperty =
            AvaloniaProperty.Register<StepLineSeries, bool>(nameof(StepBefore), false);

        public bool StepBefore
        {
            get => GetValue(StepBeforeProperty);
            set => SetValue(StepBeforeProperty, value);
        }

        public StepLineSeries()
        {
            StepBeforeProperty.Changed.AddClassHandler<StepLineSeries>((x, e) => x.RaiseSeriesChanged());
        }

        public override void RenderSeries(in SeriesRenderContext context)
        {
            var rawPoints = GetDataPoints();
            if (rawPoints.Count < 2) return;

            var lineStroke = Stroke ?? context.DefaultBrush;
            var linePen = new Pen(lineStroke, StrokeThickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

            double progress = context.AnimationProgress;
            double baselineY = context.Transform.YMin;

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                bool first = true;
                Point prevPt = default;

                foreach (var pt in rawPoints)
                {
                    if (double.IsNaN(pt.X) || double.IsNaN(pt.Y)) continue;

                    // Compute animated Y coordinate
                    var screenPt = context.Transform.ToScreen(pt.X, pt.Y);
                    double screenBaselineY = context.Transform.ToScreen(pt.X, baselineY).Y;
                    double animatedY = screenBaselineY + (screenPt.Y - screenBaselineY) * progress;
                    var currentPt = new Point(screenPt.X, animatedY);

                    if (first)
                    {
                        ctx.BeginFigure(currentPt, false);
                        first = false;
                    }
                    else
                    {
                        if (StepBefore)
                        {
                            // Step Vertically then Horizontally: (x1, y1) -> (x1, y2) -> (x2, y2)
                            ctx.LineTo(new Point(prevPt.X, currentPt.Y));
                        }
                        else
                        {
                            // Step Horizontally then Vertically: (x1, y1) -> (x2, y1) -> (x2, y2)
                            ctx.LineTo(new Point(currentPt.X, prevPt.Y));
                        }
                        ctx.LineTo(currentPt);
                    }
                    prevPt = currentPt;
                }
            }

            context.DrawingContext.DrawGeometry(null, linePen, geometry);
        }
    }
}
