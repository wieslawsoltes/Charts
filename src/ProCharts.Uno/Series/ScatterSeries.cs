using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
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

            var pointFill = Fill ?? context.DefaultSKPaint;
            var pointStroke = Stroke ?? pointFill;
            var pointSKPaint = new Pen(pointStroke, StrokeThickness);

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

                SKPaint? itemFill = pointFill;
                if (!string.IsNullOrEmpty(PointSKPaintPath))
                {
                    // Attempt dynamic item brush mapping
                    // Can fallback if not resolvable
                }

                switch (Shape)
                {
                    case ScatterShape.Circle:
                        context.Canvas.DrawCircle((float)animatedPt.X, (float)animatedPt.Y, (float)halfAnim, pointSKPaint ?? itemFill);
                        break;

                    case ScatterShape.Square:
                        var rect = new Rect(animatedPt.X - halfAnim, animatedPt.Y - halfAnim, animSize, animSize);
                        context.Canvas.DrawRect((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, pointSKPaint ?? itemFill);
                        break;

                    case ScatterShape.Diamond:
                        var geometry = new SKPath();
                        // using (var ctx = geometry.Open())
                        {
                            geometry.MoveTo(new Point(animatedPt.X, animatedPt.Y - halfAnim), true);
                            geometry.LineTo(new Point(animatedPt.X + halfAnim, animatedPt.Y));
                            geometry.LineTo(new Point(animatedPt.X, animatedPt.Y + halfAnim));
                            geometry.LineTo(new Point(animatedPt.X - halfAnim, animatedPt.Y));
                            geometry.Close(true);
                        }
                        context.Canvas.DrawPath(geometry, pointSKPaint ?? itemFill);
                        break;

                    case ScatterShape.Cross:
                        var crossSKPaint = new Pen(pointStroke, StrokeThickness + 1.0);
                        context.Canvas.DrawLine(crossSKPaint, new Point(animatedPt.X - halfAnim, animatedPt.Y), new Point(animatedPt.X + halfAnim, animatedPt.Y));
                        context.Canvas.DrawLine(crossSKPaint, new Point(animatedPt.X, animatedPt.Y - halfAnim), new Point(animatedPt.X, animatedPt.Y + halfAnim));
                        break;
                }
            }
        }
    }
}