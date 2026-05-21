using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Avalonia.Styles;

namespace ProCharts.Avalonia.Controls
{
    public class SankeyChart : ChartBase
    {
        public class SankeyNode
        {
            public string Name { get; set; } = string.Empty;
            public double Value { get; set; }
            public IBrush? Color { get; set; }
            // Layout fields
            internal Rect Bounds { get; set; }
            internal double CurrentSourceOffset { get; set; }
            internal double CurrentTargetOffset { get; set; }
        }

        public class SankeyLink
        {
            public string Source { get; set; } = string.Empty;
            public string Target { get; set; } = string.Empty;
            public double Flow { get; set; }
            public IBrush? Color { get; set; }
        }

        public static readonly StyledProperty<IList<SankeyNode>?> NodesProperty =
            AvaloniaProperty.Register<SankeyChart, IList<SankeyNode>?>(nameof(Nodes));

        public static readonly StyledProperty<IList<SankeyLink>?> LinksProperty =
            AvaloniaProperty.Register<SankeyChart, IList<SankeyLink>?>(nameof(Links));

        public IList<SankeyNode>? Nodes
        {
            get => GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        public IList<SankeyLink>? Links
        {
            get => GetValue(LinksProperty);
            set => SetValue(LinksProperty, value);
        }

        public SankeyChart()
        {
            NodesProperty.Changed.AddClassHandler<SankeyChart>((x, e) => x.InvalidateVisual());
            LinksProperty.Changed.AddClassHandler<SankeyChart>((x, e) => x.InvalidateVisual());
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double top = 20;
            double bottom = 20;
            double left = 30;
            double right = 30;

            if (!string.IsNullOrEmpty(Title)) top += 24;

            double w = Math.Max(10, bounds.Width - left - right);
            double h = Math.Max(10, bounds.Height - top - bottom);

            return new Rect(left, top, w, h);
        }

        protected override void RenderChart(DrawingContext context)
        {
            if (Nodes == null || Nodes.Count == 0 || Links == null || Links.Count == 0)
            {
                RenderEmptyState(context);
                return;
            }

            var area = EffectivePlotArea;
            double nodeWidth = 20.0;
            double progress = AnimationProgress;

            // 1. Layout nodes into layers
            // To be robust:
            // Identify source-only nodes (left), target-only nodes (right), and middle nodes.
            // Simplified clean 2-column layout: Left column = nodes that only output, Right column = nodes that receive.
            // Let's compute node column placement.
            var sources = Links.Select(l => l.Source).ToHashSet();
            var targets = Links.Select(l => l.Target).ToHashSet();

            var leftNodes = Nodes.Where(n => sources.Contains(n.Name) && !targets.Contains(n.Name)).ToList();
            var rightNodes = Nodes.Where(n => targets.Contains(n.Name)).ToList();
            var remaining = Nodes.Where(n => !leftNodes.Contains(n) && !rightNodes.Contains(n)).ToList();

            // Default remaining to left if empty, else divide
            foreach (var rem in remaining)
            {
                leftNodes.Add(rem);
            }

            if (leftNodes.Count == 0 || rightNodes.Count == 0)
            {
                RenderEmptyState(context);
                return;
            }

            // 2. Compute Node Values from Flows
            var nodeFlowSum = new Dictionary<string, double>();
            foreach (var node in Nodes)
            {
                double outFlow = Links.Where(l => l.Source == node.Name).Sum(l => l.Flow);
                double inFlow = Links.Where(l => l.Target == node.Name).Sum(l => l.Flow);
                node.Value = Math.Max(outFlow, inFlow);
                node.CurrentSourceOffset = 0.0;
                node.CurrentTargetOffset = 0.0;
            }

            // Scale factor to map values to screen pixels
            double leftSum = leftNodes.Sum(n => n.Value);
            double rightSum = rightNodes.Sum(n => n.Value);
            double maxSum = Math.Max(leftSum, rightSum);

            if (maxSum <= 0) return;

            double nodeGap = 16.0;
            double availableHeight = area.Height - (Math.Max(leftNodes.Count, rightNodes.Count) - 1) * nodeGap;
            double scale = availableHeight / maxSum;

            // Layout Left Column
            double leftY = area.Top;
            var activePalette = Palette ?? Palette.Default;
            for (int i = 0; i < leftNodes.Count; i++)
            {
                var node = leftNodes[i];
                double h = Math.Max(4.0, node.Value * scale);
                node.Bounds = new Rect(area.Left, leftY, nodeWidth, h);
                leftY += h + nodeGap;

                // Draw Node
                var brush = node.Color ?? activePalette.GetBrush(i);
                var borderPen = new Pen(new SolidColorBrush(Color.Parse("#40FFFFFF")), 1.0);
                context.DrawRectangle(brush, borderPen, new RoundedRect(node.Bounds, new CornerRadius(4.0)));

                // Text Name Label
                var labelFont = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.SemiBold);
                var ftLabel = new FormattedText(
                    node.Name,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    labelFont,
                    11,
                    SystemBrush);
                context.DrawText(ftLabel, new Point(node.Bounds.Left - ftLabel.Width - 6.0, node.Bounds.Top + (node.Bounds.Height - ftLabel.Height) / 2.0));
            }

            // Layout Right Column
            double rightY = area.Top;
            for (int i = 0; i < rightNodes.Count; i++)
            {
                var node = rightNodes[i];
                double h = Math.Max(4.0, node.Value * scale);
                node.Bounds = new Rect(area.Right - nodeWidth, rightY, nodeWidth, h);
                rightY += h + nodeGap;

                // Draw Node
                var brush = node.Color ?? activePalette.GetBrush(i + leftNodes.Count);
                var borderPen = new Pen(new SolidColorBrush(Color.Parse("#40FFFFFF")), 1.0);
                context.DrawRectangle(brush, borderPen, new RoundedRect(node.Bounds, new CornerRadius(4.0)));

                // Text Name Label
                var labelFont = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.SemiBold);
                var ftLabel = new FormattedText(
                    node.Name,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    labelFont,
                    11,
                    SystemBrush);
                context.DrawText(ftLabel, new Point(node.Bounds.Right + 6.0, node.Bounds.Top + (node.Bounds.Height - ftLabel.Height) / 2.0));
            }

            // 3. Render Link Flow Bezier Ribbons
            foreach (var link in Links)
            {
                var srcNode = Nodes.FirstOrDefault(n => n.Name == link.Source);
                var tgtNode = Nodes.FirstOrDefault(n => n.Name == link.Target);

                if (srcNode == null || tgtNode == null) continue;

                double flowHeight = link.Flow * scale * progress;

                // Compute ribbons bounds
                double srcTop = srcNode.Bounds.Top + srcNode.CurrentSourceOffset;
                double tgtTop = tgtNode.Bounds.Top + tgtNode.CurrentTargetOffset;

                srcNode.CurrentSourceOffset += flowHeight;
                tgtNode.CurrentTargetOffset += flowHeight;

                var ptSrcTop = new Point(srcNode.Bounds.Right, srcTop);
                var ptSrcBottom = new Point(srcNode.Bounds.Right, srcTop + flowHeight);
                var ptTgtTop = new Point(tgtNode.Bounds.Left, tgtTop);
                var ptTgtBottom = new Point(tgtNode.Bounds.Left, tgtTop + flowHeight);

                // Bezier ribbon geometry
                var geom = new StreamGeometry();
                using (var ctx = geom.Open())
                {
                    ctx.BeginFigure(ptSrcTop, true);
                    
                    // Top bezier curve
                    double ctrlX1 = ptSrcTop.X + (ptTgtTop.X - ptSrcTop.X) / 2.0;
                    ctx.CubicBezierTo(
                        new Point(ctrlX1, ptSrcTop.Y),
                        new Point(ctrlX1, ptTgtTop.Y),
                        ptTgtTop);

                    // Line down target
                    ctx.LineTo(ptTgtBottom);

                    // Bottom bezier curve (backward)
                    ctx.CubicBezierTo(
                        new Point(ctrlX1, ptTgtBottom.Y),
                        new Point(ctrlX1, ptSrcBottom.Y),
                        ptSrcBottom);
                }

                // Ribbon fill brush (highly transparent matching node color or custom link color)
                Color brushColor = Colors.Teal;
                if (srcNode.Color is SolidColorBrush scb) brushColor = scb.Color;
                else if (Palette != null)
                {
                    var b = activePalette.GetBrush(Nodes.IndexOf(srcNode));
                    if (b is SolidColorBrush scbPal) brushColor = scbPal.Color;
                }
                var ribbonBrush = link.Color ?? new SolidColorBrush(Color.FromArgb(50, brushColor.R, brushColor.G, brushColor.B));

                context.DrawGeometry(ribbonBrush, null, geom);
            }
        }

        private void RenderEmptyState(DrawingContext context)
        {
            var ft = new FormattedText(
                "Configure Sankey Nodes and Flows",
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
