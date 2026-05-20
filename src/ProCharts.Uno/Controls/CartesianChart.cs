using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Avalonia.Collections;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

using ProCharts.Uno.Components;
using ProCharts.Uno.Maths;
using ProCharts.Uno.Series;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public partial class CartesianChart : ChartBase
    {
        // --- DEPENDENCY PROPERTIES ---

        public static readonly DependencyProperty XAxisProperty =
            DependencyProperty.Register(nameof(XAxis), typeof(Axis), typeof(CartesianChart), new PropertyMetadata(default(Axis), OnPropertyChanged));

        public static readonly DependencyProperty YAxisProperty =
            DependencyProperty.Register(nameof(YAxis), typeof(Axis), typeof(CartesianChart), new PropertyMetadata(default(Axis), OnPropertyChanged));

        public static readonly DependencyProperty AxisSKPaintProperty =
            DependencyProperty.Register(nameof(AxisSKPaint), typeof(SKPaint), typeof(CartesianChart), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        public static readonly DependencyProperty GridLineSKPaintProperty =
            DependencyProperty.Register(nameof(GridLineSKPaint), typeof(SKPaint), typeof(CartesianChart), new PropertyMetadata(default(SKPaint?), OnPropertyChanged));

        // --- PROPERTIES ---

        public Axis XAxis { get => (Axis)GetValue(XAxisProperty);
            set => SetValue(XAxisProperty, value);
        }

        public Axis YAxis { get => (Axis)GetValue(YAxisProperty);
            set => SetValue(YAxisProperty, value);
        }

        public SKPaint? AxisSKPaint { get => (SKPaint?)GetValue(AxisSKPaintProperty);
            set => SetValue(AxisSKPaintProperty, value);
        }

        public SKPaint? GridLineSKPaint { get => (SKPaint?)GetValue(GridLineSKPaintProperty);
            set => SetValue(GridLineSKPaintProperty, value);
        }

        public AvaloniaList<CartesianSeries> Series { get; } = new AvaloniaList<CartesianSeries>();

        public CartesianChart()
        {
            // Initialize default axes
            XAxis = new Axis { Position = AxisPosition.Bottom, Title = "X Axis" };
            YAxis = new Axis { Position = AxisPosition.Left, Title = "Y Axis" };

            // Listen to series collection changes to trigger redraws
            Series.CollectionChanged += OnSeriesCollectionChanged;
            
            PointerPressed += OnPointerPressedInternal;
            PointerMoved += OnPointerMovedInternal;
            PointerReleased += OnPointerReleasedInternal;
            PointerWheelChanged += OnPointerWheelChangedInternal;
        }

        private void OnSeriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (CartesianSeries oldSeries in e.OldItems)
                {
                    oldSeries.SeriesChanged -= OnSeriesChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (CartesianSeries newSeries in e.NewItems)
                {
                    newSeries.SeriesChanged += OnSeriesChanged;
                }
            }
            
            // Trigger redrawing and potentially run animations
            if (IsAnimationEnabled) StartEntryAnimation();
            else InvalidateVisual();
        }

        private void OnSeriesChanged(object? sender, EventArgs e)
        {
            InvalidateVisual();
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double top = 16;
            double bottom = 16;
            double left = 24;
            double right = 24;

            // X-Axis padding
            if (XAxis != null && XAxis.Position == AxisPosition.Top)
            {
                top = 48;
                if (!string.IsNullOrEmpty(XAxis.Title))
                {
                    top += 20;
                }
            }
            else
            {
                bottom = 48;
                if (XAxis != null && !string.IsNullOrEmpty(XAxis.Title))
                {
                    bottom += 20;
                }
            }

            // Y-Axis padding
            if (YAxis != null && YAxis.Position == AxisPosition.Right)
            {
                right = 64;
                if (!string.IsNullOrEmpty(YAxis.Title))
                {
                    right += 20;
                }
            }
            else
            {
                left = 64;
                if (YAxis != null && !string.IsNullOrEmpty(YAxis.Title))
                {
                    left += 20;
                }
            }

            // General title padding always at the top
            if (!string.IsNullOrEmpty(Title))
            {
                top += 28;
            }

            double w = Math.Max(10, bounds.Width - left - right);
            double h = Math.Max(10, bounds.Height - top - bottom);

            return new Rect(left, top, w, h);
        }

        private double _zoomScaleX = 1.0;
        private double _zoomOffsetX = 0.0;
        private double _zoomScaleY = 1.0;
        private double _zoomOffsetY = 0.0;

        private Point? _panStartPoint;
        private double _panStartOffsetX;
        private double _panStartOffsetY;
        private bool _isPanning;

        private void OnPointerPressedInternal(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(this).Position;
            if (EffectivePlotArea.Contains(pt))
            {
                var props = e.GetCurrentPoint(this).Properties;
                if (props.IsMiddleButtonPressed || props.IsRightButtonPressed || props.IsLeftButtonPressed)
                {
                    _panStartPoint = pt;
                    _panStartOffsetX = _zoomOffsetX;
                    _panStartOffsetY = _zoomOffsetY;
                    _isPanning = true;
                    CapturePointer(e.Pointer);
                }
            }
        }

        private void OnPointerMovedInternal(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_isPanning && _panStartPoint.HasValue)
            {
                var pt = e.GetCurrentPoint(this).Position;
                double deltaX = pt.X - _panStartPoint.Value.X;
                double deltaY = pt.Y - _panStartPoint.Value.Y;

                var (xMin, xMax, yMin, yMax) = ScanBounds();
                double axMin = double.IsNaN(XAxis.Minimum ?? double.NaN) ? xMin : XAxis.Minimum!.Value;
                double axMax = double.IsNaN(XAxis.Maximum ?? double.NaN) ? xMax : XAxis.Maximum!.Value;
                double ayMin = double.IsNaN(YAxis.Minimum ?? double.NaN) ? yMin : YAxis.Minimum!.Value;
                double ayMax = double.IsNaN(YAxis.Maximum ?? double.NaN) ? yMax : YAxis.Maximum!.Value;

                double xRange = axMax - axMin;
                double yRange = ayMax - ayMin;

                double deltaXData = -(deltaX / EffectivePlotArea.Width) * (xRange / _zoomScaleX);
                double deltaYData = (deltaY / EffectivePlotArea.Height) * (yRange / _zoomScaleY);

                _zoomOffsetX = _panStartOffsetX + deltaXData;
                _zoomOffsetY = _panStartOffsetY + deltaYData;

                InvalidateVisual();
            }
        }

        private void OnPointerReleasedInternal(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                _panStartPoint = null;
                ReleasePointerCapture(e.Pointer);
            }
        }

        private void OnPointerWheelChangedInternal(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(this).Position;
            if (!EffectivePlotArea.Contains(pt)) return;

            var (xMin, xMax, yMin, yMax) = ScanBounds();
            double axMin = double.IsNaN(XAxis.Minimum ?? double.NaN) ? xMin : XAxis.Minimum!.Value;
            double axMax = double.IsNaN(XAxis.Maximum ?? double.NaN) ? xMax : XAxis.Maximum!.Value;
            
            double xRange = axMax - axMin;
            var wheelDelta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
            double zoomFactor = wheelDelta > 0 ? 1.15 : 0.85;
            
            double ratio = (pt.X - EffectivePlotArea.Left) / EffectivePlotArea.Width;
            double cursorVal = (axMin + _zoomOffsetX) + (xRange / _zoomScaleX) * ratio;

            double oldScale = _zoomScaleX;
            _zoomScaleX = Math.Clamp(_zoomScaleX * zoomFactor, 1.0, 100.0);

            _zoomOffsetX = cursorVal - (xRange / _zoomScaleX) * ratio - axMin;
            
            if (_zoomScaleX <= 1.0)
            {
                _zoomScaleX = 1.0;
                _zoomOffsetX = 0.0;
            }

            InvalidateVisual();
        }

        protected override void DrawTooltip(SKCanvas context, Point mousePoint)
        {
            var activeSeriesList = Series.Where(s => s.IsVisible).ToList();
            if (activeSeriesList.Count == 0) return;

            var (xMin, xMax, yMin, yMax) = ScanBounds();
            double axMin = double.IsNaN(XAxis.Minimum ?? double.NaN) ? xMin : XAxis.Minimum!.Value;
            double axMax = double.IsNaN(XAxis.Maximum ?? double.NaN) ? xMax : XAxis.Maximum!.Value;
            double ayMin = double.IsNaN(YAxis.Minimum ?? double.NaN) ? yMin : YAxis.Minimum!.Value;
            double ayMax = double.IsNaN(YAxis.Maximum ?? double.NaN) ? yMax : YAxis.Maximum!.Value;

            double currentXMin = axMin + _zoomOffsetX;
            double currentXMax = axMin + (axMax - axMin) / _zoomScaleX + _zoomOffsetX;
            double currentYMin = ayMin + _zoomOffsetY;
            double currentYMax = ayMin + (ayMax - ayMin) / _zoomScaleY + _zoomOffsetY;

            var transform = new CoordinateTransform(
                EffectivePlotArea,
                currentXMin, currentXMax,
                currentYMin, currentYMax,
                XAxis.IsLogarithmic, YAxis.IsLogarithmic,
                XAxis.IsReversed, YAxis.IsReversed);

            double dataX = transform.ToData(mousePoint).X;

            var allPoints = activeSeriesList.SelectMany(s => s.GetDataPoints()).ToList();
            if (allPoints.Count == 0) return;

            double targetX = allPoints.OrderBy(p => Math.Abs(p.X - dataX)).First().X;

            var screenTargetPt = transform.ToScreen(targetX, currentYMin);
            var crossSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#40FFFFFF")), 1.0, new DashStyle(new[] { 4.0, 4.0 }, 0.0));
            context.DrawLine(crossSKPaint, new Point(screenTargetPt.X, EffectivePlotArea.Top), new Point(screenTargetPt.X, EffectivePlotArea.Bottom));

            var activePalette = Palette ?? Palette.Default;
            var tooltipItems = new List<(string Name, double Value, SKPaint SKColor)>();

            for (int i = 0; i < Series.Count; i++)
            {
                var series = Series[i];
                if (!series.IsVisible) continue;

                var points = series.GetDataPoints();
                var ptIndex = -1;
                for (int k = 0; k < points.Count; k++)
                {
                    if (Math.Abs(points[k].X - targetX) < 1e-5)
                    {
                        ptIndex = k;
                        break;
                    }
                }
                if (ptIndex != -1 && !double.IsNaN(points[ptIndex].Y))
                {
                    var pt = points[ptIndex];
                    var seriesSKPaint = series.Fill ?? activePalette.GetSKPaint(i);
                    tooltipItems.Add((string.IsNullOrEmpty(series.Title) ? $"Series {i + 1}" : series.Title!, pt.Y, seriesSKPaint));

                    var screenPt = transform.ToScreen(pt.X, pt.Y);
                    var pulseSKPaint = new Pen(seriesSKPaint, 1.5);
                    SKColor brushSKColor = SKColors.Purple;
                    if (seriesSKPaint is SolidSKColorSKPaint scb) brushSKColor = scb.SKColor;
                    var pulseFill = new SolidSKColorSKPaint(new SKColor((byte)(brushSKColor.Red), (byte)(brushSKColor.Green), (byte)(brushSKColor.Blue), (byte)(40)));
                    
                    context.DrawEllipse(seriesSKPaint, null, screenPt, 4.0, 4.0);
                    context.DrawEllipse(pulseFill, pulseSKPaint, screenPt, 8.0, 8.0);
                }
            }

            if (tooltipItems.Count == 0) return;

            double padding = 12;
            double titleHeight = 20;
            double rowHeight = 18;
            double tooltipWidth = 160;
            double tooltipHeight = padding * 2 + titleHeight + tooltipItems.Count * rowHeight;

            var titleFont = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            var textFont = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var textSKPaint = SKPaintes.White;

            var ftTitle = new FormattedText(
                $"X Value: {targetX:G4}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                titleFont,
                11,
                textSKPaint);
            
            tooltipWidth = Math.Max(tooltipWidth, ftTitle.Width + padding * 2);

            var formattedRows = new List<(FormattedText Text, SKPaint SKColor)>();
            foreach (var item in tooltipItems)
            {
                var ftRow = new FormattedText(
                    $"{item.Name}: {item.Value:N2}",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    textFont,
                    11,
                    textSKPaint);
                formattedRows.Add((ftRow, item.SKColor));
                tooltipWidth = Math.Max(tooltipWidth, ftRow.Width + padding * 2 + 15);
            }

            double tx = mousePoint.X + 15;
            double ty = mousePoint.Y + 15;

            if (tx + tooltipWidth > EffectivePlotArea.Right)
            {
                tx = mousePoint.X - tooltipWidth - 15;
            }
            if (ty + tooltipHeight > EffectivePlotArea.Bottom)
            {
                ty = mousePoint.Y - tooltipHeight - 15;
            }
            
            tx = Math.Max(EffectivePlotArea.Left, tx);
            ty = Math.Max(EffectivePlotArea.Top, ty);

            var tooltipRect = new Rect(tx, ty, tooltipWidth, tooltipHeight);
            var bgSKPaint = new SolidSKColorSKPaint(SKColor.Parse("#E81F242E"));
            var borderSKPaint = new Pen(new SolidSKColorSKPaint(SKColor.Parse("#30FFFFFF")), 1.0);
            
            context.DrawRectangle(bgSKPaint, borderSKPaint, new RoundedRect(tooltipRect, new CornerRadius(6.0)));

            double curX = tx + padding;
            double curY = ty + padding;

            context.DrawText(ftTitle, new Point(curX, curY));
            curY += titleHeight;

            foreach (var row in formattedRows)
            {
                context.DrawEllipse(row.SKColor, null, new Point(curX + 4, curY + 6), 3.0, 3.0);
                context.DrawText(row.Text, new Point(curX + 14, curY));
                curY += rowHeight;
            }
        }

        protected override void RenderChart(SKCanvas context)
        {
            if (Series.Count == 0)
            {
                RenderEmptyState(context);
                return;
            }

            var (xMin, xMax, yMin, yMax) = ScanBounds();

            if (XAxis.Minimum.HasValue) xMin = XAxis.Minimum.Value;
            if (XAxis.Maximum.HasValue) xMax = XAxis.Maximum.Value;
            if (YAxis.Minimum.HasValue) yMin = YAxis.Minimum.Value;
            if (YAxis.Maximum.HasValue) yMax = YAxis.Maximum.Value;

            double currentXMin = xMin + _zoomOffsetX;
            double currentXMax = xMin + (xMax - xMin) / _zoomScaleX + _zoomOffsetX;
            double currentYMin = yMin + _zoomOffsetY;
            double currentYMax = yMin + (yMax - yMin) / _zoomScaleY + _zoomOffsetY;

            var transform = new CoordinateTransform(
                EffectivePlotArea,
                currentXMin, currentXMax,
                currentYMin, currentYMax,
                XAxis.IsLogarithmic, YAxis.IsLogarithmic,
                XAxis.IsReversed, YAxis.IsReversed);

            RenderGridLinesAndAxes(context, transform);

            var activePalette = Palette ?? Palette.Default;
            for (int i = 0; i < Series.Count; i++)
            {
                var series = Series[i];
                if (!series.IsVisible) continue;

                var defaultSKPaint = activePalette.GetSKPaint(i);
                var renderContext = new SeriesRenderContext(
                    this,
                    context,
                    transform,
                    AnimationProgress,
                    EffectivePlotArea,
                    defaultSKPaint);

                series.RenderSeries(renderContext);
            }
        }

        private (double xMin, double xMax, double yMin, double yMax) ScanBounds()
        {
            double xMin = double.MaxValue;
            double xMax = double.MinValue;
            double yMin = double.MaxValue;
            double yMax = double.MinValue;

            bool hasData = false;

            foreach (var series in Series)
            {
                if (!series.IsVisible) continue;

                var points = series.GetDataPoints();
                if (points == null || points.Count == 0) continue;

                foreach (var pt in points)
                {
                    if (double.IsNaN(pt.X) || double.IsInfinity(pt.X) ||
                        double.IsNaN(pt.Y) || double.IsInfinity(pt.Y))
                    {
                        continue; // Skip invalid points
                    }

                    xMin = Math.Min(xMin, pt.X);
                    xMax = Math.Max(xMax, pt.X);
                    yMin = Math.Min(yMin, pt.Y);
                    yMax = Math.Max(yMax, pt.Y);
                    hasData = true;
                }
            }

            if (!hasData)
            {
                return (0.0, 10.0, 0.0, 10.0);
            }

            // Add subtle padding to Y bounds to prevent values touching margins
            double yRange = yMax - yMin;
            if (yRange < 1e-9) yRange = 1.0;
            yMin -= yRange * 0.05;
            yMax += yRange * 0.05;

            // Prevent X min/max matching
            if (Math.Abs(xMax - xMin) < 1e-9)
            {
                xMin -= 0.5;
                xMax += 0.5;
            }

            return (xMin, xMax, yMin, yMax);
        }

        private void RenderGridLinesAndAxes(SKCanvas context, CoordinateTransform transform)
        {
            var axisSKPaint = new Pen(AxisSKPaint ?? SystemSKPaint, 1.0);
            
            // Light grey dashed line for grid lines
            var gridLineSKPaint = GridLineSKPaint ?? new SolidSKColorSKPaint(
                IsDarkTheme ? SKColor.Parse("#2A2E35") : SKColor.Parse("#E2E8F0"));
            var gridSKPaint = new Pen(gridLineSKPaint, 1.0, new DashStyle(new[] { 4.0, 4.0 }, 0.0));

            var textSKPaint = LabelForeground ?? SystemSKPaint;
            var font = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);

            // --- DRAW Y AXIS & GRIDLINES ---
            if (YAxis.IsVisible)
            {
                double[] yTicks = YAxis.GetTicks(transform.YMin, transform.YMax);
                foreach (var val in yTicks)
                {
                    var pt = transform.ToScreen(transform.XMin, val);
                    
                    // Gridline - check X-axis active line position to avoid overlaps
                    double xAxisLineY = XAxis.Position == AxisPosition.Top ? EffectivePlotArea.Top : EffectivePlotArea.Bottom;
                    if (Math.Abs(pt.Y - xAxisLineY) > 1e-3)
                    {
                        context.DrawLine(gridSKPaint, new Point(EffectivePlotArea.Left, pt.Y), new Point(EffectivePlotArea.Right, pt.Y));
                    }

                    // Tick label
                    string labelStr = val.ToString(YAxis.LabelFormat);
                    var ft = new FormattedText(
                        labelStr,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        font,
                        11,
                        textSKPaint);

                    double lx = YAxis.Position == AxisPosition.Right ? EffectivePlotArea.Right + 8 : EffectivePlotArea.Left - ft.Width - 8;
                    double ly = pt.Y - ft.Height / 2.0;
                    context.DrawText(ft, new Point(lx, ly));
                }

                // Y-Axis main line
                if (YAxis.Position == AxisPosition.Right)
                {
                    context.DrawLine(axisSKPaint, new Point(EffectivePlotArea.Right, EffectivePlotArea.Top), new Point(EffectivePlotArea.Right, EffectivePlotArea.Bottom));
                }
                else
                {
                    context.DrawLine(axisSKPaint, new Point(EffectivePlotArea.Left, EffectivePlotArea.Top), new Point(EffectivePlotArea.Left, EffectivePlotArea.Bottom));
                }

                // Draw Y-Axis Title
                if (!string.IsNullOrEmpty(YAxis.Title))
                {
                    var ftTitle = new FormattedText(
                        YAxis.Title,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(font.FontFamily, font.FontStyle, FontWeight.Bold),
                        12,
                        textSKPaint);

                    // Draw vertical text rotated by -90 degrees
                    double xTranslate = YAxis.Position == AxisPosition.Right ? Bounds.Width - 16 : 16;
                    using (context.PushTransform(Matrix.CreateRotation(-Math.PI / 2.0) * Matrix.CreateTranslation(xTranslate, EffectivePlotArea.Center.Y + ftTitle.Width / 2.0)))
                    {
                        context.DrawText(ftTitle, new Point(0, 0));
                    }
                }
            }

            // --- DRAW X AXIS & GRIDLINES ---
            if (XAxis.IsVisible)
            {
                double[] xTicks = XAxis.GetTicks(transform.XMin, transform.XMax);
                foreach (var val in xTicks)
                {
                    var pt = transform.ToScreen(val, transform.YMin);

                    // Gridline - check Y-axis active line position to avoid overlaps
                    double yAxisLineX = YAxis.Position == AxisPosition.Right ? EffectivePlotArea.Right : EffectivePlotArea.Left;
                    if (Math.Abs(pt.X - yAxisLineX) > 1e-3)
                    {
                        context.DrawLine(gridSKPaint, new Point(pt.X, EffectivePlotArea.Top), new Point(pt.X, EffectivePlotArea.Bottom));
                    }

                    // Tick label
                    string labelStr = val.ToString(XAxis.LabelFormat);
                    var ft = new FormattedText(
                        labelStr,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        font,
                        11,
                        textSKPaint);

                    double lx = pt.X - ft.Width / 2.0;
                    double ly = XAxis.Position == AxisPosition.Top ? EffectivePlotArea.Top - ft.Height - 6 : EffectivePlotArea.Bottom + 6;
                    context.DrawText(ft, new Point(lx, ly));
                }

                // X-Axis main line
                if (XAxis.Position == AxisPosition.Top)
                {
                    context.DrawLine(axisSKPaint, new Point(EffectivePlotArea.Left, EffectivePlotArea.Top), new Point(EffectivePlotArea.Right, EffectivePlotArea.Top));
                }
                else
                {
                    context.DrawLine(axisSKPaint, new Point(EffectivePlotArea.Left, EffectivePlotArea.Bottom), new Point(EffectivePlotArea.Right, EffectivePlotArea.Bottom));
                }

                // Draw X-Axis Title
                if (!string.IsNullOrEmpty(XAxis.Title))
                {
                    var ftTitle = new FormattedText(
                        XAxis.Title,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(font.FontFamily, font.FontStyle, FontWeight.Bold),
                        12,
                        textSKPaint);

                    double lx = EffectivePlotArea.Left + (EffectivePlotArea.Width - ftTitle.Width) / 2.0;
                    double ly = XAxis.Position == AxisPosition.Top ? EffectivePlotArea.Top - ftTitle.Height - 24 : EffectivePlotArea.Bottom + 24;
                    context.DrawText(ftTitle, new Point(lx, ly));
                }
            }
        }

        private void RenderEmptyState(SKCanvas context)
        {
            var textSKPaint = LabelForeground ?? SystemSKPaint;
            var ft = new FormattedText(
                "No Data Available",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Italic, FontWeight.SemiBold),
                14,
                textSKPaint);

            double tx = EffectivePlotArea.Left + (EffectivePlotArea.Width - ft.Width) / 2.0;
            double ty = EffectivePlotArea.Top + (EffectivePlotArea.Height - ft.Height) / 2.0;
            context.DrawText(ft, new Point(tx, ty));
        }

        public override System.Collections.Generic.IEnumerable<ChartSeries> GetSeries() => Series;
    }
}