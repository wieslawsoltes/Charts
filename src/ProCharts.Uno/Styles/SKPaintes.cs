using System;
using System.Collections.Generic;
using SkiaSharp;

namespace ProCharts.Uno
{
    public static class SKPaintes
    {
        public static SKPaint White => new SKPaint { Color = SKColors.White, IsAntialias = true };
        public static SKPaint Black => new SKPaint { Color = SKColors.Black, IsAntialias = true };
        public static SKPaint Transparent => new SKPaint { Color = SKColors.Transparent, IsAntialias = true };
    }

    public class SolidSKColorSKPaint : SKPaint
    {
        public SKColor SKColor => Color;

        public SolidSKColorSKPaint()
        {
            Style = SKPaintStyle.Fill;
            IsAntialias = true;
        }

        public SolidSKColorSKPaint(SKColor color) : this()
        {
            Color = color;
        }

        public SolidSKColorSKPaint(SKColor color, double opacity) : this()
        {
            Color = new SKColor(color.Red, color.Green, color.Blue, (byte)(opacity * 255));
        }

        public static SolidSKColorSKPaint Parse(string hex)
        {
            return new SolidSKColorSKPaint(SKColor.Parse(hex));
        }
    }

    public class DashStyle
    {
        public IReadOnlyList<double> Dashes { get; }
        public double Offset { get; }

        public DashStyle(IEnumerable<double> dashes, double offset)
        {
            Dashes = new List<double>(dashes).AsReadOnly();
            Offset = offset;
        }

        public static DashStyle Dash => new DashStyle(new[] { 4.0, 4.0 }, 0.0);
    }

    public class Pen : SKPaint
    {
        public Pen(SKPaint brush, double thickness = 1.0, DashStyle dashStyle = null, SKStrokeCap lineCap = SKStrokeCap.Butt, SKStrokeJoin lineJoin = SKStrokeJoin.Miter)
        {
            Style = SKPaintStyle.Stroke;
            StrokeWidth = (float)thickness;
            Color = brush?.Color ?? SKColors.Black;
            IsAntialias = true;
            StrokeCap = lineCap;
            StrokeJoin = lineJoin;

            if (dashStyle != null && dashStyle.Dashes != null)
            {
                var floats = new float[dashStyle.Dashes.Count];
                for (int i = 0; i < dashStyle.Dashes.Count; i++)
                {
                    floats[i] = (float)dashStyle.Dashes[i];
                }
                PathEffect = SKPathEffect.CreateDash(floats, (float)dashStyle.Offset);
            }
        }
    }

    public enum RelativeUnit
    {
        Absolute,
        Relative
    }

    public struct RelativePoint
    {
        public double X { get; }
        public double Y { get; }
        public RelativeUnit Unit { get; }

        public RelativePoint(double x, double y, RelativeUnit unit)
        {
            X = x;
            Y = y;
            Unit = unit;
        }

        public SKPoint ToSKPoint(SKRect bounds)
        {
            if (Unit == RelativeUnit.Relative)
            {
                return new SKPoint(
                    (float)(bounds.Left + X * bounds.Width),
                    (float)(bounds.Top + Y * bounds.Height)
                );
            }
            else
            {
                return new SKPoint((float)X, (float)Y);
            }
        }
    }

    public class GradientStop
    {
        public SKColor Color { get; set; }
        public double Offset { get; set; }

        public GradientStop() { }
        public GradientStop(SKColor color, double offset)
        {
            Color = color;
            Offset = offset;
        }
    }

    public class GradientStops : List<GradientStop>
    {
        public GradientStops() { }
        public GradientStops(IEnumerable<GradientStop> collection) : base(collection) { }
    }

    public class LinearGradientSKPaint : SKPaint
    {
        public RelativePoint StartPoint { get; set; }
        public RelativePoint EndPoint { get; set; }
        public GradientStops GradientStops { get; set; } = new GradientStops();

        public LinearGradientSKPaint()
        {
            Style = SKPaintStyle.Fill;
            IsAntialias = true;
        }

        public void ApplyShader(SKRect bounds)
        {
            if (GradientStops == null || GradientStops.Count == 0) return;

            // Sort stops by offset
            var sortedStops = new List<GradientStop>(GradientStops);
            sortedStops.Sort((a, b) => a.Offset.CompareTo(b.Offset));

            var colors = new SKColor[sortedStops.Count];
            var positions = new float[sortedStops.Count];
            for (int i = 0; i < sortedStops.Count; i++)
            {
                colors[i] = sortedStops[i].Color;
                positions[i] = (float)sortedStops[i].Offset;
            }

            var start = StartPoint.ToSKPoint(bounds);
            var end = EndPoint.ToSKPoint(bounds);

            Shader = SKShader.CreateLinearGradient(
                start,
                end,
                colors,
                positions,
                SKShaderTileMode.Clamp
            );
        }
    }
}
