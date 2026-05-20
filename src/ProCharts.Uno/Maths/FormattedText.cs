using System;
using System.Globalization;
using SkiaSharp;
using Microsoft.UI.Xaml;

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
        public SKPaint Paint { get; }
        public double Width { get; }
        public double Height { get; }

        public FormattedText(
            string text,
            CultureInfo culture,
            FlowDirection flowDirection,
            Typeface typeface,
            double fontSize,
            SKPaint paint)
        {
            Text = text ?? string.Empty;
            
            // Clone or configure the paint
            Paint = new SKPaint
            {
                Color = paint?.Color ?? SKColors.Black,
                TextSize = (float)fontSize,
                IsAntialias = true
            };
            
            if (typeface != null)
            {
                var weight = SKFontStyleWeight.Normal;
                if (typeface.FontWeight == FontWeight.Bold) weight = SKFontStyleWeight.Bold;
                else if (typeface.FontWeight == FontWeight.SemiBold) weight = SKFontStyleWeight.SemiBold;
                else if (typeface.FontWeight == FontWeight.Medium) weight = SKFontStyleWeight.Medium;
                else if (typeface.FontWeight == FontWeight.Light) weight = SKFontStyleWeight.Light;

                Paint.Typeface = SKTypeface.FromFamilyName(typeface.FontFamily, 
                    weight,
                    SKFontStyleWidth.Normal,
                    typeface.FontStyle == FontStyle.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
            }

            // Measure bounds
            SKRect textBounds = default;
            Paint.MeasureText(Text, ref textBounds);
            Width = textBounds.Width;
            Height = Math.Max(fontSize, textBounds.Height); // Use fontSize as fallback for height
        }
    }
}
