using System;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace ProCharts.Uno.Maths
{
    public enum FontStyle
    {
        Normal,
        Italic
    }

    public enum FontWeight
    {
        Normal,
        Bold,
        SemiBold,
        Medium,
        Light
    }

    public class Typeface
    {
        public string FontFamily { get; }
        public FontStyle FontStyle { get; }
        public FontWeight FontWeight { get; }

        public Typeface(string fontFamily, FontStyle fontStyle = FontStyle.Normal, FontWeight fontWeight = FontWeight.Normal)
        {
            FontFamily = fontFamily;
            FontStyle = fontStyle;
            FontWeight = fontWeight;
        }
    }

    public class FormattedText
    {
        public string Text { get; }
        public Brush Paint { get; }
        public double Width { get; }
        public double Height { get; }
        public Typeface? Typeface { get; }
        public double FontSize { get; }
        public FlowDirection FlowDirection { get; }

        public FormattedText(
            string text,
            CultureInfo culture,
            FlowDirection flowDirection,
            Typeface? typeface,
            double fontSize,
            Brush paint)
        {
            Text = text ?? string.Empty;
            Paint = paint;
            this.Typeface = typeface;
            FontSize = fontSize;
            FlowDirection = flowDirection;

            // Off-screen measure
            var tb = new TextBlock
            {
                Text = Text,
                FontSize = FontSize,
                FlowDirection = FlowDirection
            };

            if (Typeface != null)
            {
                tb.FontFamily = new FontFamily(Typeface.FontFamily);
                tb.FontStyle = Typeface.FontStyle == FontStyle.Italic ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal;

                switch (Typeface.FontWeight)
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

            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Width = tb.DesiredSize.Width;
            Height = tb.DesiredSize.Height > 0 ? tb.DesiredSize.Height : FontSize * 1.25;
        }
    }
}
