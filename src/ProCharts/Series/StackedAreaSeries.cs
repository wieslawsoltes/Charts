using System;
using System.Linq;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using ProCharts.Controls;
using ProCharts.Maths;

namespace ProCharts.Series
{
    public class StackedAreaSeries : CartesianSeries
    {
        public static readonly StyledProperty<bool> IsSmoothProperty =
            AvaloniaProperty.Register<StackedAreaSeries, bool>(nameof(IsSmooth), false);

        public bool IsSmooth
        {
            get => GetValue(IsSmoothProperty);
            set => SetValue(IsSmoothProperty, value);
        }

        public StackedAreaSeries()
        {
            IsSmoothProperty.Changed.AddClassHandler<StackedAreaSeries>((x, e) => x.RaiseSeriesChanged());
            StrokeThickness = 2.0;
        }

        public override void RenderSeries(in SeriesRenderContext context)
        {
            var rawPoints = GetDataPoints();
            if (rawPoints.Count == 0) return;

            var parent = context.Chart as CartesianChart;
            if (parent == null) return;

            var stackedSeriesList = parent.Series
                .OfType<StackedAreaSeries>()
                .Where(s => s.IsVisible)
                .ToList();

            int seriesIndex = stackedSeriesList.IndexOf(this);
            if (seriesIndex < 0) return;

            var areaFill = Fill;
            var areaStroke = Stroke ?? context.DefaultBrush;
            var areaPen = new Pen(areaStroke, StrokeThickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

            if (areaFill == null && areaStroke is SolidColorBrush solidBrush)
            {
                var color = solidBrush.Color;
                areaFill = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(Color.FromArgb((byte)(color.A * 0.45), color.R, color.G, color.B), 0.0),
                        new GradientStop(Color.FromArgb((byte)(color.A * 0.05), color.R, color.G, color.B), 1.0)
                    }
                };
            }
            else if (areaFill == null)
            {
                areaFill = new SolidColorBrush(Color.FromArgb(80, 6, 182, 212)); // Cyan fallback
            }

            double progress = context.AnimationProgress;

            // Generate screen coordinates for both bottom and top paths of the stack
            var bottomScreenPoints = new List<Point>(rawPoints.Count);
            var topScreenPoints = new List<Point>(rawPoints.Count);

            for (int i = 0; i < rawPoints.Count; i++)
            {
                var pt = rawPoints[i];
                double valY = double.IsNaN(pt.Y) ? 0.0 : pt.Y;

                double baseValY = 0.0;
                for (int s = 0; s < seriesIndex; s++)
                {
                    var sPoints = stackedSeriesList[s].GetDataPoints();
                    if (i < sPoints.Count && !double.IsNaN(sPoints[i].Y))
                    {
                        baseValY += sPoints[i].Y;
                    }
                }

                double topValY = baseValY + valY;

                // Animate values
                double animatedBaseY = baseValY * progress;
                double animatedTopY = (baseValY + (topValY - baseValY) * progress);

                var baseScr = context.Transform.ToScreen(pt.X, animatedBaseY);
                var topScr = context.Transform.ToScreen(pt.X, animatedTopY);

                bottomScreenPoints.Add(baseScr);
                topScreenPoints.Add(topScr);
            }

            // Draw filled polygon
            var fillGeometry = new StreamGeometry();
            using (var ctx = fillGeometry.Open())
            {
                ctx.BeginFigure(bottomScreenPoints[0], true);
                
                // Go left-to-right along top
                for (int i = 0; i < topScreenPoints.Count; i++)
                {
                    ctx.LineTo(topScreenPoints[i]);
                }

                // Go right-to-left along bottom
                for (int i = bottomScreenPoints.Count - 1; i >= 0; i--)
                {
                    ctx.LineTo(bottomScreenPoints[i]);
                }

                ctx.EndFigure(true);
            }
            context.DrawingContext.DrawGeometry(areaFill, null, fillGeometry);

            // Draw line stroke on top path only
            var strokeGeometry = new StreamGeometry();
            using (var ctx = strokeGeometry.Open())
            {
                ctx.BeginFigure(topScreenPoints[0], false);
                for (int i = 1; i < topScreenPoints.Count; i++)
                {
                    ctx.LineTo(topScreenPoints[i]);
                }
            }
            context.DrawingContext.DrawGeometry(null, areaPen, strokeGeometry);
        }
    }
}
