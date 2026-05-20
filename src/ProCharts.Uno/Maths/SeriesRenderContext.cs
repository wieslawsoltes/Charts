using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using ProCharts.Uno.Controls;

namespace ProCharts.Uno.Maths
{
    public readonly struct SeriesRenderContext
    {
        public ChartBase Chart { get; }
        public DrawingContext Canvas { get; }
        public CoordinateTransform Transform { get; }
        public double AnimationProgress { get; }
        public Rect PlotArea { get; }
        public Brush DefaultPaint { get; }
        public Brush DefaultSKPaint => DefaultPaint;

        public SeriesRenderContext(
            ChartBase chart,
            DrawingContext context,
            CoordinateTransform transform,
            double animationProgress,
            Rect plotArea,
            Brush defaultPaint)
        {
            Chart = chart;
            Canvas = context;
            Transform = transform;
            AnimationProgress = animationProgress;
            PlotArea = plotArea;
            DefaultPaint = defaultPaint;
        }
    }
}