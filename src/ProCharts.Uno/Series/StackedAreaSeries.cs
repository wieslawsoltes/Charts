using System;
using System.Linq;
using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Controls;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Series
{
    public class StackedAreaSeries : CartesianSeries
    {
        public static readonly DependencyProperty IsSmoothProperty =
            DependencyProperty.Register(nameof(IsSmooth), typeof(bool), typeof(StackedAreaSeries), new PropertyMetadata(false, OnPropertyChanged));

        public bool IsSmooth { get => (bool)GetValue(IsSmoothProperty);
            set => SetValue(IsSmoothProperty, value);
        }

        public StackedAreaSeries()
        {
            StrokeThickness = 2.0;
        }

        public override void RenderSeries(SeriesRenderContext context)
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
            var areaBrush = new Pen(areaStroke, StrokeThickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

            if (areaFill == null && areaStroke != null)
            {
                var color = areaStroke.GetColor();
                var lgb = new LinearGradientBrush();
                lgb.StartPoint = new Windows.Foundation.Point(0.5, 0);
                lgb.EndPoint = new Windows.Foundation.Point(0.5, 1);
                lgb.GradientStops.Add(new GradientStop { Color = new Color(color.R, color.G, color.B, (byte)(color.A * 0.45)), Offset = 0.0 });
                lgb.GradientStops.Add(new GradientStop { Color = new Color(color.R, color.G, color.B, (byte)(color.A * 0.05)), Offset = 1.0 });
                areaFill = lgb;
            }
            else if (areaFill == null)
            {
                areaFill = new SolidColorBrush(new Color(6, 182, 212, 80)); // Cyan fallback
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
            using (var geometry = fillGeometry.Open())
            {
                geometry.MoveTo(bottomScreenPoints[0], true);
                
                // Go left-to-right along top
                for (int i = 0; i < topScreenPoints.Count; i++)
                {
                    geometry.LineTo(topScreenPoints[i]);
                }

                // Go right-to-left along bottom
                for (int i = bottomScreenPoints.Count - 1; i >= 0; i--)
                {
                    geometry.LineTo(bottomScreenPoints[i]);
                }

                geometry.Close(true);
            }
            context.Canvas.DrawGeometry(areaFill, null, fillGeometry);

            // Draw line stroke on top path only
            var strokeGeometry = new StreamGeometry();
            using (var geometry = strokeGeometry.Open())
            {
                geometry.MoveTo(topScreenPoints[0], false);
                for (int i = 1; i < topScreenPoints.Count; i++)
                {
                    geometry.LineTo(topScreenPoints[i]);
                }
            }
            context.Canvas.DrawGeometry(null, areaBrush, strokeGeometry);
        }
    }
}