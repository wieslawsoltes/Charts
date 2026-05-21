using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Avalonia.Styles;

namespace ProCharts.Avalonia.Controls
{
    public class TreemapChart : ChartBase
    {
        public class TreemapItem
        {
            public string Label { get; set; } = string.Empty;
            public double Value { get; set; }
            public IBrush? Color { get; set; }
        }

        public static readonly StyledProperty<IList<TreemapItem>?> ItemsProperty =
            AvaloniaProperty.Register<TreemapChart, IList<TreemapItem>?>(nameof(Items));

        public IList<TreemapItem>? Items
        {
            get => GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public TreemapChart()
        {
            ItemsProperty.Changed.AddClassHandler<TreemapChart>((x, e) => x.InvalidateVisual());
        }

        protected override Rect CalculatePlotArea(Size bounds)
        {
            double padding = 16;
            if (!string.IsNullOrEmpty(Title)) padding += 24;

            double w = Math.Max(10, bounds.Width - padding * 2);
            double h = Math.Max(10, bounds.Height - padding * 2);

            return new Rect(padding, padding, w, h);
        }

        protected override void RenderChart(DrawingContext context)
        {
            if (Items == null || Items.Count == 0)
            {
                RenderEmptyState(context);
                return;
            }

            var sortedItems = Items.Where(i => i.Value > 0).OrderByDescending(i => i.Value).ToList();
            if (sortedItems.Count == 0)
            {
                RenderEmptyState(context);
                return;
            }

            double progress = AnimationProgress;
            var activePalette = Palette ?? Palette.Default;

            // Run recursive binary space partitioning
            Partition(context, EffectivePlotArea, sortedItems, activePalette, progress);
        }

        private void Partition(DrawingContext context, Rect rect, List<TreemapItem> items, Palette palette, double progress)
        {
            if (items.Count == 0) return;

            if (items.Count == 1)
            {
                // Base Case: Draw the single rectangle block
                DrawItemBlock(context, rect, items[0], palette, progress);
                return;
            }

            // Binary split: Divide items list into two sub-groups with closest total weight balancing
            double totalVal = items.Sum(i => i.Value);
            double runningSum = 0.0;
            int splitIdx = 1;

            for (int i = 0; i < items.Count - 1; i++)
            {
                runningSum += items[i].Value;
                if (runningSum >= totalVal / 2.0)
                {
                    splitIdx = i + 1;
                    break;
                }
            }

            var leftGroup = items.Take(splitIdx).ToList();
            var rightGroup = items.Skip(splitIdx).ToList();

            double leftSum = leftGroup.Sum(i => i.Value);

            // Alternate split direction based on rectangle aspect ratio
            bool splitHorizontally = rect.Width >= rect.Height;

            if (splitHorizontally)
            {
                double splitWidth = rect.Width * (leftSum / totalVal);
                var leftRect = new Rect(rect.Left, rect.Top, splitWidth, rect.Height);
                var rightRect = new Rect(rect.Left + splitWidth, rect.Top, rect.Width - splitWidth, rect.Height);

                Partition(context, leftRect, leftGroup, palette, progress);
                Partition(context, rightRect, rightGroup, palette, progress);
            }
            else
            {
                double splitHeight = rect.Height * (leftSum / totalVal);
                var topRect = new Rect(rect.Left, rect.Top, rect.Width, splitHeight);
                var bottomRect = new Rect(rect.Left, rect.Top + splitHeight, rect.Width, rect.Height - splitHeight);

                Partition(context, topRect, leftGroup, palette, progress);
                Partition(context, bottomRect, rightGroup, palette, progress);
            }
        }

        private void DrawItemBlock(DrawingContext context, Rect rect, TreemapItem item, Palette palette, double progress)
        {
            // Shrink rect slightly for spacing / padding gap
            double gap = 1.5;
            if (rect.Width <= gap * 2.0 || rect.Height <= gap * 2.0) return;

            // Apply entry animation scale
            double cx = rect.Left + rect.Width / 2.0;
            double cy = rect.Top + rect.Height / 2.0;
            double aw = rect.Width * progress;
            double ah = rect.Height * progress;
            var animatedRect = new Rect(cx - aw / 2.0 + gap, cy - ah / 2.0 + gap, aw - gap * 2.0, ah - gap * 2.0);

            // Palette brush mapping
            int idx = Items!.IndexOf(item);
            var itemBrush = item.Color ?? palette.GetBrush(idx >= 0 ? idx : 0);
            var borderPen = new Pen(new SolidColorBrush(Color.Parse("#40FFFFFF")), 1.0);

            context.DrawRectangle(itemBrush, borderPen, new RoundedRect(animatedRect, new CornerRadius(4.0)));

            // Draw category label centered inside rect (only if there's enough space)
            if (animatedRect.Width > 45 && animatedRect.Height > 25)
            {
                var labelFont = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
                var ftLabel = new FormattedText(
                    item.Label,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    labelFont,
                    11,
                    Brushes.White);

                var ftVal = new FormattedText(
                    $"{item.Value:G3}",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal),
                    9,
                    new SolidColorBrush(Color.Parse("#CCFFFFFF")));

                if (ftLabel.Width < animatedRect.Width - 6)
                {
                    double lx = animatedRect.Left + (animatedRect.Width - ftLabel.Width) / 2.0;
                    double ly = animatedRect.Top + (animatedRect.Height - (ftLabel.Height + ftVal.Height)) / 2.0;
                    context.DrawText(ftLabel, new Point(lx, ly));
                    
                    if (animatedRect.Height > 40 && ftVal.Width < animatedRect.Width - 6)
                    {
                        double vx = animatedRect.Left + (animatedRect.Width - ftVal.Width) / 2.0;
                        context.DrawText(ftVal, new Point(vx, ly + ftLabel.Height + 2.0));
                    }
                }
            }
        }

        private void RenderEmptyState(DrawingContext context)
        {
            var ft = new FormattedText(
                "Configure Treemap Items",
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
