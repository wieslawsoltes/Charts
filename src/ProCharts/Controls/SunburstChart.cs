using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class SunburstChart : ChartBase
    {
        public class SunburstNode
        {
            public string Name { get; set; } = string.Empty;
            public double Value { get; set; }
            public IBrush? Color { get; set; }
            public List<SunburstNode> Children { get; } = new List<SunburstNode>();

            // Layout helpers
            internal double StartAngle { get; set; }
            internal double SweepAngle { get; set; }
            internal int Depth { get; set; }
            internal double InnerRadius { get; set; }
            internal double OuterRadius { get; set; }
        }

        public static readonly StyledProperty<IList<SunburstNode>?> RootNodesProperty =
            AvaloniaProperty.Register<SunburstChart, IList<SunburstNode>?>(nameof(RootNodes));

        public static readonly StyledProperty<double> InnerHollowRadiusPercentProperty =
            AvaloniaProperty.Register<SunburstChart, double>(nameof(InnerHollowRadiusPercent), 0.25);

        public IList<SunburstNode>? RootNodes
        {
            get => GetValue(RootNodesProperty);
            set => SetValue(RootNodesProperty, value);
        }

        public double InnerHollowRadiusPercent
        {
            get => GetValue(InnerHollowRadiusPercentProperty);
            set => SetValue(InnerHollowRadiusPercentProperty, value);
        }

        private List<SunburstNode> _flatNodes = new List<SunburstNode>();
        private SunburstNode? _hoveredNode;

        public SunburstChart()
        {
            RootNodesProperty.Changed.AddClassHandler<SunburstChart>((x, e) => x.InvalidateVisual());
            InnerHollowRadiusPercentProperty.Changed.AddClassHandler<SunburstChart>((x, e) => x.InvalidateVisual());
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double padding = 20;
            if (!string.IsNullOrEmpty(Title)) padding += 24;

            double side = Math.Min(bounds.Width, bounds.Height) - padding * 2;
            side = Math.Max(10, side);

            double cx = (bounds.Width - side) / 2.0;
            double cy = (bounds.Height - side) / 2.0;
            if (!string.IsNullOrEmpty(Title)) cy += 12;

            return new Rect(cx, cy, side, side);
        }

        protected override void RenderChart(DrawingContext context)
        {
            _flatNodes.Clear();
            if (RootNodes == null || RootNodes.Count == 0)
            {
                RenderEmptyState(context);
                return;
            }

            var area = EffectivePlotArea;
            var center = area.Center;
            double maxRadius = area.Width / 2.0;

            // 1. Calculate depths and sum values recursively
            int maxDepth = 0;
            foreach (var root in RootNodes)
            {
                CalculateNodeValuesAndDepth(root, 0, ref maxDepth);
            }

            int layers = maxDepth + 1;
            double hollowR = maxRadius * Math.Clamp(InnerHollowRadiusPercent, 0.0, 0.9);
            double layerThickness = (maxRadius - hollowR) / layers;

            // 2. Compute circular layout angles recursively
            double rootSum = RootNodes.Sum(r => r.Value);
            if (rootSum <= 0) return;

            double currentAngle = 0.0;
            var activePalette = Palette ?? Palette.Default;
            int nodeColorCounter = 0;

            foreach (var root in RootNodes)
            {
                double rootSweep = 360.0 * (root.Value / rootSum);
                LayoutNodeAngles(root, currentAngle, rootSweep, 0, hollowR, layerThickness, ref nodeColorCounter, activePalette);
                currentAngle += rootSweep;
            }

            // 3. Render Node Arc Sectors
            double progress = AnimationProgress;

            // Draw sectors
            foreach (var node in _flatNodes)
            {
                double sweep = node.SweepAngle * progress;
                if (sweep <= 0.05) continue;

                // Animate depth radius slightly outwards for entrance wave
                double nodeInner = hollowR + (node.InnerRadius - hollowR) * progress;
                double nodeOuter = hollowR + (node.OuterRadius - hollowR) * progress;

                var fillBrush = node.Color ?? activePalette.GetBrush(0);
                if (node == _hoveredNode)
                {
                    fillBrush = new SolidColorBrush(Color.Parse("#FFFFFF")); // Highlight hovered segment in pure white
                }

                var borderPen = new Pen(new SolidColorBrush(Color.Parse("#30FFFFFF")), 1.0);
                DrawArcSector(context, center, nodeInner, nodeOuter, node.StartAngle, sweep, fillBrush, borderPen);
            }
        }

        private void DrawArcSector(DrawingContext context, Point center, double innerR, double outerR, double startAngle, double sweepAngle, IBrush brush, Pen pen)
        {
            if (sweepAngle >= 359.95)
            {
                // Full circle ring sector
                var outerGeom = new EllipseGeometry(new Rect(center.X - outerR, center.Y - outerR, outerR * 2, outerR * 2));
                var innerGeom = new EllipseGeometry(new Rect(center.X - innerR, center.Y - innerR, innerR * 2, innerR * 2));
                var donutGeom = new CombinedGeometry(GeometryCombineMode.Exclude, outerGeom, innerGeom);
                context.DrawGeometry(brush, pen, donutGeom);
                return;
            }

            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                double radStart = startAngle * Math.PI / 180.0;
                double radEnd = (startAngle + sweepAngle) * Math.PI / 180.0;

                Point pos = center + new Point(outerR * Math.Cos(radStart), outerR * Math.Sin(radStart));
                Point poe = center + new Point(outerR * Math.Cos(radEnd), outerR * Math.Sin(radEnd));
                Point pis = center + new Point(innerR * Math.Cos(radStart), innerR * Math.Sin(radStart));
                Point pie = center + new Point(innerR * Math.Cos(radEnd), innerR * Math.Sin(radEnd));

                bool isLargeArc = sweepAngle > 180.0;

                ctx.BeginFigure(pis, true);
                ctx.LineTo(pos);
                ctx.ArcTo(poe, new Size(outerR, outerR), 0.0, isLargeArc, SweepDirection.Clockwise);
                ctx.LineTo(pie);
                ctx.ArcTo(pis, new Size(innerR, innerR), 0.0, isLargeArc, SweepDirection.CounterClockwise);
            }

            context.DrawGeometry(brush, pen, geom);
        }

        private double CalculateNodeValuesAndDepth(SunburstNode node, int currentDepth, ref int maxDepth)
        {
            node.Depth = currentDepth;
            if (currentDepth > maxDepth) maxDepth = currentDepth;

            if (node.Children.Count > 0)
            {
                double childSum = 0;
                foreach (var child in node.Children)
                {
                    childSum += CalculateNodeValuesAndDepth(child, currentDepth + 1, ref maxDepth);
                }
                node.Value = Math.Max(node.Value, childSum);
            }

            if (node.Value <= 0) node.Value = 1.0; // Fallback
            return node.Value;
        }

        private void LayoutNodeAngles(
            SunburstNode node,
            double startAngle,
            double sweepAngle,
            int depth,
            double hollowR,
            double thickness,
            ref int colorCounter,
            Palette palette)
        {
            node.StartAngle = startAngle;
            node.SweepAngle = sweepAngle;
            node.InnerRadius = hollowR + depth * thickness;
            node.OuterRadius = hollowR + (depth + 1) * thickness;
            
            if (node.Color == null)
            {
                node.Color = palette.GetBrush(colorCounter++);
            }

            _flatNodes.Add(node);

            if (node.Children.Count > 0 && sweepAngle > 0.05)
            {
                double childSum = node.Children.Sum(c => c.Value);
                if (childSum > 0)
                {
                    double currentChildAngle = startAngle;
                    foreach (var child in node.Children)
                    {
                        double childSweep = sweepAngle * (child.Value / childSum);
                        LayoutNodeAngles(child, currentChildAngle, childSweep, depth + 1, hollowR, thickness, ref colorCounter, palette);
                        currentChildAngle += childSweep;
                    }
                }
            }
        }

        protected override void UpdateHoverState(Point? mousePoint)
        {
            if (!mousePoint.HasValue || _flatNodes.Count == 0)
            {
                _hoveredNode = null;
                return;
            }

            var center = EffectivePlotArea.Center;
            var diff = mousePoint.Value - center;
            double dist = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
            double mouseAngle = Math.Atan2(diff.Y, diff.X) * 180.0 / Math.PI;
            if (mouseAngle < 0) mouseAngle += 360.0;

            SunburstNode? currentHover = null;

            foreach (var node in _flatNodes)
            {
                if (dist >= node.InnerRadius && dist <= node.OuterRadius)
                {
                    double nodeStart = node.StartAngle % 360.0;
                    double nodeEnd = (node.StartAngle + node.SweepAngle) % 360.0;
                    if (nodeEnd < nodeStart) nodeEnd += 360.0;

                    // Normalize mouse angle to compare cleanly
                    double normMouse = mouseAngle;
                    if (normMouse < nodeStart) normMouse += 360.0;

                    if (normMouse >= nodeStart && normMouse <= nodeStart + node.SweepAngle)
                    {
                        currentHover = node;
                        break;
                    }
                }
            }

            _hoveredNode = currentHover;
        }

        protected override void DrawTooltip(DrawingContext context, Point mousePoint)
        {
            if (_hoveredNode == null) return;

            double padding = 10;
            double textHeight = 18;
            double tooltipWidth = 150;
            double tooltipHeight = padding * 2 + textHeight * 2;

            var fontTitle = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            var fontText = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var textBrush = Brushes.White;

            var ftTitle = new FormattedText(_hoveredNode.Name, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontTitle, 11, textBrush);
            var ftVal = new FormattedText($"Value: {_hoveredNode.Value:N1} (Level {_hoveredNode.Depth})", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontText, 11, textBrush);

            tooltipWidth = Math.Max(tooltipWidth, Math.Max(ftTitle.Width, ftVal.Width) + padding * 2 + 10);

            double tx = mousePoint.X + 15;
            double ty = mousePoint.Y + 15;

            if (tx + tooltipWidth > Bounds.Width) tx = mousePoint.X - tooltipWidth - 15;
            if (ty + tooltipHeight > Bounds.Height) ty = mousePoint.Y - tooltipHeight - 15;

            tx = Math.Max(0, tx);
            ty = Math.Max(0, ty);

            var tooltipRect = new Rect(tx, ty, tooltipWidth, tooltipHeight);
            var bgBrush = new SolidColorBrush(Color.Parse("#EC1F242E"));
            var borderPen = new Pen(new SolidColorBrush(Color.Parse("#30FFFFFF")), 1.0);

            context.DrawRectangle(bgBrush, borderPen, new RoundedRect(tooltipRect, new CornerRadius(6.0)));

            context.DrawEllipse(_hoveredNode.Color, null, new Point(tx + padding + 4, ty + padding + 6), 3.5, 3.5);
            context.DrawText(ftTitle, new Point(tx + padding + 12, ty + padding));
            context.DrawText(ftVal, new Point(tx + padding + 12, ty + padding + textHeight));
        }

        private void RenderEmptyState(DrawingContext context)
        {
            var ft = new FormattedText(
                "Configure Sunburst Tree Hierarchy Nodes",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Italic, FontWeight.SemiBold),
                13,
                SystemBrush);

            double tx = EffectivePlotArea.Left + (EffectivePlotArea.Width - ft.Width) / 2.0;
            double ty = EffectivePlotArea.Top + (EffectivePlotArea.Height - ft.Height) / 2.0;
            context.DrawText(ft, new Point(tx, ty));
        }
    }
}
