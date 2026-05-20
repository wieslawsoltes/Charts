using System;
using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Series
{
    public class AreaSeries : LineSeries
    {
        public AreaSeries()
        {
            // Set default styling
            StrokeThickness = 2.0;
        }

        public override void RenderSeries(SeriesRenderContext context)
        {
            var rawPoints = GetDataPoints();
            if (rawPoints.Count == 0) return;

            // 1. Process Empty/NaN Points
            var processedPoints = ProcessEmptyPoints(rawPoints, context.Transform);

            // 2. Build segments for rendering (useful for Gap mode)
            var segments = BuildSegments(processedPoints);

            var lineStroke = Stroke ?? context.DefaultSKPaint;
            var lineSKPaint = new Pen(lineStroke, StrokeThickness, lineCap: SKStrokeCap.Round, lineJoin: SKStrokeJoin.Round);

            // 3. Render each segment
            foreach (var segment in segments)
            {
                if (segment.Count < 2) continue;

                // Animate points
                var animatedPoints = AnimatePoints(segment, context.Transform, context.AnimationProgress);

                // Draw filled area first
                DrawFilledArea(context.Canvas, context.Transform, animatedPoints, lineStroke);

                // Draw outline stroke on top
                if (IsSmooth && animatedPoints.Count > 2)
                {
                    DrawBezierSpline(context.Canvas, lineSKPaint, animatedPoints);
                }
                else
                {
                    DrawStraightLines(context.Canvas, lineSKPaint, animatedPoints);
                }
            }

            // 4. Render Markers if active
            if (ShowMarkers)
            {
                var markerFillSKPaint = MarkerFill ?? lineStroke;
                var markerSKPaint = new Pen(lineStroke, 1.0);
                double halfSize = MarkerSize / 2.0;

                foreach (var segment in segments)
                {
                    var animatedPoints = AnimatePoints(segment, context.Transform, context.AnimationProgress);
                    foreach (var pt in animatedPoints)
                    {
                        context.Canvas.DrawEllipse(markerFillSKPaint, markerSKPaint, pt, halfSize, halfSize);
                    }
                }
            }
        }

        private void DrawFilledArea(SKCanvas context, CoordinateTransform transform, List<Point> points, SKPaint seriesSKPaint)
        {
            if (points.Count < 2) return;

            // Create a gorgeous linear gradient brush if no custom Fill is set
            var areaFill = Fill;
            if (areaFill == null && seriesSKPaint != null)
            {
                var color = seriesSKPaint.Color;
                areaFill = new LinearGradientSKPaint
                {
                    StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(new SKColor((byte)(color.Red), (byte)(color.Green), (byte)(color.Blue), (byte)((byte)(color.Alpha * 0.40))), 0.0),
                        new GradientStop(new SKColor((byte)(color.Red), (byte)(color.Green), (byte)(color.Blue), (byte)((byte)(color.Alpha * 0.02))), 1.0)
                    }
                };
            }
            else if (areaFill == null)
            {
                areaFill = new SolidSKColorSKPaint(new SKColor((byte)(79), (byte)(70), (byte)(229), (byte)(50))); // Fallback semi-transparent indigo
            }

            // Determine baseline in Y screen space
            double rawBaselineY = transform.YMin;
            if (rawBaselineY < 0 && transform.YMax > 0) rawBaselineY = 0.0;
            
            // X values for baseline caps
            double firstX = transform.ToData(points[0]).X;
            double lastX = transform.ToData(points[points.Count - 1]).X;

            var firstBaselinePt = transform.ToScreen(firstX, rawBaselineY);
            var lastBaselinePt = transform.ToScreen(lastX, rawBaselineY);

            var geometry = new SKPath();
            using (var ctx = geometry.Open())
            {
                // Start at the bottom left baseline cap
                geometry.MoveTo(firstBaselinePt, true);
                
                // Draw up to the first point
                geometry.LineTo(points[0]);

                // Draw curve or straight lines along the series path
                if (IsSmooth && points.Count > 2)
                {
                    const double smoothness = 0.4;
                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        var p0 = i == 0 ? points[i] : points[i - 1];
                        var p1 = points[i];
                        var p2 = points[i + 1];
                        var p3 = i + 2 < points.Count ? points[i + 2] : p2;

                        var cp1 = p1 + (p2 - p0) * (smoothness / 3.0);
                        var cp2 = p2 - (p3 - p1) * (smoothness / 3.0);

                        ctx.CubicBezierTo(cp1, cp2, p2);
                    }
                }
                else
                {
                    for (int i = 1; i < points.Count; i++)
                    {
                        geometry.LineTo(points[i]);
                    }
                }

                // Drop down to the bottom right baseline cap
                geometry.LineTo(lastBaselinePt);
            }

            // Draw the closed geometry with our beautiful transparent gradient
            context.DrawGeometry(areaFill, null, geometry);
        }
    }
}