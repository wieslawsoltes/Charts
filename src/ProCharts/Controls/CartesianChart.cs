using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Styling;
using ProCharts.Components;
using ProCharts.Maths;
using ProCharts.Series;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class CartesianChart : ChartBase
    {
        // --- DEPENDENCY PROPERTIES ---

        public static readonly StyledProperty<Axis> XAxisProperty =
            AvaloniaProperty.Register<CartesianChart, Axis>(nameof(XAxis));

        public static readonly StyledProperty<Axis> YAxisProperty =
            AvaloniaProperty.Register<CartesianChart, Axis>(nameof(YAxis));

        public static readonly StyledProperty<IBrush?> AxisBrushProperty =
            AvaloniaProperty.Register<CartesianChart, IBrush?>(nameof(AxisBrush));

        public static readonly StyledProperty<IBrush?> GridLineBrushProperty =
            AvaloniaProperty.Register<CartesianChart, IBrush?>(nameof(GridLineBrush));

        // --- PROPERTIES ---

        public Axis XAxis
        {
            get => GetValue(XAxisProperty);
            set => SetValue(XAxisProperty, value);
        }

        public Axis YAxis
        {
            get => GetValue(YAxisProperty);
            set => SetValue(YAxisProperty, value);
        }

        public IBrush? AxisBrush
        {
            get => GetValue(AxisBrushProperty);
            set => SetValue(AxisBrushProperty, value);
        }

        public IBrush? GridLineBrush
        {
            get => GetValue(GridLineBrushProperty);
            set => SetValue(GridLineBrushProperty, value);
        }

        public AvaloniaList<CartesianSeries> Series { get; } = new AvaloniaList<CartesianSeries>();

        public CartesianChart()
        {
            // Initialize default axes
            XAxis = new Axis { Position = AxisPosition.Bottom, Title = "X Axis" };
            YAxis = new Axis { Position = AxisPosition.Left, Title = "Y Axis" };

            // Listen to series collection changes to trigger redraws
            Series.CollectionChanged += OnSeriesCollectionChanged;
            
            // Watch for changes that require redraws
            XAxisProperty.Changed.AddClassHandler<CartesianChart>((x, e) => x.InvalidateVisual());
            YAxisProperty.Changed.AddClassHandler<CartesianChart>((x, e) => x.InvalidateVisual());
            AxisBrushProperty.Changed.AddClassHandler<CartesianChart>((x, e) => x.InvalidateVisual());
            GridLineBrushProperty.Changed.AddClassHandler<CartesianChart>((x, e) => x.InvalidateVisual());
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
            double bottom = 48;
            double left = 64;
            double right = 24;

            if (!string.IsNullOrEmpty(Title))
            {
                top += 28;
            }

            if (XAxis != null && !string.IsNullOrEmpty(XAxis.Title))
            {
                bottom += 20; // Extra padding for X axis title
            }

            if (YAxis != null && !string.IsNullOrEmpty(YAxis.Title))
            {
                left += 20; // Extra padding for Y axis title
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

        protected override void OnPointerPressed(Avalonia.Input.PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            var pt = e.GetPosition(this);
            if (EffectivePlotArea.Contains(pt))
            {
                var props = e.GetCurrentPoint(this).Properties;
                if (props.IsMiddleButtonPressed || props.IsRightButtonPressed || props.IsLeftButtonPressed)
                {
                    _panStartPoint = pt;
                    _panStartOffsetX = _zoomOffsetX;
                    _panStartOffsetY = _zoomOffsetY;
                    _isPanning = true;
                    e.Pointer.Capture(this);
                }
            }
        }

        protected override void OnPointerMoved(Avalonia.Input.PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_isPanning && _panStartPoint.HasValue)
            {
                var pt = e.GetPosition(this);
                var delta = pt - _panStartPoint.Value;

                var (xMin, xMax, yMin, yMax) = ScanBounds();
                double axMin = double.IsNaN(XAxis.Minimum ?? double.NaN) ? xMin : XAxis.Minimum!.Value;
                double axMax = double.IsNaN(XAxis.Maximum ?? double.NaN) ? xMax : XAxis.Maximum!.Value;
                double ayMin = double.IsNaN(YAxis.Minimum ?? double.NaN) ? yMin : YAxis.Minimum!.Value;
                double ayMax = double.IsNaN(YAxis.Maximum ?? double.NaN) ? yMax : YAxis.Maximum!.Value;

                double xRange = axMax - axMin;
                double yRange = ayMax - ayMin;

                double deltaXData = -(delta.X / EffectivePlotArea.Width) * (xRange / _zoomScaleX);
                double deltaYData = (delta.Y / EffectivePlotArea.Height) * (yRange / _zoomScaleY);

                _zoomOffsetX = _panStartOffsetX + deltaXData;
                _zoomOffsetY = _panStartOffsetY + deltaYData;

                InvalidateVisual();
            }
        }

        protected override void OnPointerReleased(Avalonia.Input.PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (_isPanning)
            {
                _isPanning = false;
                _panStartPoint = null;
                e.Pointer.Capture(null);
            }
        }

        protected override void OnPointerWheelChanged(Avalonia.Input.PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            
            var pt = e.GetPosition(this);
            if (!EffectivePlotArea.Contains(pt)) return;

            var (xMin, xMax, yMin, yMax) = ScanBounds();
            double axMin = double.IsNaN(XAxis.Minimum ?? double.NaN) ? xMin : XAxis.Minimum!.Value;
            double axMax = double.IsNaN(XAxis.Maximum ?? double.NaN) ? xMax : XAxis.Maximum!.Value;
            
            double xRange = axMax - axMin;
            double zoomFactor = e.Delta.Y > 0 ? 1.15 : 0.85;
            
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

        protected override void DrawTooltip(DrawingContext context, Point mousePoint)
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
                XAxis.Position == AxisPosition.Top,
                YAxis.Position == AxisPosition.Right);

            double dataX = transform.ToData(mousePoint).X;

            var allPoints = activeSeriesList.SelectMany(s => s.GetDataPoints()).ToList();
            if (allPoints.Count == 0) return;

            double targetX = allPoints.OrderBy(p => Math.Abs(p.X - dataX)).First().X;

            var screenTargetPt = transform.ToScreen(targetX, currentYMin);
            var crossPen = new Pen(new SolidColorBrush(Color.Parse("#40FFFFFF")), 1.0, new DashStyle(new[] { 4.0, 4.0 }, 0.0));
            context.DrawLine(crossPen, new Point(screenTargetPt.X, EffectivePlotArea.Top), new Point(screenTargetPt.X, EffectivePlotArea.Bottom));

            var activePalette = Palette ?? Palette.Default;
            var tooltipItems = new List<(string Name, double Value, IBrush Color)>();

            for (int i = 0; i < Series.Count; i++)
            {
                var series = Series[i];
                if (!series.IsVisible) continue;

                var points = series.GetDataPoints();
                var pt = points.FirstOrDefault(p => Math.Abs(p.X - targetX) < 1e-5);
                if (pt != default && !double.IsNaN(pt.Y))
                {
                    var seriesBrush = series.Fill ?? activePalette.GetBrush(i);
                    tooltipItems.Add((string.IsNullOrEmpty(series.Title) ? $"Series {i + 1}" : series.Title!, pt.Y, seriesBrush));

                    var screenPt = transform.ToScreen(pt.X, pt.Y);
                    var pulsePen = new Pen(seriesBrush, 1.5);
                    Color brushColor = Colors.Purple;
                    if (seriesBrush is SolidColorBrush scb) brushColor = scb.Color;
                    var pulseFill = new SolidColorBrush(Color.FromArgb(40, brushColor.R, brushColor.G, brushColor.B));
                    
                    context.DrawEllipse(seriesBrush, null, screenPt, 4.0, 4.0);
                    context.DrawEllipse(pulseFill, pulsePen, screenPt, 8.0, 8.0);
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
            var textBrush = Brushes.White;

            var ftTitle = new FormattedText(
                $"X Value: {targetX:G4}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                titleFont,
                11,
                textBrush);
            
            tooltipWidth = Math.Max(tooltipWidth, ftTitle.Width + padding * 2);

            var formattedRows = new List<(FormattedText Text, IBrush Color)>();
            foreach (var item in tooltipItems)
            {
                var ftRow = new FormattedText(
                    $"{item.Name}: {item.Value:N2}",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    textFont,
                    11,
                    textBrush);
                formattedRows.Add((ftRow, item.Color));
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
            var bgBrush = new SolidColorBrush(Color.Parse("#E81F242E"));
            var borderPen = new Pen(new SolidColorBrush(Color.Parse("#30FFFFFF")), 1.0);
            
            context.DrawRectangle(bgBrush, borderPen, new RoundedRect(tooltipRect, new CornerRadius(6.0)));

            double curX = tx + padding;
            double curY = ty + padding;

            context.DrawText(ftTitle, new Point(curX, curY));
            curY += titleHeight;

            foreach (var row in formattedRows)
            {
                context.DrawEllipse(row.Color, null, new Point(curX + 4, curY + 6), 3.0, 3.0);
                context.DrawText(row.Text, new Point(curX + 14, curY));
                curY += rowHeight;
            }
        }

        protected override void RenderChart(DrawingContext context)
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
                XAxis.Position == AxisPosition.Top,
                YAxis.Position == AxisPosition.Right);

            RenderGridLinesAndAxes(context, transform);

            var activePalette = Palette ?? Palette.Default;
            for (int i = 0; i < Series.Count; i++)
            {
                var series = Series[i];
                if (!series.IsVisible) continue;

                var defaultBrush = activePalette.GetBrush(i);
                var renderContext = new SeriesRenderContext(
                    this,
                    context,
                    transform,
                    AnimationProgress,
                    EffectivePlotArea,
                    defaultBrush);

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

                hasData = true;
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

        private void RenderGridLinesAndAxes(DrawingContext context, CoordinateTransform transform)
        {
            var axisPen = new Pen(AxisBrush ?? SystemBrush, 1.0);
            
            // Light grey dashed line for grid lines
            var gridLineBrush = GridLineBrush ?? new SolidColorBrush(
                ActualThemeVariant == ThemeVariant.Dark ? Color.Parse("#2A2E35") : Color.Parse("#E2E8F0"));
            var gridPen = new Pen(gridLineBrush, 1.0, new DashStyle(new[] { 4.0, 4.0 }, 0.0));

            var textBrush = LabelForeground ?? SystemBrush;
            var font = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);

            // --- DRAW Y AXIS & GRIDLINES ---
            if (YAxis.IsVisible)
            {
                double[] yTicks = YAxis.GetTicks(transform.YMin, transform.YMax);
                foreach (var val in yTicks)
                {
                    var pt = transform.ToScreen(transform.XMin, val);
                    
                    // Gridline
                    if (val != transform.YMin || YAxis.Position == AxisPosition.Right)
                    {
                        context.DrawLine(gridPen, new Point(EffectivePlotArea.Left, pt.Y), new Point(EffectivePlotArea.Right, pt.Y));
                    }

                    // Tick label
                    string labelStr = val.ToString(YAxis.LabelFormat);
                    var ft = new FormattedText(
                        labelStr,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        font,
                        11,
                        textBrush);

                    double lx = EffectivePlotArea.Left - ft.Width - 8;
                    double ly = pt.Y - ft.Height / 2.0;
                    context.DrawText(ft, new Point(lx, ly));
                }

                // Left Y-Axis main line
                context.DrawLine(axisPen, new Point(EffectivePlotArea.Left, EffectivePlotArea.Top), new Point(EffectivePlotArea.Left, EffectivePlotArea.Bottom));

                // Draw Y-Axis Title
                if (!string.IsNullOrEmpty(YAxis.Title))
                {
                    var ftTitle = new FormattedText(
                        YAxis.Title,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(font.FontFamily, font.Style, FontWeight.Bold),
                        12,
                        textBrush);

                    // Draw vertical text rotated by -90 degrees
                    using (context.PushTransform(Matrix.CreateRotation(-Math.PI / 2.0) * Matrix.CreateTranslation(16, EffectivePlotArea.Center.Y + ftTitle.Width / 2.0)))
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

                    // Gridline
                    if (val != transform.XMin || XAxis.Position == AxisPosition.Top)
                    {
                        context.DrawLine(gridPen, new Point(pt.X, EffectivePlotArea.Top), new Point(pt.X, EffectivePlotArea.Bottom));
                    }

                    // Tick label
                    string labelStr = val.ToString(XAxis.LabelFormat);
                    var ft = new FormattedText(
                        labelStr,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        font,
                        11,
                        textBrush);

                    double lx = pt.X - ft.Width / 2.0;
                    double ly = EffectivePlotArea.Bottom + 6;
                    context.DrawText(ft, new Point(lx, ly));
                }

                // Bottom X-Axis main line
                context.DrawLine(axisPen, new Point(EffectivePlotArea.Left, EffectivePlotArea.Bottom), new Point(EffectivePlotArea.Right, EffectivePlotArea.Bottom));

                // Draw X-Axis Title
                if (!string.IsNullOrEmpty(XAxis.Title))
                {
                    var ftTitle = new FormattedText(
                        XAxis.Title,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(font.FontFamily, font.Style, FontWeight.Bold),
                        12,
                        textBrush);

                    double lx = EffectivePlotArea.Left + (EffectivePlotArea.Width - ftTitle.Width) / 2.0;
                    double ly = EffectivePlotArea.Bottom + 24;
                    context.DrawText(ftTitle, new Point(lx, ly));
                }
            }
        }

        private void RenderEmptyState(DrawingContext context)
        {
            var textBrush = LabelForeground ?? SystemBrush;
            var ft = new FormattedText(
                "No Data Available",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Italic, FontWeight.SemiBold),
                14,
                textBrush);

            double tx = EffectivePlotArea.Left + (EffectivePlotArea.Width - ft.Width) / 2.0;
            double ty = EffectivePlotArea.Top + (EffectivePlotArea.Height - ft.Height) / 2.0;
            context.DrawText(ft, new Point(tx, ty));
        }

        public override System.Collections.Generic.IEnumerable<ChartSeries> GetSeries() => Series;
    }
}
