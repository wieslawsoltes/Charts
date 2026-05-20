using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ProCharts.Uno
{
    // --- BASIC PAINT TYPES ---

    public enum SKPaintStyle
    {
        Fill,
        Stroke,
        StrokeAndFill
    }

    public enum SKStrokeCap
    {
        Butt,
        Round,
        Square
    }

    public enum SKStrokeJoin
    {
        Miter,
        Round,
        Bevel
    }

    public struct SKColor
    {
        public byte Alpha { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public SKColor(byte red, byte green, byte blue, byte alpha = 255)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public SKColor(byte red, byte green, byte blue) : this(red, green, blue, 255) { }

        public SKColor(Windows.UI.Color color)
        {
            Red = color.R;
            Green = color.G;
            Blue = color.B;
            Alpha = color.A;
        }

        public static implicit operator Windows.UI.Color(SKColor color) => Windows.UI.Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        public static implicit operator SKColor(Windows.UI.Color color) => new SKColor(color);

        public Windows.UI.Color ToColor() => (Windows.UI.Color)this;

        public static SKColor Parse(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return new SKColor(0, 0, 0, 0);
            hex = hex.TrimStart('#');
            if (hex.Length == 8)
            {
                byte a = Convert.ToByte(hex.Substring(0, 2), 16);
                byte r = Convert.ToByte(hex.Substring(2, 2), 16);
                byte g = Convert.ToByte(hex.Substring(4, 2), 16);
                byte b = Convert.ToByte(hex.Substring(6, 2), 16);
                return new SKColor(r, g, b, a);
            }
            else if (hex.Length == 6)
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                return new SKColor(r, g, b, 255);
            }
            return new SKColor(0, 0, 0, 255);
        }
    }

    public static class SKColors
    {
        public static SKColor White => new SKColor(255, 255, 255);
        public static SKColor Black => new SKColor(0, 0, 0);
        public static SKColor Transparent => new SKColor(0, 0, 0, 0);
        public static SKColor SkyBlue => new SKColor(135, 206, 235);
        public static SKColor Purple => new SKColor(128, 0, 128);
        public static SKColor Teal => new SKColor(0, 128, 128);
    }

    public class SKPaint
    {
        private SKColor _color;
        private Brush? _winUIBrush;

        public SKColor Color
        {
            get
            {
                if (_winUIBrush is SolidColorBrush scb) return new SKColor(scb.Color);
                return _color;
            }
            set
            {
                _color = value;
                _winUIBrush = null;
            }
        }

        public SKPaintStyle Style { get; set; } = SKPaintStyle.Fill;
        public float StrokeWidth { get; set; } = 1.0f;
        public SKStrokeCap StrokeCap { get; set; } = SKStrokeCap.Butt;
        public SKStrokeJoin StrokeJoin { get; set; } = SKStrokeJoin.Miter;
        public bool IsAntialias { get; set; } = true;

        // Legacy Skia fields mapped to no-ops or simple properties
        public object? Shader { get; set; }
        public object? PathEffect { get; set; }
        public object? MaskFilter { get; set; }
        public object? ImageFilter { get; set; }
        public object? BlendMode { get; set; }

        public SKPaint() { }

        public SKPaint(Brush brush)
        {
            _winUIBrush = brush;
            if (brush is SolidColorBrush scb)
            {
                _color = new SKColor(scb.Color);
            }
        }

        public virtual Brush ToBrush()
        {
            if (_winUIBrush != null) return _winUIBrush;
            return new SolidColorBrush(_color.ToColor());
        }

        public static implicit operator Brush(SKPaint? paint) => paint?.ToBrush()!;
        public static implicit operator SKPaint(Brush? brush) => brush != null ? new SKPaint(brush) : null!;
    }

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

    // --- PEN AND DASH STYLES ---

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
        public Brush? Brush { get; }
        public DashStyle? Dash { get; }

        public Pen(Brush? brush, double thickness = 1.0, DashStyle? dashStyle = null, SKStrokeCap lineCap = SKStrokeCap.Butt, SKStrokeJoin lineJoin = SKStrokeJoin.Miter)
        {
            Brush = brush;
            Style = SKPaintStyle.Stroke;
            StrokeWidth = (float)thickness;
            Dash = dashStyle;
            StrokeCap = lineCap;
            StrokeJoin = lineJoin;

            if (brush is SolidColorBrush scb)
            {
                Color = new SKColor(scb.Color);
            }
            else if ((object?)brush is SKPaint paint)
            {
                Color = paint.Color;
            }
            else
            {
                Color = brush.GetColor();
            }
        }

        public override Brush ToBrush()
        {
            return Brush ?? base.ToBrush();
        }
    }

    // --- GRADIENTS AND RELATIVE COORDINATES ---

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

        public Windows.Foundation.Point ToPoint(ProCharts.Uno.Maths.Rect bounds)
        {
            if (Unit == RelativeUnit.Relative)
            {
                return new Windows.Foundation.Point(
                    bounds.Left + X * bounds.Width,
                    bounds.Top + Y * bounds.Height
                );
            }
            else
            {
                return new Windows.Foundation.Point(X, Y);
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

        public override Brush ToBrush()
        {
            var brush = new LinearGradientBrush();
            brush.StartPoint = new Windows.Foundation.Point(StartPoint.X, StartPoint.Y);
            brush.EndPoint = new Windows.Foundation.Point(EndPoint.X, EndPoint.Y);
            brush.MappingMode = StartPoint.Unit == RelativeUnit.Relative ? BrushMappingMode.RelativeToBoundingBox : BrushMappingMode.Absolute;

            foreach (var stop in GradientStops)
            {
                brush.GradientStops.Add(new Microsoft.UI.Xaml.Media.GradientStop
                {
                    Color = stop.Color.ToColor(),
                    Offset = stop.Offset
                });
            }
            return brush;
        }
    }

    // --- BRUSH EXTENSIONS ---

    public static class BrushExtensions
    {
        public static Windows.UI.Color GetColor(this Brush? brush)
        {
            if (brush is SolidColorBrush scb)
            {
                return scb.Color;
            }
            if (brush is LinearGradientBrush lgb && lgb.GradientStops.Count > 0)
            {
                return lgb.GradientStops[0].Color;
            }
            if ((object?)brush is SKPaint paint)
            {
                return paint.Color.ToColor();
            }
            return Microsoft.UI.Colors.Transparent;
        }
    }
}
