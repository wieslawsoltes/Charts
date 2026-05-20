using System;
using System.Collections.Generic;
using SkiaSharp;
using Microsoft.UI.Xaml;
using Windows.Foundation;

namespace ProCharts.Uno.Maths
{
    // --- CUSTOM COORDINATE TYPES ---

    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static Point operator *(Point p, double scale)
        {
            return new Point(p.X * scale, p.Y * scale);
        }

        public static Point operator /(Point p, double scale)
        {
            return new Point(p.X / scale, p.Y / scale);
        }

        // Implicit conversions
        public static implicit operator Windows.Foundation.Point(Point p) => new Windows.Foundation.Point(p.X, p.Y);
        public static implicit operator Point(Windows.Foundation.Point p) => new Point(p.X, p.Y);

        public static implicit operator SKPoint(Point p) => new SKPoint((float)p.X, (float)p.Y);
        public static implicit operator Point(SKPoint p) => new Point(p.X, p.Y);
    }

    public struct Rect
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public Rect(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rect(Point position, Size size)
        {
            X = position.X;
            Y = position.Y;
            Width = size.Width;
            Height = size.Height;
        }

        public Rect(Point p1, Point p2)
        {
            X = Math.Min(p1.X, p2.X);
            Y = Math.Min(p1.Y, p2.Y);
            Width = Math.Abs(p1.X - p2.X);
            Height = Math.Abs(p1.Y - p2.Y);
        }

        public double Left => X;
        public double Top => Y;
        public double Right => X + Width;
        public double Bottom => Y + Height;
        public Point Position => new Point(X, Y);
        public Size Size => new Size(Width, Height);

        public Point Center => new Point(X + Width / 2.0, Y + Height / 2.0);

        public bool Contains(Point p)
        {
            return p.X >= X && p.X <= X + Width && p.Y >= Y && p.Y <= Y + Height;
        }

        public bool Intersects(Rect rect)
        {
            return rect.Left < Right && Left < rect.Right && rect.Top < Bottom && Top < rect.Bottom;
        }

        public Rect Inflate(double thickness)
        {
            return new Rect(X - thickness, Y - thickness, Width + thickness * 2, Height + thickness * 2);
        }

        public Rect Deflate(double thickness)
        {
            return new Rect(X + thickness, Y + thickness, Math.Max(0, Width - thickness * 2), Math.Max(0, Height - thickness * 2));
        }

        // Implicit conversions
        public static implicit operator Windows.Foundation.Rect(Rect r) => new Windows.Foundation.Rect(r.X, r.Y, r.Width, r.Height);
        public static implicit operator Rect(Windows.Foundation.Rect r) => new Rect(r.X, r.Y, r.Width, r.Height);

        public static implicit operator SKRect(Rect r) => new SKRect((float)r.Left, (float)r.Top, (float)r.Right, (float)r.Bottom);
        public static implicit operator Rect(SKRect r) => new Rect(r.Left, r.Top, r.Width, r.Height);
    }

    public class RoundedRect
    {
        public Rect Rect { get; }
        public Microsoft.UI.Xaml.CornerRadius CornerRadius { get; }

        public RoundedRect(Rect rect, Microsoft.UI.Xaml.CornerRadius cornerRadius)
        {
            Rect = rect;
            CornerRadius = cornerRadius;
        }

        public RoundedRect(Rect rect, double radiusX, double radiusY)
        {
            Rect = rect;
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(radiusX, radiusY, radiusX, radiusY);
        }

        public RoundedRect(Rect rect, double radius)
        {
            Rect = rect;
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(radius);
        }
    }

    // --- MATRIX TRANSFORMS ---

    public struct Matrix
    {
        public SKMatrix Value { get; }

        public Matrix(SKMatrix value)
        {
            Value = value;
        }

        public static Matrix CreateRotation(double angleInRadians)
        {
            return new Matrix(SKMatrix.CreateRotation((float)angleInRadians));
        }

        public static Matrix CreateTranslation(double x, double y)
        {
            return new Matrix(SKMatrix.CreateTranslation((float)x, (float)y));
        }

        public static Matrix CreateScale(double x, double y)
        {
            return new Matrix(SKMatrix.CreateScale((float)x, (float)y));
        }

        public static Matrix Identity => new Matrix(SKMatrix.CreateIdentity());

        public static Matrix operator *(Matrix left, Matrix right)
        {
            return new Matrix(SKMatrix.Concat(left.Value, right.Value));
        }

        public static implicit operator SKMatrix(Matrix m) => m.Value;
        public static implicit operator Matrix(SKMatrix m) => new Matrix(m);
    }

    // --- GEOMETRY BUILDING AND COMBINING ---

    public abstract class Geometry
    {
        public abstract SKPath ToSKPath();
    }

    public class EllipseGeometry : Geometry
    {
        public Rect Rect { get; }
        public EllipseGeometry(Rect rect)
        {
            Rect = rect;
        }

        public override SKPath ToSKPath()
        {
            var path = new SKPath();
            path.AddOval(new SKRect((float)Rect.Left, (float)Rect.Top, (float)Rect.Right, (float)Rect.Bottom));
            return path;
        }
    }

    public enum GeometryCombineMode
    {
        Union,
        Intersect,
        Xor,
        Exclude
    }

    public class CombinedGeometry : Geometry
    {
        public GeometryCombineMode CombineMode { get; }
        public Geometry Geometry1 { get; }
        public Geometry Geometry2 { get; }

        public CombinedGeometry(GeometryCombineMode combineMode, Geometry geometry1, Geometry geometry2)
        {
            CombineMode = combineMode;
            Geometry1 = geometry1;
            Geometry2 = geometry2;
        }

        public override SKPath ToSKPath()
        {
            var p1 = Geometry1?.ToSKPath() ?? new SKPath();
            var p2 = Geometry2?.ToSKPath() ?? new SKPath();

            var op = SKPathOp.Union;
            switch (CombineMode)
            {
                case GeometryCombineMode.Union:
                    op = SKPathOp.Union;
                    break;
                case GeometryCombineMode.Intersect:
                    op = SKPathOp.Intersect;
                    break;
                case GeometryCombineMode.Xor:
                    op = SKPathOp.Xor;
                    break;
                case GeometryCombineMode.Exclude:
                    op = SKPathOp.Difference;
                    break;
            }

            var path = p1.Op(p2, op);
            return path ?? p1;
        }
    }

    // --- CANVAS AND PATH EXTENSION METHODS ---

    public static class CanvasExtensions
    {
        private static void PreparePaint(SKPaint? paint, SKRect bounds)
        {
            if (paint is LinearGradientSKPaint lgp)
            {
                lgp.ApplyShader(bounds);
            }
        }

        public static void DrawLine(this SKCanvas canvas, SKPaint? paint, Point p1, Point p2)
        {
            if (paint == null) return;
            var bounds = new SKRect((float)Math.Min(p1.X, p2.X), (float)Math.Min(p1.Y, p2.Y), (float)Math.Max(p1.X, p2.X), (float)Math.Max(p1.Y, p2.Y));
            PreparePaint(paint, bounds);
            canvas.DrawLine((float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y, paint);
        }

        public static void DrawText(this SKCanvas canvas, FormattedText? ft, Point origin)
        {
            if (ft == null || string.IsNullOrEmpty(ft.Text)) return;
            if (ft.Paint != null)
            {
                PreparePaint(ft.Paint, new SKRect((float)origin.X, (float)origin.Y, (float)(origin.X + ft.Width), (float)(origin.Y + ft.Height)));
            }
            canvas.DrawText(ft.Text, (float)origin.X, (float)(origin.Y + ft.Height), ft.Font, ft.Paint);
        }

        public static void DrawRectangle(this SKCanvas canvas, SKPaint? fill, SKPaint? stroke, Rect rect)
        {
            SKRect r = rect;
            if (fill != null)
            {
                PreparePaint(fill, r);
                canvas.DrawRect(r, fill);
            }
            if (stroke != null)
            {
                var oldStyle = stroke.Style;
                stroke.Style = SKPaintStyle.Stroke;
                PreparePaint(stroke, r);
                canvas.DrawRect(r, stroke);
                stroke.Style = oldStyle;
            }
        }

        public static void DrawRectangle(this SKCanvas canvas, SKPaint? fill, SKPaint? stroke, RoundedRect roundedRect)
        {
            var rr = new SKRoundRect();
            var rect = new SKRect((float)roundedRect.Rect.Left, (float)roundedRect.Rect.Top, (float)roundedRect.Rect.Right, (float)roundedRect.Rect.Bottom);
            rr.SetRectRadii(rect, new[]
            {
                new SKPoint((float)roundedRect.CornerRadius.TopLeft, (float)roundedRect.CornerRadius.TopLeft),
                new SKPoint((float)roundedRect.CornerRadius.TopRight, (float)roundedRect.CornerRadius.TopRight),
                new SKPoint((float)roundedRect.CornerRadius.BottomRight, (float)roundedRect.CornerRadius.BottomRight),
                new SKPoint((float)roundedRect.CornerRadius.BottomLeft, (float)roundedRect.CornerRadius.BottomLeft),
            });

            if (fill != null)
            {
                var oldStyle = fill.Style;
                fill.Style = SKPaintStyle.Fill;
                PreparePaint(fill, rect);
                canvas.DrawRoundRect(rr, fill);
                fill.Style = oldStyle;
            }
            if (stroke != null)
            {
                var oldStyle = stroke.Style;
                stroke.Style = SKPaintStyle.Stroke;
                PreparePaint(stroke, rect);
                canvas.DrawRoundRect(rr, stroke);
                stroke.Style = oldStyle;
            }
        }

        public static void DrawEllipse(this SKCanvas canvas, SKPaint? fill, SKPaint? stroke, Point center, double rx, double ry)
        {
            var rect = new SKRect((float)(center.X - rx), (float)(center.Y - ry), (float)(center.X + rx), (float)(center.Y + ry));
            if (fill != null)
            {
                var oldStyle = fill.Style;
                fill.Style = SKPaintStyle.Fill;
                PreparePaint(fill, rect);
                canvas.DrawOval((float)center.X, (float)center.Y, (float)rx, (float)ry, fill);
                fill.Style = oldStyle;
            }
            if (stroke != null)
            {
                var oldStyle = stroke.Style;
                stroke.Style = SKPaintStyle.Stroke;
                PreparePaint(stroke, rect);
                canvas.DrawOval((float)center.X, (float)center.Y, (float)rx, (float)ry, stroke);
                stroke.Style = oldStyle;
            }
        }

        public static void DrawGeometry(this SKCanvas canvas, SKPaint? fill, SKPaint? stroke, Geometry? geometry)
        {
            if (geometry == null) return;
            var path = geometry.ToSKPath();
            canvas.DrawGeometry(fill, stroke, path);
        }

        public static void DrawGeometry(this SKCanvas canvas, SKPaint? fill, SKPaint? stroke, SKPath? path)
        {
            if (path == null) return;
            var bounds = path.Bounds;
            if (fill != null)
            {
                var oldStyle = fill.Style;
                fill.Style = SKPaintStyle.Fill;
                PreparePaint(fill, bounds);
                canvas.DrawPath(path, fill);
                fill.Style = oldStyle;
            }
            if (stroke != null)
            {
                var oldStyle = stroke.Style;
                stroke.Style = SKPaintStyle.Stroke;
                PreparePaint(stroke, bounds);
                canvas.DrawPath(path, stroke);
                stroke.Style = oldStyle;
            }
        }

        public static IDisposable PushTransform(this SKCanvas canvas, SKMatrix matrix)
        {
            canvas.Save();
            canvas.Concat(matrix);
            return new CanvasSavePopper(canvas);
        }

        public static IDisposable PushGeometryClip(this SKCanvas canvas, Geometry clipGeometry)
        {
            canvas.Save();
            if (clipGeometry != null)
            {
                var path = clipGeometry.ToSKPath();
                canvas.ClipPath(path);
            }
            return new CanvasSavePopper(canvas);
        }

        private class CanvasSavePopper : IDisposable
        {
            private readonly SKCanvas _canvas;
            public CanvasSavePopper(SKCanvas canvas) => _canvas = canvas;
            public void Dispose() => _canvas.Restore();
        }
    }

    public static class PathExtensions
    {
        public static PathOpenContext Open(this SKPath path)
        {
            return new PathOpenContext(path);
        }

        public static void MoveTo(this SKPath path, Point p)
        {
            path.MoveTo((float)p.X, (float)p.Y);
        }

        public static void MoveTo(this SKPath path, Point p, bool isFilled)
        {
            path.MoveTo((float)p.X, (float)p.Y);
        }

        public static void LineTo(this SKPath path, Point p)
        {
            path.LineTo((float)p.X, (float)p.Y);
        }

        public static void CubicBezierTo(this SKPath path, Point p1, Point p2, Point p3)
        {
            path.CubicTo((float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y, (float)p3.X, (float)p3.Y);
        }

        public static void QuadBezierTo(this SKPath path, Point p1, Point p2)
        {
            path.QuadTo((float)p1.X, (float)p1.Y, (float)p2.X, (float)p2.Y);
        }

        public static void ArcTo(this SKPath path, Point p, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection)
        {
            path.ArcTo(
                (float)size.Width,
                (float)size.Height,
                (float)rotationAngle,
                isLargeArc ? SKPathArcSize.Large : SKPathArcSize.Small,
                sweepDirection == SweepDirection.Clockwise ? SKPathDirection.Clockwise : SKPathDirection.CounterClockwise,
                (float)p.X,
                (float)p.Y);
        }

        public static void Close(this SKPath path, bool close)
        {
            if (close) path.Close();
        }
    }

    public class PathOpenContext : IDisposable
    {
        private readonly SKPath _path;
        public PathOpenContext(SKPath path) => _path = path;

        public void MoveTo(Point p, bool isFilled = true) => _path.MoveTo(p, isFilled);
        public void LineTo(Point p) => _path.LineTo(p);
        public void CubicBezierTo(Point p1, Point p2, Point p3) => _path.CubicBezierTo(p1, p2, p3);
        public void QuadBezierTo(Point p1, Point p2) => _path.QuadBezierTo(p1, p2);
        
        public void ArcTo(Point p, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection) =>
            _path.ArcTo(p, size, rotationAngle, isLargeArc, sweepDirection);

        public void Close(bool close) => _path.Close(close);

        public void Dispose()
        {
            // NOP
        }
    }

    public enum SweepDirection
    {
        CounterClockwise,
        Clockwise
    }
}
