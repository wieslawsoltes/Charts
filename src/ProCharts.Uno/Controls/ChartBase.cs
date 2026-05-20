using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Microsoft.UI.Dispatching;
using SkiaSharp;
using SkiaSharp.Views.Windows;

using ProCharts.Uno.Maths;
using ProCharts.Uno.Styles;
using ProCharts.Uno.Series;

namespace ProCharts.Uno.Controls
{
    public abstract partial class ChartBase : Grid
    {
        // --- CANVAS ---
        protected readonly SKXamlCanvas _canvas;

        // --- DEPENDENCY PROPERTIES ---

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ChartBase), new PropertyMetadata(default(string), OnPropertyChanged));

        public static readonly DependencyProperty PaletteProperty =
            DependencyProperty.Register(nameof(Palette), typeof(Palette), typeof(ChartBase), new PropertyMetadata(default(Palette), OnPropertyChanged));

        public static readonly DependencyProperty LabelForegroundProperty =
            DependencyProperty.Register(nameof(LabelForeground), typeof(SKPaint), typeof(ChartBase), new PropertyMetadata(default(SKPaint), OnPropertyChanged));

        public static readonly DependencyProperty PlotAreaBackgroundProperty =
            DependencyProperty.Register(nameof(PlotAreaBackground), typeof(SKPaint), typeof(ChartBase), new PropertyMetadata(default(SKPaint), OnPropertyChanged));

        public static readonly DependencyProperty PlotAreaContentProperty =
            DependencyProperty.Register(nameof(PlotAreaContent), typeof(Control), typeof(ChartBase), new PropertyMetadata(default(Control), OnPropertyChanged));

        public static readonly DependencyProperty IsAnimationEnabledProperty =
            DependencyProperty.Register(nameof(IsAnimationEnabled), typeof(bool), typeof(ChartBase), new PropertyMetadata(true, OnPropertyChanged));

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(nameof(AnimationDuration), typeof(TimeSpan), typeof(ChartBase), new PropertyMetadata(TimeSpan.FromSeconds(1), OnPropertyChanged));

        public static readonly DependencyProperty EasingProperty =
            DependencyProperty.Register(nameof(Easing), typeof(EasingType), typeof(ChartBase), new PropertyMetadata(EasingType.CubicEaseOut, OnPropertyChanged));

        public static readonly DependencyProperty AnimationProgressProperty =
            DependencyProperty.Register(nameof(AnimationProgress), typeof(double), typeof(ChartBase), new PropertyMetadata(1.0, OnPropertyChanged));

        // --- PROPERTIES ---

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Palette? Palette
        {
            get => (Palette?)GetValue(PaletteProperty);
            set => SetValue(PaletteProperty, value);
        }

        public SKPaint? LabelForeground
        {
            get => (SKPaint?)GetValue(LabelForegroundProperty);
            set => SetValue(LabelForegroundProperty, value);
        }

        public SKPaint? PlotAreaBackground
        {
            get => (SKPaint?)GetValue(PlotAreaBackgroundProperty);
            set => SetValue(PlotAreaBackgroundProperty, value);
        }

        public Control? PlotAreaContent
        {
            get => (Control?)GetValue(PlotAreaContentProperty);
            set => SetValue(PlotAreaContentProperty, value);
        }

        public bool IsAnimationEnabled
        {
            get => (bool)GetValue(IsAnimationEnabledProperty);
            set => SetValue(IsAnimationEnabledProperty, value);
        }

        public TimeSpan AnimationDuration
        {
            get => (TimeSpan)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public EasingType Easing
        {
            get => (EasingType)GetValue(EasingProperty);
            set => SetValue(EasingProperty, value);
        }

        public double AnimationProgress
        {
            get => (double)GetValue(AnimationProgressProperty);
            protected set => SetValue(AnimationProgressProperty, value);
        }

        // --- INTERNAL FIELDS ---

        protected Rect EffectivePlotArea { get; set; }
        public Rect Bounds => new Rect(0, 0, ActualWidth, ActualHeight);
        private DispatcherTimer? _animationTimer;
        private Stopwatch? _animationStopwatch;
        protected Point? MousePoint => _mousePoint;
        private Point? _mousePoint;

        protected static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChartBase chart)
            {
                chart.InvalidateVisual();
            }
        }

        protected ChartBase()
        {
            _canvas = new SKXamlCanvas();
            this.Children.Add(_canvas);
            _canvas.PaintSurface += OnPaintSurface;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            // Interactive pointer event registration
            PointerMoved += (s, e) =>
            {
                var pt = e.GetCurrentPoint(this).Position;
                var bounds = new Size(ActualWidth, ActualHeight);
                if (bounds.Width > 0 && bounds.Height > 0)
                {
                    EffectivePlotArea = CalculatePlotArea(bounds);
                }
                if (EffectivePlotArea.Contains(pt))
                {
                    _mousePoint = pt;
                }
                else
                {
                    _mousePoint = null;
                }
                UpdateHoverState(_mousePoint);
                InvalidateVisual();
            };

            PointerExited += (s, e) =>
            {
                _mousePoint = null;
                UpdateHoverState(null);
                InvalidateVisual();
            };
        }

        public void InvalidateVisual()
        {
            _canvas?.Invalidate();
        }

        protected virtual void UpdateHoverState(Point? mousePoint)
        {
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (IsAnimationEnabled)
            {
                StartEntryAnimation();
            }
            else
            {
                AnimationProgress = 1.0;
                InvalidateVisual();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopAnimation();
        }

        public void StartEntryAnimation()
        {
            StopAnimation();

            AnimationProgress = 0.0;
            _animationStopwatch = Stopwatch.StartNew();

            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
            _animationTimer.Tick += (s, e) => UpdateAnimationStep();
            _animationTimer.Start();
        }

        private void UpdateAnimationStep()
        {
            if (_animationStopwatch == null || _animationTimer == null) return;

            double elapsedMs = _animationStopwatch.ElapsedMilliseconds;
            double durationMs = AnimationDuration.TotalMilliseconds;

            if (durationMs <= 0)
            {
                AnimationProgress = 1.0;
                StopAnimation();
                InvalidateVisual();
                return;
            }

            double t = elapsedMs / durationMs;
            if (t >= 1.0)
            {
                AnimationProgress = 1.0;
                StopAnimation();
            }
            else
            {
                AnimationProgress = ProCharts.Uno.Maths.Easing.Interpolate(t, Easing);
            }

            InvalidateVisual();
        }

        private void StopAnimation()
        {
            _animationTimer?.Stop();
            _animationTimer = null;
            _animationStopwatch?.Stop();
            _animationStopwatch = null;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double width = double.IsNaN(Width) ? 300 : Width;
            double height = double.IsNaN(Height) ? 200 : Height;

            if (!double.IsInfinity(availableSize.Width)) width = availableSize.Width;
            if (!double.IsInfinity(availableSize.Height)) height = availableSize.Height;

            return new Size(Math.Max(100, width), Math.Max(100, height));
        }

        /// <summary>
        /// Calculates the inner plot area by subtracting space for titles, axes, and legends.
        /// </summary>
        protected virtual Rect CalculatePlotArea(Size bounds)
        {
            double top = 16;
            double bottom = 32;
            double left = 48;
            double right = 16;

            if (!string.IsNullOrEmpty(Title))
            {
                top += 28; // Title offset
            }

            double w = Math.Max(10, bounds.Width - left - right);
            double h = Math.Max(10, bounds.Height - top - bottom);

            return new Rect(left, top, w, h);
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            var bounds = new Size(ActualWidth, ActualHeight);
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            // 1. Calculate boundaries
            EffectivePlotArea = CalculatePlotArea(bounds);

            // 2. Draw Title
            DrawTitle(canvas, bounds);

            // 3. Draw Plot Area Background
            var bg = PlotAreaBackground;
            if (bg != null)
            {
                canvas.DrawRect((float)EffectivePlotArea.X, (float)EffectivePlotArea.Y, (float)EffectivePlotArea.Width, (float)EffectivePlotArea.Height, bg);
            }

            // 4. Custom derived rendering (Gridlines, Axes, Series)
            RenderChart(canvas);

            // 5. Draw dynamic interactive Tooltip on top
            if (_mousePoint.HasValue)
            {
                DrawTooltip(canvas, _mousePoint.Value);
            }
        }

        protected virtual void DrawTitle(SKCanvas context, Size bounds)
        {
            if (string.IsNullOrEmpty(Title)) return;

            var foreground = LabelForeground ?? SystemSKPaint;
            var formattedText = new FormattedText(
                Title,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                16,
                foreground);

            double tx = (bounds.Width - formattedText.Width) / 2.0; // Centered
            double ty = 12 + formattedText.Height;

            context.DrawText(formattedText.Text, (float)tx, (float)ty, formattedText.Paint);
        }

        protected abstract void RenderChart(SKCanvas context);

        protected virtual void DrawTooltip(SKCanvas context, Point mousePoint)
        {
        }

        /// <summary>
        /// Unified access to all series in the chart.
        /// </summary>
        public virtual System.Collections.Generic.IEnumerable<ChartSeries> GetSeries() => Array.Empty<ChartSeries>();

        protected SKPaint SystemSKPaint => LabelForeground ?? new SKPaint 
        { 
            Color = ActualTheme == ElementTheme.Dark ? SKColors.White : SKColors.Black,
            IsAntialias = true
        };
    }
}
