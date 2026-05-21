using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;

namespace ProCharts.Uno.Styles
{
    public class DashStyle
    {
        public IReadOnlyList<double> Dashes { get; }
        public double Offset { get; }

        public DashStyle(IEnumerable<double> dashes, double offset)
        {
            Dashes = new List<double>(dashes).AsReadOnly();
            Offset = offset;
        }

        public static DashStyle Dash => new DashStyle(new[] { 4.0, 4.0 }, 0.0);
    }

    public class Pen
    {
        public Brush Brush { get; }
        public double Thickness { get; }
        public DashStyle? Dash { get; }
        public PenLineCap LineCap { get; }
        public PenLineJoin LineJoin { get; }

        public Pen(Brush brush, double thickness = 1.0, DashStyle? dashStyle = null, PenLineCap lineCap = PenLineCap.Flat, PenLineJoin lineJoin = PenLineJoin.Miter)
        {
            Brush = brush ?? new SolidColorBrush(Microsoft.UI.Colors.Black);
            Thickness = thickness;
            Dash = dashStyle;
            LineCap = lineCap;
            LineJoin = lineJoin;
        }
    }
}
