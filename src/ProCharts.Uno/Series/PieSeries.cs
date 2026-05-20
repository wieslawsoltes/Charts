using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ProCharts.Uno.Series
{
    public class PieSeries : ChartSeries
    {
        // --- DEPENDENCY PROPERTIES ---

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(PieSeries), new PropertyMetadata(0.0, OnPropertyChanged));

        public static readonly DependencyProperty ExplodedProperty =
            DependencyProperty.Register(nameof(Exploded), typeof(bool), typeof(PieSeries), new PropertyMetadata(false, OnPropertyChanged));

        public static readonly DependencyProperty ExplodeDistanceProperty =
            DependencyProperty.Register(nameof(ExplodeDistance), typeof(double), typeof(PieSeries), new PropertyMetadata(8.0, OnPropertyChanged));

        // --- PROPERTIES ---

        public double Value { get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public bool Exploded { get => (bool)GetValue(ExplodedProperty);
            set => SetValue(ExplodedProperty, value);
        }

        public double ExplodeDistance { get => (double)GetValue(ExplodeDistanceProperty);
            set => SetValue(ExplodeDistanceProperty, value);
        }

        public PieSeries()
        {
        }

        /// <summary>
        /// Renders a circular sector slice with dynamic center explosion and donut settings.
        /// </summary>
        public void RenderSlice(
            SKCanvas context,
            Point center,
            double outerRadius,
            double innerRadius,
            double startAngle,
            double sweepAngle,
            SKPaint defaultSKPaint)
        {
            if (sweepAngle <= 0.01) return;

            // 1. Calculate Exploded Center Offset
            if (Exploded)
            {
                double bisectAngle = startAngle + sweepAngle / 2.0;
                double radBisect = bisectAngle * Math.PI / 180.0;
                double dx = ExplodeDistance * Math.Cos(radBisect);
                double dy = ExplodeDistance * Math.Sin(radBisect);
                center = new Point(center.X + dx, center.Y + dy);
            }

            var fillSKPaint = Fill ?? defaultSKPaint;
            var baseStroke = Stroke ?? SKPaintes.Transparent;
            var strokeSKPaint = new Pen(baseStroke, StrokeThickness);

            // Handle full circles (near 360 degrees) to prevent ArcTo collapse
            if (sweepAngle >= 359.9)
            {
                if (innerRadius <= 0.0)
                {
                    context.DrawEllipse(fillSKPaint, strokeSKPaint, center, outerRadius, outerRadius);
                }
                else
                {
                    // Draw donut using nested geometry subtraction
                    var outerGeom = new EllipseGeometry(new Rect(center.X - outerRadius, center.Y - outerRadius, outerRadius * 2, outerRadius * 2));
                    var innerGeom = new EllipseGeometry(new Rect(center.X - innerRadius, center.Y - innerRadius, innerRadius * 2, innerRadius * 2));
                    var donutGeom = new CombinedGeometry(GeometryCombineMode.Exclude, outerGeom, innerGeom);
                    context.DrawGeometry(fillSKPaint, strokeSKPaint, donutGeom);
                }
                return;
            }

            // 2. Draw Donut or Pie Sector using SKPath
            var geometry = new SKPath();
            using (var ctx = geometry.Open())
            {
                double radStart = startAngle * Math.PI / 180.0;
                double radEnd = (startAngle + sweepAngle) * Math.PI / 180.0;

                Point pos = center + new Point(outerRadius * Math.Cos(radStart), outerRadius * Math.Sin(radStart));
                Point poe = center + new Point(outerRadius * Math.Cos(radEnd), outerRadius * Math.Sin(radEnd));

                bool isLargeArc = sweepAngle > 180.0;

                if (innerRadius <= 0.0)
                {
                    // --- STANDARD PIE SLICE ---
                    geometry.MoveTo(center, true);
                    geometry.LineTo(pos);
                    ctx.ArcTo(poe, new Size(outerRadius, outerRadius), 0.0, isLargeArc, SweepDirection.Clockwise);
                }
                else
                {
                    // --- HOLLOW DONUT SLICE ---
                    Point pis = center + new Point(innerRadius * Math.Cos(radStart), innerRadius * Math.Sin(radStart));
                    Point pie = center + new Point(innerRadius * Math.Cos(radEnd), innerRadius * Math.Sin(radEnd));

                    geometry.MoveTo(pis, true);
                    geometry.LineTo(pos);
                    ctx.ArcTo(poe, new Size(outerRadius, outerRadius), 0.0, isLargeArc, SweepDirection.Clockwise);
                    geometry.LineTo(pie);
                    ctx.ArcTo(pis, new Size(innerRadius, innerRadius), 0.0, isLargeArc, SweepDirection.CounterClockwise);
                }
            }

            context.DrawGeometry(fillSKPaint, strokeSKPaint, geometry);
        }
    }
}