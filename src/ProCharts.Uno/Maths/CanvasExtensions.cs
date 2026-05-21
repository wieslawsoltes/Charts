using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
        public double M11 { get; set; }
        public double M12 { get; set; }
        public double M21 { get; set; }
        public double M22 { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

        public Matrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }

        public static Matrix CreateRotation(double angleInRadians)
        {
            double cos = Math.Cos(angleInRadians);
            double sin = Math.Sin(angleInRadians);
            return new Matrix(cos, sin, -sin, cos, 0, 0);
        }

        public static Matrix CreateTranslation(double x, double y)
        {
            return new Matrix(1, 0, 0, 1, x, y);
        }

        public static Matrix CreateScale(double x, double y)
        {
            return new Matrix(x, 0, 0, y, 0, 0);
        }

        public static Matrix Identity => new Matrix(1, 0, 0, 1, 0, 0);

        public static Matrix operator *(Matrix left, Matrix right)
        {
            return new Matrix(
                left.M11 * right.M11 + left.M12 * right.M21,
                left.M11 * right.M12 + left.M12 * right.M22,
                left.M21 * right.M11 + left.M22 * right.M21,
                left.M21 * right.M12 + left.M22 * right.M22,
                left.OffsetX * right.M11 + left.OffsetY * right.M21 + right.OffsetX,
                left.OffsetX * right.M12 + left.OffsetY * right.M22 + right.OffsetY
            );
        }
    }

    // --- GEOMETRY BUILDING AND COMBINING ---

    public abstract class Geometry
    {
        public abstract Microsoft.UI.Xaml.Media.Geometry ToWinUIGeometry();
        public abstract Rect GetBounds();
    }

    public class EllipseGeometry : Geometry
    {
        public Rect Rect { get; }
        public EllipseGeometry(Rect rect)
        {
            Rect = rect;
        }

        public override Rect GetBounds() => Rect;

        public override Microsoft.UI.Xaml.Media.Geometry ToWinUIGeometry()
        {
            return new Microsoft.UI.Xaml.Media.EllipseGeometry
            {
                Center = Rect.Center,
                RadiusX = Rect.Width / 2.0,
                RadiusY = Rect.Height / 2.0
            };
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

        public override Rect GetBounds()
        {
            var b1 = Geometry1?.GetBounds() ?? new Rect();
            var b2 = Geometry2?.GetBounds() ?? new Rect();
            if (Geometry1 == null) return b2;
            if (Geometry2 == null) return b1;
            double x = Math.Min(b1.X, b2.X);
            double y = Math.Min(b1.Y, b2.Y);
            double w = Math.Max(b1.Right, b2.Right) - x;
            double h = Math.Max(b1.Bottom, b2.Bottom) - y;
            return new Rect(x, y, w, h);
        }

        public override Microsoft.UI.Xaml.Media.Geometry ToWinUIGeometry()
        {
            var g1 = Geometry1?.ToWinUIGeometry();
            var g2 = Geometry2?.ToWinUIGeometry();

            if (g1 == null) return g2 ?? new Microsoft.UI.Xaml.Media.GeometryGroup();
            if (g2 == null) return g1;

            var group = new Microsoft.UI.Xaml.Media.GeometryGroup();
            if (CombineMode == GeometryCombineMode.Exclude || CombineMode == GeometryCombineMode.Xor)
            {
                group.FillRule = Microsoft.UI.Xaml.Media.FillRule.EvenOdd;
            }
            else
            {
                group.FillRule = Microsoft.UI.Xaml.Media.FillRule.Nonzero;
            }
            group.Children.Add(g1);
            group.Children.Add(g2);
            return group;
        }
    }

    public class StreamGeometry : Geometry
    {
        public Microsoft.UI.Xaml.Media.PathGeometry PathGeometry { get; } = new Microsoft.UI.Xaml.Media.PathGeometry();
        private Microsoft.UI.Xaml.Media.PathFigure? _currentFigure;

        private double _minX = double.MaxValue;
        private double _minY = double.MaxValue;
        private double _maxX = double.MinValue;
        private double _maxY = double.MinValue;

        private void UpdateBounds(Point p)
        {
            if (p.X < _minX) _minX = p.X;
            if (p.Y < _minY) _minY = p.Y;
            if (p.X > _maxX) _maxX = p.X;
            if (p.Y > _maxY) _maxY = p.Y;
        }

        public override Rect GetBounds()
        {
            if (_minX == double.MaxValue) return new Rect();
            return new Rect(_minX, _minY, _maxX - _minX, _maxY - _minY);
        }

        public override Microsoft.UI.Xaml.Media.Geometry ToWinUIGeometry() => PathGeometry;

        public void MoveTo(Point p, bool isFilled = true)
        {
            UpdateBounds(p);
            _currentFigure = new Microsoft.UI.Xaml.Media.PathFigure
            {
                StartPoint = p,
                IsFilled = isFilled,
                IsClosed = false
            };
            PathGeometry.Figures.Add(_currentFigure);
        }

        public void LineTo(Point p)
        {
            UpdateBounds(p);
            if (_currentFigure == null) MoveTo(p);
            _currentFigure!.Segments.Add(new Microsoft.UI.Xaml.Media.LineSegment { Point = p });
        }

        public void CubicBezierTo(Point p1, Point p2, Point p3)
        {
            UpdateBounds(p1);
            UpdateBounds(p2);
            UpdateBounds(p3);
            if (_currentFigure == null) MoveTo(p1);
            _currentFigure!.Segments.Add(new Microsoft.UI.Xaml.Media.BezierSegment
            {
                Point1 = p1,
                Point2 = p2,
                Point3 = p3
            });
        }

        public void QuadBezierTo(Point p1, Point p2)
        {
            UpdateBounds(p1);
            UpdateBounds(p2);
            if (_currentFigure == null) MoveTo(p1);
            _currentFigure!.Segments.Add(new Microsoft.UI.Xaml.Media.QuadraticBezierSegment
            {
                Point1 = p1,
                Point2 = p2
            });
        }

        public void ArcTo(Point p, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection)
        {
            UpdateBounds(p);
            if (_currentFigure == null) MoveTo(p);
            _currentFigure!.Segments.Add(new Microsoft.UI.Xaml.Media.ArcSegment
            {
                Point = p,
                Size = size,
                RotationAngle = rotationAngle,
                IsLargeArc = isLargeArc,
                SweepDirection = sweepDirection == SweepDirection.Clockwise ? Microsoft.UI.Xaml.Media.SweepDirection.Clockwise : Microsoft.UI.Xaml.Media.SweepDirection.Counterclockwise
            });
        }

        public void Close()
        {
            if (_currentFigure != null)
            {
                _currentFigure.IsClosed = true;
            }
        }
    }

    // --- DRAWING CONTEXT ENGINE ---

    public class DrawingContext
    {
        private readonly Microsoft.UI.Xaml.Controls.Canvas _canvas;
        private readonly Stack<TransformGroup> _transformStack = new();
        private readonly Stack<Geometry> _clipStack = new();

        public DrawingContext(Microsoft.UI.Xaml.Controls.Canvas canvas)
        {
            _canvas = canvas;
        }

        private TransformGroup CurrentTransform
        {
            get
            {
                var tg = new TransformGroup();
                foreach (var t in _transformStack)
                {
                    if (t is TransformGroup subGroup)
                    {
                        foreach (var child in subGroup.Children)
                        {
                            tg.Children.Add(CloneTransform(child));
                        }
                    }
                    else
                    {
                        tg.Children.Add(CloneTransform(t));
                    }
                }
                return tg;
            }
        }

        private Transform CloneTransform(Transform t)
        {
            if (t is MatrixTransform mt)
            {
                return new MatrixTransform { Matrix = mt.Matrix };
            }
            if (t is ScaleTransform st)
            {
                return new ScaleTransform { ScaleX = st.ScaleX, ScaleY = st.ScaleY, CenterX = st.CenterX, CenterY = st.CenterY };
            }
            if (t is TranslateTransform tt)
            {
                return new TranslateTransform { X = tt.X, Y = tt.Y };
            }
            if (t is RotateTransform rt)
            {
                return new RotateTransform { Angle = rt.Angle, CenterX = rt.CenterX, CenterY = rt.CenterY };
            }
            if (t is TransformGroup tg)
            {
                var newTg = new TransformGroup();
                foreach (var child in tg.Children)
                {
                    newTg.Children.Add(CloneTransform(child));
                }
                return newTg;
            }
            return t;
        }

        private RectangleGeometry? CurrentClip
        {
            get
            {
                if (_clipStack.Count == 0) return null;
                Rect result = _clipStack.Peek().GetBounds();
                foreach (var clip in _clipStack)
                {
                    var b = clip.GetBounds();
                    double x = Math.Max(result.X, b.X);
                    double y = Math.Max(result.Y, b.Y);
                    double right = Math.Min(result.Right, b.Right);
                    double bottom = Math.Min(result.Bottom, b.Bottom);
                    double w = Math.Max(0, right - x);
                    double h = Math.Max(0, bottom - y);
                    result = new Rect(x, y, w, h);
                }
                return new RectangleGeometry { Rect = result };
            }
        }

        private void ApplyTransformAndClip(UIElement element)
        {
            if (_transformStack.Count > 0)
            {
                element.RenderTransform = CurrentTransform;
            }
            var clip = CurrentClip;
            if (clip != null)
            {
                element.Clip = clip;
            }
        }

        private void ApplyStrokeAndFill(Microsoft.UI.Xaml.Shapes.Shape shape, Brush? fill, Pen? stroke)
        {
            if (fill != null)
            {
                shape.Fill = fill;
            }
            if (stroke != null)
            {
                shape.Stroke = stroke.Brush;
                shape.StrokeThickness = stroke.Thickness;

                if (stroke.Dash != null && stroke.Dash.Dashes != null && stroke.Dash.Dashes.Count > 0)
                {
                    var dashCollection = new DoubleCollection();
                    foreach (var dash in stroke.Dash.Dashes)
                    {
                        dashCollection.Add(dash);
                    }
                    shape.StrokeDashArray = dashCollection;
                    shape.StrokeDashOffset = stroke.Dash.Offset;
                }

                shape.StrokeStartLineCap = stroke.LineCap;
                shape.StrokeEndLineCap = stroke.LineCap;
                shape.StrokeLineJoin = stroke.LineJoin;
            }
        }

        public void DrawLine(Pen? pen, Point p1, Point p2)
        {
            if (pen == null) return;
            var line = new Microsoft.UI.Xaml.Shapes.Line
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y
            };
            ApplyStrokeAndFill(line, null, pen);
            ApplyTransformAndClip(line);
            _canvas.Children.Add(line);
        }

        public void DrawLine(double x1, double y1, double x2, double y2, Pen? pen)
        {
            DrawLine(pen, new Point(x1, y1), new Point(x2, y2));
        }

        public void DrawPath(StreamGeometry? path, Pen? stroke)
        {
            DrawGeometry(null, stroke, path);
        }

        public void DrawText(FormattedText? ft, Point origin)
        {
            if (ft == null || string.IsNullOrEmpty(ft.Text)) return;

            var tb = new TextBlock
            {
                Text = ft.Text,
                FontSize = ft.FontSize,
                FlowDirection = ft.FlowDirection,
                Foreground = ft.Paint ?? new SolidColorBrush(Microsoft.UI.Colors.Black)
            };

            if (ft.Typeface != null)
            {
                tb.FontFamily = new FontFamily(ft.Typeface.FontFamily);
                tb.FontStyle = ft.Typeface.FontStyle == FontStyle.Italic ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal;

                switch (ft.Typeface.FontWeight)
                {
                    case FontWeight.Bold:
                        tb.FontWeight = Microsoft.UI.Text.FontWeights.Bold;
                        break;
                    case FontWeight.SemiBold:
                        tb.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
                        break;
                    case FontWeight.Medium:
                        tb.FontWeight = Microsoft.UI.Text.FontWeights.Medium;
                        break;
                    case FontWeight.Light:
                        tb.FontWeight = Microsoft.UI.Text.FontWeights.Light;
                        break;
                    default:
                        tb.FontWeight = Microsoft.UI.Text.FontWeights.Normal;
                        break;
                }
            }

            Canvas.SetLeft(tb, origin.X);
            Canvas.SetTop(tb, origin.Y);
            ApplyTransformAndClip(tb);
            _canvas.Children.Add(tb);
        }

        public void DrawRectangle(Brush? fill, Pen? stroke, Rect rect)
        {
            var r = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Width = Math.Max(0, rect.Width),
                Height = Math.Max(0, rect.Height)
            };
            ApplyStrokeAndFill(r, fill, stroke);
            Canvas.SetLeft(r, rect.X);
            Canvas.SetTop(r, rect.Y);
            ApplyTransformAndClip(r);
            _canvas.Children.Add(r);
        }

        public void DrawRectangle(Brush? fill, Pen? stroke, RoundedRect roundedRect)
        {
            var r = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Width = Math.Max(0, roundedRect.Rect.Width),
                Height = Math.Max(0, roundedRect.Rect.Height),
                RadiusX = roundedRect.CornerRadius.TopLeft,
                RadiusY = roundedRect.CornerRadius.TopLeft
            };
            ApplyStrokeAndFill(r, fill, stroke);
            Canvas.SetLeft(r, roundedRect.Rect.X);
            Canvas.SetTop(r, roundedRect.Rect.Y);
            ApplyTransformAndClip(r);
            _canvas.Children.Add(r);
        }

        public void DrawEllipse(Brush? fill, Pen? stroke, Point center, double rx, double ry)
        {
            var e = new Microsoft.UI.Xaml.Shapes.Ellipse
            {
                Width = rx * 2,
                Height = ry * 2
            };
            ApplyStrokeAndFill(e, fill, stroke);
            Canvas.SetLeft(e, center.X - rx);
            Canvas.SetTop(e, center.Y - ry);
            ApplyTransformAndClip(e);
            _canvas.Children.Add(e);
        }

        public void DrawGeometry(Brush? fill, Pen? stroke, Geometry? geometry)
        {
            if (geometry == null) return;
            var winGeometry = geometry.ToWinUIGeometry();
            DrawGeometryInternal(fill, stroke, winGeometry);
        }

        public void DrawGeometry(Brush? fill, Pen? stroke, StreamGeometry? path)
        {
            if (path == null) return;
            var winGeometry = path.ToWinUIGeometry();
            DrawGeometryInternal(fill, stroke, winGeometry);
        }

        private void DrawGeometryInternal(Brush? fill, Pen? stroke, Microsoft.UI.Xaml.Media.Geometry winGeometry)
        {
            var p = new Microsoft.UI.Xaml.Shapes.Path
            {
                Data = winGeometry
            };
            ApplyStrokeAndFill(p, fill, stroke);
            Canvas.SetLeft(p, 0);
            Canvas.SetTop(p, 0);
            ApplyTransformAndClip(p);
            _canvas.Children.Add(p);
        }

        public IDisposable PushTransform(Matrix matrix)
        {
            var transform = new TransformGroup();
            transform.Children.Add(new MatrixTransform
            {
                Matrix = new Microsoft.UI.Xaml.Media.Matrix(
                    matrix.M11, matrix.M12,
                    matrix.M21, matrix.M22,
                    matrix.OffsetX, matrix.OffsetY)
            });
            _transformStack.Push(transform);
            return new ActionDisposable(() => _transformStack.Pop());
        }

        public IDisposable PushGeometryClip(Geometry clipGeometry)
        {
            _clipStack.Push(clipGeometry);
            return new ActionDisposable(() => _clipStack.Pop());
        }
    }

    public class ActionDisposable : IDisposable
    {
        private readonly Action _action;
        public ActionDisposable(Action action) => _action = action;
        public void Dispose() => _action();
    }

    // --- PATH EXTENSIONS ---

    public static class PathExtensions
    {
        public static PathOpenContext Open(this StreamGeometry path)
        {
            return new PathOpenContext(path);
        }

        public static void MoveTo(this StreamGeometry path, Point p)
        {
            path.MoveTo(p);
        }

        public static void MoveTo(this StreamGeometry path, Point p, bool isFilled)
        {
            path.MoveTo(p, isFilled);
        }

        public static void LineTo(this StreamGeometry path, Point p)
        {
            path.LineTo(p);
        }

        public static void CubicBezierTo(this StreamGeometry path, Point p1, Point p2, Point p3)
        {
            path.CubicBezierTo(p1, p2, p3);
        }

        public static void QuadBezierTo(this StreamGeometry path, Point p1, Point p2)
        {
            path.QuadBezierTo(p1, p2);
        }

        public static void ArcTo(this StreamGeometry path, Point p, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection)
        {
            path.ArcTo(p, size, rotationAngle, isLargeArc, sweepDirection);
        }

        public static void Close(this StreamGeometry path, bool close)
        {
            if (close) path.Close();
        }
    }

    public class PathOpenContext : IDisposable
    {
        private readonly StreamGeometry _path;
        public PathOpenContext(StreamGeometry path) => _path = path;

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
