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
    public class StepLineSeries : CartesianSeries
    {
        public static readonly DependencyProperty StepBeforeProperty =
            DependencyProperty.Register(nameof(StepBefore), typeof(bool), typeof(StepLineSeries), new PropertyMetadata(false, OnPropertyChanged));

        public bool StepBefore { get => (bool)GetValue(StepBeforeProperty);
            set => SetValue(StepBeforeProperty, value);
        }

        public StepLineSeries()
        {
        }

        public override void RenderSeries(SeriesRenderContext context)
        {
            var rawPoints = GetDataPoints();
            if (rawPoints.Count < 2) return;

            var lineStroke = Stroke ?? context.DefaultSKPaint;
            var lineSKPaint = new Pen(lineStroke, StrokeThickness, lineCap: SKStrokeCap.Round, lineJoin: SKStrokeJoin.Round);

            double progress = context.AnimationProgress;
            double baselineY = context.Transform.YMin;

            var geometry = new SKPath();
            // using (var ctx = geometry.Open())
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
                        geometry.MoveTo(currentPt, false);
                        first = false;
                    }
                    else
                    {
                        if (StepBefore)
                        {
                            // Step Vertically then Horizontally: (x1, y1) -> (x1, y2) -> (x2, y2)
                            geometry.LineTo(new Point(prevPt.X, currentPt.Y));
                        }
                        else
                        {
                            // Step Horizontally then Vertically: (x1, y1) -> (x2, y1) -> (x2, y2)
                            geometry.LineTo(new Point(currentPt.X, prevPt.Y));
                        }
                        geometry.LineTo(currentPt);
                    }
                    prevPt = currentPt;
                }
            }

            context.Canvas.DrawPath(geometry, lineSKPaint ?? null);
        }
    }
}