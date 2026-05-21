using Avalonia;
using Avalonia.Media;
using ProCharts.Avalonia.Controls;

namespace ProCharts.Avalonia.Maths
{
    public readonly struct SeriesRenderContext
    {
        public ChartBase Chart { get; }
        public DrawingContext DrawingContext { get; }
        public CoordinateTransform Transform { get; }
        public double AnimationProgress { get; }
        public Rect PlotArea { get; }
        public IBrush DefaultBrush { get; }

        public SeriesRenderContext(
            ChartBase chart,
            DrawingContext drawingContext,
            CoordinateTransform transform,
            double animationProgress,
            Rect plotArea,
            IBrush defaultBrush)
        {
            Chart = chart;
            DrawingContext = drawingContext;
            Transform = transform;
            AnimationProgress = animationProgress;
            PlotArea = plotArea;
            DefaultBrush = defaultBrush;
        }
    }
}
