using System;
using Avalonia;
using Avalonia.Media;
using ProCharts.Maths;

namespace ProCharts.Series
{
    public enum ScatterShape
    {
        Circle,
        Square,
        Cross,
        Diamond
    }

    public class ScatterSeries : CartesianSeries
    {
        public static readonly StyledProperty<ScatterShape> ShapeProperty =
            AvaloniaProperty.Register<ScatterSeries, ScatterShape>(nameof(Shape), ScatterShape.Circle);

        public static readonly StyledProperty<double> SizeProperty =
            AvaloniaProperty.Register<ScatterSeries, double>(nameof(Size), 8.0);

        public ScatterShape Shape
        {
            get => GetValue(ShapeProperty);
            set => SetValue(ShapeProperty, value);
        }

        public double Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public ScatterSeries()
        {
            ShapeProperty.Changed.AddClassHandler<ScatterSeries>((x, e) => x.RaiseSeriesChanged());
            SizeProperty.Changed.AddClassHandler<ScatterSeries>((x, e) => x.RaiseSeriesChanged());
        }

        public override void RenderSeries(in SeriesRenderContext context)
        {
            var rawPoints = GetDataPoints();
            if (rawPoints.Count == 0) return;

            var pointFill = Fill ?? context.DefaultBrush;
            var pointStroke = Stroke ?? pointFill;
            var pointPen = new Pen(pointStroke, StrokeThickness);

            double progress = context.AnimationProgress;
            double halfSize = Size / 2.0;

            foreach (var pt in rawPoints)
            {
                if (double.IsNaN(pt.X) || double.IsNaN(pt.Y)) continue;

                var screenPt = context.Transform.ToScreen(pt.X, pt.Y);

                // Entry animation: grow markers or rise from YMin
                double animSize = Size * progress;
                double halfAnim = animSize / 2.0;

                // Alternate entry animation: rise from baseline
                double screenBaselineY = context.Transform.ToScreen(pt.X, context.Transform.YMin).Y;
                double animatedY = screenBaselineY + (screenPt.Y - screenBaselineY) * progress;
                var animatedPt = new Point(screenPt.X, animatedY);

                IBrush? itemFill = pointFill;
                if (!string.IsNullOrEmpty(PointBrushPath))
                {
                    // Attempt dynamic item brush mapping
                    // Can fallback if not resolvable
                }

                switch (Shape)
                {
                    case ScatterShape.Circle:
                        context.DrawingContext.DrawEllipse(itemFill, pointPen, animatedPt, halfAnim, halfAnim);
                        break;

                    case ScatterShape.Square:
                        var rect = new Rect(animatedPt.X - halfAnim, animatedPt.Y - halfAnim, animSize, animSize);
                        context.DrawingContext.DrawRectangle(itemFill, pointPen, rect);
                        break;

                    case ScatterShape.Diamond:
                        var geometry = new StreamGeometry();
                        using (var ctx = geometry.Open())
                        {
                            ctx.BeginFigure(new Point(animatedPt.X, animatedPt.Y - halfAnim), true);
                            ctx.LineTo(new Point(animatedPt.X + halfAnim, animatedPt.Y));
                            ctx.LineTo(new Point(animatedPt.X, animatedPt.Y + halfAnim));
                            ctx.LineTo(new Point(animatedPt.X - halfAnim, animatedPt.Y));
                            ctx.EndFigure(true);
                        }
                        context.DrawingContext.DrawGeometry(itemFill, pointPen, geometry);
                        break;

                    case ScatterShape.Cross:
                        var crossPen = new Pen(pointStroke, StrokeThickness + 1.0);
                        context.DrawingContext.DrawLine(crossPen, new Point(animatedPt.X - halfAnim, animatedPt.Y), new Point(animatedPt.X + halfAnim, animatedPt.Y));
                        context.DrawingContext.DrawLine(crossPen, new Point(animatedPt.X, animatedPt.Y - halfAnim), new Point(animatedPt.X, animatedPt.Y + halfAnim));
                        break;
                }
            }
        }
    }
}
