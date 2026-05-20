using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Styles;

namespace ProCharts.Controls
{
    public class AlluvialChart : ChartBase
    {
        public class AlluvialNode
        {
            public string Name { get; set; } = string.Empty;
            public int Stage { get; set; } // 0-indexed column index
            public double Value { get; set; }
            public IBrush? Color { get; set; }
            // Layout helpers
            internal Rect Bounds { get; set; }
            internal double CurrentSourceOffset { get; set; }
            internal double CurrentTargetOffset { get; set; }
        }

        public class AlluvialLink
        {
            public string Source { get; set; } = string.Empty; // Node name in stage S
            public string Target { get; set; } = string.Empty; // Node name in stage S+1
            public double Flow { get; set; }
            public IBrush? Color { get; set; }
        }

        public static readonly StyledProperty<IList<AlluvialNode>?> NodesProperty =
            AvaloniaProperty.Register<AlluvialChart, IList<AlluvialNode>?>(nameof(Nodes));

        public static readonly StyledProperty<IList<AlluvialLink>?> LinksProperty =
            AvaloniaProperty.Register<AlluvialChart, IList<AlluvialLink>?>(nameof(Links));

        public static readonly StyledProperty<double> NodeWidthProperty =
            AvaloniaProperty.Register<AlluvialChart, double>(nameof(NodeWidth), 18.0);

        public static readonly StyledProperty<double> NodeGapProperty =
            AvaloniaProperty.Register<AlluvialChart, double>(nameof(NodeGap), 12.0);

        public IList<AlluvialNode>? Nodes
        {
            get => GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        public IList<AlluvialLink>? Links
        {
            get => GetValue(LinksProperty);
            set => SetValue(LinksProperty, value);
        }

        public double NodeWidth
        {
            get => GetValue(NodeWidthProperty);
            set => SetValue(NodeWidthProperty, value);
        }

        public double NodeGap
        {
            get => GetValue(NodeGapProperty);
            set => SetValue(NodeGapProperty, value);
        }

        private int _hoveredNodeIndex = -1;
        private List<AlluvialNode> _renderedNodes = new List<AlluvialNode>();

        public AlluvialChart()
        {
            NodesProperty.Changed.AddClassHandler<AlluvialChart>((x, e) => x.InvalidateVisual());
            LinksProperty.Changed.AddClassHandler<AlluvialChart>((x, e) => x.InvalidateVisual());
            NodeWidthProperty.Changed.AddClassHandler<AlluvialChart>((x, e) => x.InvalidateVisual());
            NodeGapProperty.Changed.AddClassHandler<AlluvialChart>((x, e) => x.InvalidateVisual());
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double top = 20;
            double bottom = 20;
            double left = 40;
            double right = 40;

            if (!string.IsNullOrEmpty(Title)) top += 24;

            double w = Math.Max(10, bounds.Width - left - right);
            double h = Math.Max(10, bounds.Height - top - bottom);

            return new Rect(left, top, w, h);
        }

        protected override void RenderChart(DrawingContext context)
        {
            _renderedNodes.Clear();
            if (Nodes == null || Nodes.Count == 0 || Links == null || Links.Count == 0)
            {
                RenderEmptyState(context);
                return;
            }

            var area = EffectivePlotArea;
            double nWidth = Math.Clamp(NodeWidth, 4.0, 50.0);
            double gap = Math.Clamp(NodeGap, 2.0, 50.0);
            double progress = AnimationProgress;
            var activePalette = Palette ?? Palette.Default;

            // 1. Calculate stages
            int maxStage = Nodes.Max(n => n.Stage);
            int stageCount = maxStage + 1;
            if (stageCount < 2)
            {
                RenderEmptyState(context);
                return;
            }

            // 2. Calculate values of nodes based on incoming/outgoing flows
            var nodeFlowSum = new Dictionary<string, double>();
            foreach (var node in Nodes)
            {
                double outFlow = Links.Where(l => l.Source == node.Name).Sum(l => l.Flow);
                double inFlow = Links.Where(l => l.Target == node.Name).Sum(l => l.Flow);
                node.Value = Math.Max(node.Value, Math.Max(outFlow, inFlow));
                node.CurrentSourceOffset = 0.0;
                node.CurrentTargetOffset = 0.0;
            }

            // 3. Layout columns
            double columnSpacing = area.Width / (stageCount - 1);

            for (int s = 0; s < stageCount; s++)
            {
                var stageNodes = Nodes.Where(n => n.Stage == s).ToList();
                if (stageNodes.Count == 0) continue;

                double totalValue = stageNodes.Sum(n => n.Value);
                if (totalValue <= 0) continue;

                // Scale factor for this column
                double availableHeight = area.Height - (stageNodes.Count - 1) * gap;
                double scale = availableHeight / totalValue;

                double currentY = area.Top;
                double colX = area.Left + s * columnSpacing;

                // Adjust outermost columns to keep nodes inside the plot bounds
                if (s == 0)
                {
                    // colX stays at Left
                }
                else if (s == stageCount - 1)
                {
                    colX = area.Right - nWidth;
                }
                else
                {
                    colX = colX - nWidth / 2.0;
                }

                for (int i = 0; i < stageNodes.Count; i++)
                {
                    var node = stageNodes[i];
                    double h = Math.Max(4.0, node.Value * scale);

                    node.Bounds = new Rect(colX, currentY, nWidth, h);
                    _renderedNodes.Add(node);

                    currentY += h + gap;

                    // Draw node block
                    var brush = node.Color ?? activePalette.GetBrush(_renderedNodes.Count - 1);
                    if (_renderedNodes.Count - 1 == _hoveredNodeIndex)
                    {
                        brush = new SolidColorBrush(Color.Parse("#FFFFFF"));
                    }

                    var borderPen = new Pen(new SolidColorBrush(Color.Parse("#40FFFFFF")), 1.0);
                    context.DrawRectangle(brush, borderPen, new RoundedRect(node.Bounds, new CornerRadius(3.0)));

                    // Label drawing
                    var font = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
                    var ft = new FormattedText(node.Name, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, 10, SystemBrush);

                    double tx = node.Bounds.Left - ft.Width - 6;
                    if (s == 0)
                    {
                        tx = node.Bounds.Right + 6;
                    }
                    else if (s == stageCount - 1)
                    {
                        tx = node.Bounds.Left - ft.Width - 6;
                    }
                    else
                    {
                        tx = node.Bounds.Center.X - ft.Width / 2.0;
                    }

                    double ty = node.Bounds.Top - ft.Height - 2;
                    if (s != 0 && s != stageCount - 1)
                    {
                        // Draw stacked label at the top or bottom
                        ty = node.Bounds.Top + (node.Bounds.Height - ft.Height) / 2.0;
                        tx = node.Bounds.Right + 6;
                    }
                    else
                    {
                        ty = node.Bounds.Top + (node.Bounds.Height - ft.Height) / 2.0;
                    }

                    context.DrawText(ft, new Point(tx, ty));
                }
            }

            // 4. Render links stage-by-stage (stage S to S+1)
            for (int s = 0; s < stageCount - 1; s++)
            {
                var stageLinks = Links.Where(l =>
                {
                    var src = Nodes.FirstOrDefault(n => n.Name == l.Source && n.Stage == s);
                    var tgt = Nodes.FirstOrDefault(n => n.Name == l.Target && n.Stage == s + 1);
                    return src != null && tgt != null;
                }).ToList();

                // Compute scaling factors matching each node's local bounds
                foreach (var link in stageLinks)
                {
                    var srcNode = Nodes.First(n => n.Name == link.Source && n.Stage == s);
                    var tgtNode = Nodes.First(n => n.Name == link.Target && n.Stage == s + 1);

                    double srcScale = srcNode.Bounds.Height / srcNode.Value;
                    double tgtScale = tgtNode.Bounds.Height / tgtNode.Value;

                    double flowSrcH = link.Flow * srcScale * progress;
                    double flowTgtH = link.Flow * tgtScale * progress;

                    double srcTop = srcNode.Bounds.Top + srcNode.CurrentSourceOffset;
                    double tgtTop = tgtNode.Bounds.Top + tgtNode.CurrentTargetOffset;

                    srcNode.CurrentSourceOffset += flowSrcH;
                    tgtNode.CurrentTargetOffset += flowTgtH;

                    var ptSrcTop = new Point(srcNode.Bounds.Right, srcTop);
                    var ptSrcBottom = new Point(srcNode.Bounds.Right, srcTop + flowSrcH);
                    var ptTgtTop = new Point(tgtNode.Bounds.Left, tgtTop);
                    var ptTgtBottom = new Point(tgtNode.Bounds.Left, tgtTop + flowTgtH);

                    // Draw cubic Bezier link ribbon
                    var geom = new StreamGeometry();
                    using (var ctx = geom.Open())
                    {
                        ctx.BeginFigure(ptSrcTop, true);

                        double ctrlX1 = ptSrcTop.X + (ptTgtTop.X - ptSrcTop.X) / 2.0;
                        ctx.CubicBezierTo(
                            new Point(ctrlX1, ptSrcTop.Y),
                            new Point(ctrlX1, ptTgtTop.Y),
                            ptTgtTop);

                        ctx.LineTo(ptTgtBottom);

                        ctx.CubicBezierTo(
                            new Point(ctrlX1, ptTgtBottom.Y),
                            new Point(ctrlX1, ptSrcBottom.Y),
                            ptSrcBottom);
                    }

                    // Translucent flowing link color based on source node color
                    Color cColor = Colors.SkyBlue;
                    if (srcNode.Color is SolidColorBrush scb) cColor = scb.Color;
                    else if (Palette != null)
                    {
                        var b = activePalette.GetBrush(Nodes.IndexOf(srcNode));
                        if (b is SolidColorBrush scbPal) cColor = scbPal.Color;
                    }

                    var ribbonBrush = link.Color ?? new SolidColorBrush(Color.FromArgb(45, cColor.R, cColor.G, cColor.B));
                    context.DrawGeometry(ribbonBrush, null, geom);
                }
            }

        }

        protected override void UpdateHoverState(Point? mousePoint)
        {
            if (!mousePoint.HasValue || _renderedNodes.Count == 0)
            {
                _hoveredNodeIndex = -1;
                return;
            }

            int hoveredIdx = -1;
            for (int i = 0; i < _renderedNodes.Count; i++)
            {
                if (_renderedNodes[i].Bounds.Contains(mousePoint.Value))
                {
                    hoveredIdx = i;
                    break;
                }
            }

            _hoveredNodeIndex = hoveredIdx;
        }

        protected override void DrawTooltip(DrawingContext context, Point mousePoint)
        {
            if (_hoveredNodeIndex == -1 || _hoveredNodeIndex >= _renderedNodes.Count) return;

            var hovered = _renderedNodes[_hoveredNodeIndex];

            double padding = 10;
            double textHeight = 18;
            double tooltipWidth = 140;
            double tooltipHeight = padding * 2 + textHeight * 2;

            var fontTitle = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            var fontText = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var textBrush = Brushes.White;

            var ftTitle = new FormattedText(hovered.Name, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontTitle, 11, textBrush);
            var ftVal = new FormattedText($"Total: {hovered.Value:N1} (Stage {hovered.Stage + 1})", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontText, 11, textBrush);

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

            context.DrawEllipse(hovered.Color, null, new Point(tx + padding + 4, ty + padding + 6), 3.5, 3.5);
            context.DrawText(ftTitle, new Point(tx + padding + 12, ty + padding));
            context.DrawText(ftVal, new Point(tx + padding + 12, ty + padding + textHeight));
        }

        private void RenderEmptyState(DrawingContext context)
        {
            var ft = new FormattedText(
                "Configure Alluvial Nodes & Multi-Stage Links",
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
