using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Styling;
using ProCharts.Avalonia.Maths;
using ProCharts.Avalonia.Styles;
using ProCharts.Avalonia.Series;

namespace ProCharts.Avalonia.Controls
{
    public abstract class ChartBase : TemplatedControl
    {
        // --- DEPENDENCY PROPERTIES ---

        public static readonly StyledProperty<string?> TitleProperty =
            AvaloniaProperty.Register<ChartBase, string?>(nameof(Title));

        public static readonly StyledProperty<Palette?> PaletteProperty =
            AvaloniaProperty.Register<ChartBase, Palette?>(nameof(Palette));

        public static readonly StyledProperty<IBrush?> LabelForegroundProperty =
            AvaloniaProperty.Register<ChartBase, IBrush?>(nameof(LabelForeground));

        public static readonly StyledProperty<IBrush?> PlotAreaBackgroundProperty =
            AvaloniaProperty.Register<ChartBase, IBrush?>(nameof(PlotAreaBackground));

        public static readonly StyledProperty<Control?> PlotAreaContentProperty =
            AvaloniaProperty.Register<ChartBase, Control?>(nameof(PlotAreaContent));

        public static readonly StyledProperty<bool> IsAnimationEnabledProperty =
            AvaloniaProperty.Register<ChartBase, bool>(nameof(IsAnimationEnabled), true);

        public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
            AvaloniaProperty.Register<ChartBase, TimeSpan>(nameof(AnimationDuration), TimeSpan.FromSeconds(1));

        public static readonly StyledProperty<EasingType> EasingProperty =
            AvaloniaProperty.Register<ChartBase, EasingType>(nameof(Easing), EasingType.CubicEaseOut);

        public static readonly DirectProperty<ChartBase, double> AnimationProgressProperty =
            AvaloniaProperty.RegisterDirect<ChartBase, double>(
                nameof(AnimationProgress),
                o => o.AnimationProgress,
                (o, v) => o.AnimationProgress = v);

        // --- PROPERTIES ---

        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Palette? Palette
        {
            get => GetValue(PaletteProperty);
            set => SetValue(PaletteProperty, value);
        }

        public IBrush? LabelForeground
        {
            get => GetValue(LabelForegroundProperty);
            set => SetValue(LabelForegroundProperty, value);
        }

        public IBrush? PlotAreaBackground
        {
            get => GetValue(PlotAreaBackgroundProperty);
            set => SetValue(PlotAreaBackgroundProperty, value);
        }

        public Control? PlotAreaContent
        {
            get => GetValue(PlotAreaContentProperty);
            set => SetValue(PlotAreaContentProperty, value);
        }

        public bool IsAnimationEnabled
        {
            get => GetValue(IsAnimationEnabledProperty);
            set => SetValue(IsAnimationEnabledProperty, value);
        }

        public TimeSpan AnimationDuration
        {
            get => GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public EasingType Easing
        {
            get => GetValue(EasingProperty);
            set => SetValue(EasingProperty, value);
        }

        private double _animationProgress = 1.0;
        public double AnimationProgress
        {
            get => _animationProgress;
            protected set => SetAndRaise(AnimationProgressProperty, ref _animationProgress, value);
        }

        // --- INTERNAL FIELDS ---

        protected Rect EffectivePlotArea { get; set; }
        private DispatcherTimer? _animationTimer;
        private Stopwatch? _animationStopwatch;
        protected Point? MousePoint => _mousePoint;
        private Point? _mousePoint;

        static ChartBase()
        {
            // Set default styling and properties
            ClipToBoundsProperty.OverrideDefaultValue<ChartBase>(false);
            BackgroundProperty.OverrideDefaultValue<ChartBase>(Brushes.Transparent);
        }

        protected ChartBase()
        {
            // Watch for changes that require redraws
            TitleProperty.Changed.AddClassHandler<ChartBase>((x, e) => x.InvalidateVisual());
            PaletteProperty.Changed.AddClassHandler<ChartBase>((x, e) => x.InvalidateVisual());
            PlotAreaBackgroundProperty.Changed.AddClassHandler<ChartBase>((x, e) => x.InvalidateVisual());
            
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            
            // Interactive pointer event registration
            PointerMoved += (s, e) =>
            {
                var pt = e.GetPosition(this);
                var bounds = Bounds.Size;
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

        protected virtual void UpdateHoverState(Point? mousePoint)
        {
        }

        private void OnLoaded(object? sender, EventArgs e)
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

        private void OnUnloaded(object? sender, EventArgs e)
        {
            StopAnimation();
        }

        public void StartEntryAnimation()
        {
            StopAnimation();

            AnimationProgress = 0.0;
            _animationStopwatch = Stopwatch.StartNew();
            
            _animationTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(16), // ~60fps
                DispatcherPriority.Render,
                (s, e) => UpdateAnimationStep());
            
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
                AnimationProgress = ProCharts.Avalonia.Maths.Easing.Interpolate(t, Easing);
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
            // Simple default sizing
            double width = double.IsNaN(Width) ? 300 : Width;
            double height = double.IsNaN(Height) ? 200 : Height;

            if (!double.IsInfinity(availableSize.Width)) width = availableSize.Width;
            if (!double.IsInfinity(availableSize.Height)) height = availableSize.Height;

            return new Size(Math.Max(100, width), Math.Max(100, height));
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == AnimationProgressProperty)
            {
                InvalidateVisual();
            }
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

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var bounds = Bounds.Size;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            // 1. Calculate boundaries
            EffectivePlotArea = CalculatePlotArea(bounds);

            // 2. Draw Title
            DrawTitle(context, bounds);

            // 3. Draw Plot Area Background
            var bg = PlotAreaBackground;
            if (bg != null)
            {
                context.FillRectangle(bg, EffectivePlotArea);
            }

            // 4. Custom derived rendering (Gridlines, Axes, Series)
            RenderChart(context);

            // 5. Draw dynamic interactive Tooltip on top
            if (_mousePoint.HasValue)
            {
                DrawTooltip(context, _mousePoint.Value);
            }
        }

        protected virtual void DrawTitle(DrawingContext context, Size bounds)
        {
            if (string.IsNullOrEmpty(Title)) return;

            var foreground = LabelForeground ?? SystemBrush;
            var formattedText = new FormattedText(
                Title,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold),
                16,
                foreground);

            double tx = (bounds.Width - formattedText.Width) / 2.0; // Centered
            double ty = 12;

            context.DrawText(formattedText, new Point(tx, ty));
        }

        protected abstract void RenderChart(DrawingContext context);

        protected virtual void DrawTooltip(DrawingContext context, Point mousePoint)
        {
        }

        /// <summary>
        /// Unified access to all series in the chart.
        /// </summary>
        public virtual System.Collections.Generic.IEnumerable<ChartSeries> GetSeries() => Array.Empty<ChartSeries>();

        protected IBrush SystemBrush => LabelForeground ?? 
            (ActualThemeVariant == ThemeVariant.Dark ? Brushes.White : Brushes.Black);
    }
}
