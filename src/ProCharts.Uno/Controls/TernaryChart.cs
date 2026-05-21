using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;
using ProCharts.Uno.Styles;

namespace ProCharts.Uno.Controls
{
    public partial class TernaryChart : ChartBase
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TernaryChart), new PropertyMetadata(default(IEnumerable?), OnPropertyChanged));

        public static readonly DependencyProperty APathProperty =
            DependencyProperty.Register(nameof(APath), typeof(string), typeof(TernaryChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty BPathProperty =
            DependencyProperty.Register(nameof(BPath), typeof(string), typeof(TernaryChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty CPathProperty =
            DependencyProperty.Register(nameof(CPath), typeof(string), typeof(TernaryChart), new PropertyMetadata(default(string?), OnPropertyChanged));

        public static readonly DependencyProperty MarkerBrushProperty =
            DependencyProperty.Register(nameof(MarkerBrush), typeof(Brush), typeof(TernaryChart), new PropertyMetadata(default(Brush?), OnPropertyChanged));

        public IEnumerable? ItemsSource
        {
            get => (System.Collections.IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string? APath { get => (string?)GetValue(APathProperty);
            set => SetValue(APathProperty, value);
        }

        public string? BPath { get => (string?)GetValue(BPathProperty);
            set => SetValue(BPathProperty, value);
        }

        public string? CPath { get => (string?)GetValue(CPathProperty);
            set => SetValue(CPathProperty, value);
        }

        public Brush? MarkerBrush { get => (Brush?)GetValue(MarkerBrushProperty);
            set => SetValue(MarkerBrushProperty, value);
        }

        public TernaryChart()
        {
        }

        private class TernaryPoint
        {
            public double A { get; set; } // Left vertex weight
            public double B { get; set; } // Right vertex weight
            public double C { get; set; } // Top vertex weight
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double padding = 36;
            if (!string.IsNullOrEmpty(Title)) padding += 28;

            double side = Math.Min(bounds.Width, bounds.Height) - padding * 2;
            side = Math.Max(10, side);

            // Center horizontally
            double cx = (bounds.Width - side) / 2.0;
            double cy = padding;

            return new Rect(cx, cy, side, side);
        }

        protected override void RenderChart(DrawingContext context)
        {
            var area = EffectivePlotArea;

            // Define Triangle Vertices (Equilateral: bottom-left, bottom-right, top-center)
            double hTriangle = area.Width * (Math.Sqrt(3.0) / 2.0);
            
            // Adjust vertical centering
            double yOffset = (area.Height - hTriangle) / 2.0;
            var vLeft = new Point(area.Left, area.Bottom - yOffset);
            var vRight = new Point(area.Right, area.Bottom - yOffset);
            var vTop = new Point(area.Left + area.Width / 2.0, area.Bottom - yOffset - hTriangle);

            var borderBrush = new Pen(new SolidColorBrush(Color.Parse("#475569")), 1.5); // slate-600
            var gridBrush = new Pen(new SolidColorBrush(new Color((byte)(148), (byte)(163), (byte)(184), (byte)(40))), 1.0); // thin gray-400
            var textBrush = SystemBrush;

            // 1. Draw Equilateral Grid Lines (10%, 20%, ..., 90%)
            for (int pct = 10; pct < 100; pct += 20)
            {
                double f = pct / 100.0;

                // Line parallel to bottom (constant C)
                var pBaseLeft = Interpolate(vLeft, vTop, f);
                var pBaseRight = Interpolate(vRight, vTop, f);
                context.DrawLine(gridBrush, pBaseLeft, pBaseRight);

                // Line parallel to left side (constant B)
                var pBLeft = Interpolate(vLeft, vRight, f);
                var pBRight = Interpolate(vTop, vRight, f);
                context.DrawLine(gridBrush, pBLeft, pBRight);

                // Line parallel to right side (constant A)
                var pALeft = Interpolate(vRight, vLeft, f);
                var pARight = Interpolate(vTop, vLeft, f);
                context.DrawLine(gridBrush, pALeft, pARight);
            }

            // 2. Draw Equilateral Triangle Outline
            context.DrawLine(borderBrush, vLeft, vRight);
            context.DrawLine(borderBrush, vRight, vTop);
            context.DrawLine(borderBrush, vTop, vLeft);

            // 3. Draw Corner Labels (A, B, C)
            var font = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            
            var ftA = new FormattedText("A (Left)", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, 11, textBrush);
            context.DrawText(ftA, new Point(vLeft.X - ftA.Width - 6, vLeft.Y - ftA.Height / 2.0));

            var ftB = new FormattedText("B (Right)", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, 11, textBrush);
            context.DrawText(ftB, new Point(vRight.X + 6, vRight.Y - ftB.Height / 2.0));

            var ftC = new FormattedText("C (Top)", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, 11, textBrush);
            context.DrawText(ftC, new Point(vTop.X - ftC.Width / 2.0, vTop.Y - ftC.Height - 6));

            // 4. Plot Points
            if (ItemsSource == null) return;

            var items = new List<TernaryPoint>();
            int idx = 0;
            foreach (var rawItem in ItemsSource)
            {
                if (rawItem == null) continue;

                var aObj = ResolvePropertyValue(rawItem, APath);
                var bObj = ResolvePropertyValue(rawItem, BPath);
                var cObj = ResolvePropertyValue(rawItem, CPath);

                double a = ConvertToDouble(aObj);
                double b = ConvertToDouble(bObj);
                double c = ConvertToDouble(cObj);

                if (double.IsNaN(a)) a = 0.0;
                if (double.IsNaN(b)) b = 0.0;
                if (double.IsNaN(c)) c = 0.0;

                // Normalize so that a + b + c = 1.0
                double sum = a + b + c;
                if (sum > 0)
                {
                    a /= sum;
                    b /= sum;
                    c /= sum;
                }
                else
                {
                    a = 0.33; b = 0.33; c = 0.33;
                }

                items.Add(new TernaryPoint { A = a, B = b, C = c });
                idx++;
            }

            var ptBrush = MarkerBrush ?? Palette?.GetBrush(0) ?? new SolidColorBrush(Color.Parse("#06B6D4"));
            var strokeBrush = new Pen(Brushes.White, 1.0);
            double progress = AnimationProgress;

            foreach (var pt in items)
            {
                // Calculate Cartesian Coordinate inside Equilateral triangle space
                // Point is: (A * vLeft) + (B * vRight) + (C * vTop)
                double px = pt.A * vLeft.X + pt.B * vRight.X + pt.C * vTop.X;
                double py = pt.A * vLeft.Y + pt.B * vRight.Y + pt.C * vTop.Y;

                // Entry animation: interpolate coordinates from the triangle center (0.33, 0.33, 0.33)
                double centerX = 0.33 * vLeft.X + 0.33 * vRight.X + 0.33 * vTop.X;
                double centerY = 0.33 * vLeft.Y + 0.33 * vRight.Y + 0.33 * vTop.Y;

                double animX = centerX + (px - centerX) * progress;
                double animY = centerY + (py - centerY) * progress;

                context.DrawEllipse(ptBrush, strokeBrush, new Point(animX, animY), 5.0, 5.0);
            }
        }

        private static Point Interpolate(Point p1, Point p2, double f)
        {
            return new Point(p1.X + (p2.X - p1.X) * f, p1.Y + (p2.Y - p1.Y) * f);
        }

        private static double ConvertToDouble(object? value)
        {
            if (value == null) return double.NaN;
            try { return Convert.ToDouble(value); } catch { return double.NaN; }
        }

        private static object? ResolvePropertyValue(object item, string? path)
        {
            if (string.IsNullOrEmpty(path)) return item;
            var prop = item.GetType().GetProperty(path);
            return prop?.GetValue(item);
        }
    }
}