using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Series
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
        public static readonly DependencyProperty ShapeProperty =
            DependencyProperty.Register(nameof(Shape), typeof(ScatterShape), typeof(ScatterSeries), new PropertyMetadata(ScatterShape.Circle, OnPropertyChanged));

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(nameof(Size), typeof(double), typeof(ScatterSeries), new PropertyMetadata(8.0, OnPropertyChanged));

        public ScatterShape Shape { get => (ScatterShape)GetValue(ShapeProperty);
            set => SetValue(ShapeProperty, value);
        }

        public double Size { get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public ScatterSeries()
        {
        }

        public override void RenderSeries(SeriesRenderContext context)
        {
            var rawPoints = GetDataPoints();
            if (rawPoints.Count == 0) return;

            var pointFill = Fill ?? context.DefaultBrush;
            var pointStroke = Stroke ?? pointFill;
            var pointBrush = new Pen(pointStroke, StrokeThickness);

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

                Brush? itemFill = pointFill;
                if (!string.IsNullOrEmpty(PointBrushPath))
                {
                    // Attempt dynamic item brush mapping
                    // Can fallback if not resolvable
                }

                switch (Shape)
                {
                    case ScatterShape.Circle:
                        context.Canvas.DrawEllipse(itemFill, pointBrush, animatedPt, halfAnim, halfAnim);
                        break;

                    case ScatterShape.Square:
                        var rect = new Rect(animatedPt.X - halfAnim, animatedPt.Y - halfAnim, animSize, animSize);
                        context.Canvas.DrawRectangle(itemFill, pointBrush, rect);
                        break;

                    case ScatterShape.Diamond:
                        var geometry = new StreamGeometry();
                        // using (var ctx = geometry.Open())
                        {
                            geometry.MoveTo(new Point(animatedPt.X, animatedPt.Y - halfAnim), true);
                            geometry.LineTo(new Point(animatedPt.X + halfAnim, animatedPt.Y));
                            geometry.LineTo(new Point(animatedPt.X, animatedPt.Y + halfAnim));
                            geometry.LineTo(new Point(animatedPt.X - halfAnim, animatedPt.Y));
                            geometry.Close(true);
                        }
                        context.Canvas.DrawGeometry(itemFill, pointBrush, geometry);
                        break;

                    case ScatterShape.Cross:
                        var crossBrush = new Pen(pointStroke, StrokeThickness + 1.0);
                        context.Canvas.DrawLine(crossBrush, new Point(animatedPt.X - halfAnim, animatedPt.Y), new Point(animatedPt.X + halfAnim, animatedPt.Y));
                        context.Canvas.DrawLine(crossBrush, new Point(animatedPt.X, animatedPt.Y - halfAnim), new Point(animatedPt.X, animatedPt.Y + halfAnim));
                        break;
                }
            }
        }
    }
}