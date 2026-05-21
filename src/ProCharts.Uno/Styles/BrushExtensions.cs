using Microsoft.UI.Xaml.Media;

namespace ProCharts.Uno.Styles
{
    public static class BrushExtensions
    {
        public static Color GetColor(this Brush? brush)
        {
            if (brush is SolidColorBrush scb)
            {
                return scb.Color;
            }
            if (brush is LinearGradientBrush lgb && lgb.GradientStops.Count > 0)
            {
                return lgb.GradientStops[0].Color;
            }
            return Microsoft.UI.Colors.Transparent;
        }
    }
}
