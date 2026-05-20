using Microsoft.UI.Xaml;
using Windows.Foundation;
using SkiaSharp;
using ProCharts.Uno.Controls;

namespace ProCharts.Uno.Maths
{
    public readonly struct SeriesRenderContext
    {
        public ChartBase Chart { get; }
        public SKCanvas Canvas { get; }
        public CoordinateTransform Transform { get; }
        public double AnimationProgress { get; }
        public SKRect PlotArea { get; }
        public SKPaint DefaultPaint { get; }
        public SKPaint DefaultSKPaint => DefaultPaint;

        public SeriesRenderContext(
            ChartBase chart,
            SKCanvas context,
            CoordinateTransform transform,
            double animationProgress,
            SKRect plotArea,
            SKPaint defaultPaint)
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