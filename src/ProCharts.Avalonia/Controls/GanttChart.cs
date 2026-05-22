using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using ProCharts.Avalonia.Components;
using ProCharts.Avalonia.Styles;

namespace ProCharts.Avalonia.Controls
{
    /// <summary>
    /// Represents a scheduling Gantt timeline chart showing tasks, durations, and progress ratios.
    /// </summary>
    public class GanttChart : ChartBase
    {
        // --- DEPENDENCY PROPERTIES ---

        /// <summary>
        /// Defines the Tasks dependency property.
        /// </summary>
        public static readonly StyledProperty<IList<GanttTask>?> TasksProperty =
            AvaloniaProperty.Register<GanttChart, IList<GanttTask>?>(nameof(Tasks));

        // --- PROPERTIES ---

        /// <summary>
        /// Gets or sets the collection of tasks on the timeline.
        /// </summary>
        public IList<GanttTask>? Tasks
        {
            get => GetValue(TasksProperty);
            set => SetValue(TasksProperty, value);
        }

        private GanttTask? _hoveredTask;

        // --- CONSTRUCTOR ---

        /// <summary>
        /// Initializes a new instance of the GanttChart class.
        /// </summary>
        public GanttChart()
        {
            TasksProperty.Changed.AddClassHandler<GanttChart>((x, e) => x.InvalidateVisual());
        }

        /// <inheritdoc />
        protected override Rect CalculatePlotArea(Size bounds)
        {
            double top = 16;
            double bottom = 36;
            double left = 140; // Wide margin for vertical task labels
            double right = 24;

            if (!string.IsNullOrEmpty(Title))
            {
                top += 28;
            }

            double w = Math.Max(10, bounds.Width - left - right);
            double h = Math.Max(10, bounds.Height - top - bottom);

            return new Rect(left, top, w, h);
        }

        /// <inheritdoc />
        protected override void RenderChart(DrawingContext context)
        {
            if (Tasks == null || Tasks.Count == 0)
            {
                RenderEmptyState(context);
                return;
            }

            var activeTasks = Tasks.Where(t => t.Start != DateTime.MinValue && t.End != DateTime.MinValue && t.End >= t.Start).ToList();
            if (activeTasks.Count == 0)
            {
                RenderEmptyState(context);
                return;
            }

            DateTime minDate = activeTasks.Min(t => t.Start);
            DateTime maxDate = activeTasks.Max(t => t.End);

            // Add margin to date range if they are identical
            if (minDate == maxDate)
            {
                minDate = minDate.AddDays(-1);
                maxDate = maxDate.AddDays(1);
            }

            double totalSeconds = (maxDate - minDate).TotalSeconds;
            if (totalSeconds <= 0) return;

            var area = EffectivePlotArea;
            double rowHeight = area.Height / activeTasks.Count;
            double xScaler = area.Width / totalSeconds;

            // 1. Draw Vertical Timeline Gridlines and Labels
            DrawTimelineGrid(context, minDate, maxDate, xScaler);

            // 2. Draw Tasks
            var activePalette = Palette ?? Palette.Default;
            var textBrush = LabelForeground ?? SystemBrush;
            var fontLabel = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.SemiBold);

            for (int i = 0; i < activeTasks.Count; i++)
            {
                var task = activeTasks[i];
                double taskRowY = area.Top + i * rowHeight;

                // Mapped coordinates
                double x1 = area.Left + (task.Start - minDate).TotalSeconds * xScaler;
                double x2 = area.Left + (task.End - minDate).TotalSeconds * xScaler;
                x1 = Math.Clamp(x1, area.Left, area.Right);
                x2 = Math.Clamp(x2, area.Left, area.Right);

                double padding = rowHeight * 0.15;
                double barHeight = rowHeight - 2 * padding;
                double barY = taskRowY + padding;

                var taskRect = new Rect(x1, barY, Math.Max(2.0, x2 - x1), barHeight);

                // Palette color
                var baseBrush = task.CustomBrush ?? activePalette.GetBrush(i);
                var fillBg = new SolidColorBrush(Color.FromArgb(40, ((SolidColorBrush)baseBrush).Color.R, ((SolidColorBrush)baseBrush).Color.G, ((SolidColorBrush)baseBrush).Color.B));
                var fillProgress = baseBrush;

                if (task == _hoveredTask)
                {
                    fillProgress = new SolidColorBrush(Color.FromArgb(220, ((SolidColorBrush)baseBrush).Color.R, ((SolidColorBrush)baseBrush).Color.G, ((SolidColorBrush)baseBrush).Color.B));
                }

                // Render Background Bar
                var borderPen = new Pen(new SolidColorBrush(Color.Parse("#30FFFFFF")), 1.0);
                context.DrawRectangle(fillBg, borderPen, new RoundedRect(taskRect, 4.0));

                // Render Progress Overlay Bar
                double progressPercent = Math.Clamp(task.Progress, 0.0, 1.0);
                double progressWidth = taskRect.Width * progressPercent * AnimationProgress;
                if (progressWidth > 0)
                {
                    var progressRect = new Rect(taskRect.X, taskRect.Y, progressWidth, taskRect.Height);
                    context.DrawRectangle(fillProgress, null, new RoundedRect(progressRect, 4.0));
                }

                // Draw Row Divider
                context.DrawLine(new Pen(new SolidColorBrush(Color.Parse("#15FFFFFF")), 1.0), new Point(Bounds.Left, taskRowY + rowHeight), new Point(Bounds.Right, taskRowY + rowHeight));

                // Draw Task Name Label in Left margin
                var ftName = new FormattedText(
                    task.Name,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    fontLabel,
                    11,
                    textBrush);

                double lx = area.Left - ftName.Width - 16;
                double ly = taskRowY + (rowHeight - ftName.Height) / 2.0;
                context.DrawText(ftName, new Point(Math.Max(8, lx), ly));
            }
        }

        private void DrawTimelineGrid(DrawingContext context, DateTime minDate, DateTime maxDate, double xScaler)
        {
            var area = EffectivePlotArea;
            var textBrush = LabelForeground ?? SystemBrush;
            var fontLabel = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var linePen = new Pen(new SolidColorBrush(Color.Parse("#15FFFFFF")), 1.0);

            int intervals = 5;
            double secondsStep = (maxDate - minDate).TotalSeconds / intervals;

            for (int i = 0; i <= intervals; i++)
            {
                double currentSecs = i * secondsStep;
                DateTime currentDate = minDate.AddSeconds(currentSecs);
                double gridX = area.Left + currentSecs * xScaler;

                // Vertical Gridline
                context.DrawLine(linePen, new Point(gridX, area.Top), new Point(gridX, area.Bottom));

                // Label at bottom
                string labelText = currentDate.ToString("MMM dd");
                var ft = new FormattedText(
                    labelText,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    fontLabel,
                    10,
                    textBrush);

                double tx = gridX - ft.Width / 2.0;
                double ty = area.Bottom + 8;
                context.DrawText(ft, new Point(tx, ty));
            }
        }

        private void RenderEmptyState(DrawingContext context)
        {
            var ft = new FormattedText(
                "Configure Gantt Tasks",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Roboto, Segoe UI", FontStyle.Italic, FontWeight.SemiBold),
                13,
                SystemBrush);

            double tx = EffectivePlotArea.Left + (EffectivePlotArea.Width - ft.Width) / 2.0;
            double ty = EffectivePlotArea.Top + (EffectivePlotArea.Height - ft.Height) / 2.0;
            context.DrawText(ft, new Point(tx, ty));
        }

        /// <inheritdoc />
        protected override void UpdateHoverState(Point? mousePoint)
        {
            if (Tasks == null || Tasks.Count == 0 || !mousePoint.HasValue)
            {
                _hoveredTask = null;
                return;
            }

            var activeTasks = Tasks.Where(t => t.Start != DateTime.MinValue && t.End != DateTime.MinValue && t.End >= t.Start).ToList();
            if (activeTasks.Count == 0)
            {
                _hoveredTask = null;
                return;
            }

            DateTime minDate = activeTasks.Min(t => t.Start);
            DateTime maxDate = activeTasks.Max(t => t.End);
            if (minDate == maxDate)
            {
                minDate = minDate.AddDays(-1);
                maxDate = maxDate.AddDays(1);
            }

            double totalSeconds = (maxDate - minDate).TotalSeconds;
            if (totalSeconds <= 0) return;

            var area = EffectivePlotArea;
            double rowHeight = area.Height / activeTasks.Count;
            double xScaler = area.Width / totalSeconds;

            GanttTask? currentHover = null;

            for (int i = 0; i < activeTasks.Count; i++)
            {
                var task = activeTasks[i];
                double taskRowY = area.Top + i * rowHeight;
                double x1 = area.Left + (task.Start - minDate).TotalSeconds * xScaler;
                double x2 = area.Left + (task.End - minDate).TotalSeconds * xScaler;

                double padding = rowHeight * 0.15;
                double barHeight = rowHeight - 2 * padding;
                double barY = taskRowY + padding;

                var taskRect = new Rect(x1, barY, Math.Max(2.0, x2 - x1), barHeight);
                if (taskRect.Contains(mousePoint.Value))
                {
                    currentHover = task;
                    break;
                }
            }

            _hoveredTask = currentHover;
        }

        /// <inheritdoc />
        protected override void DrawTooltip(DrawingContext context, Point mousePoint)
        {
            if (_hoveredTask == null) return;

            double padding = 10;
            double rowHeight = 16;
            double tooltipWidth = 180;
            double tooltipHeight = padding * 2 + rowHeight * 3;

            var fontTitle = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Bold);
            var fontText = new Typeface("Inter, Roboto, Segoe UI", FontStyle.Normal, FontWeight.Normal);
            var textBrush = Brushes.White;

            var ftTitle = new FormattedText(_hoveredTask.Name, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontTitle, 11, textBrush);
            var ftDuration = new FormattedText($"Duration: {_hoveredTask.Start:MM/dd} - {_hoveredTask.End:MM/dd}", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontText, 10, textBrush);
            var ftProgress = new FormattedText($"Completion: {(_hoveredTask.Progress * 100.0):F0}%", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, fontText, 10, textBrush);

            tooltipWidth = Math.Max(tooltipWidth, Math.Max(ftTitle.Width, Math.Max(ftDuration.Width, ftProgress.Width)) + padding * 2 + 10);

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

            context.DrawText(ftTitle, new Point(tx + padding, ty + padding));
            context.DrawText(ftDuration, new Point(tx + padding, ty + padding + rowHeight));
            context.DrawText(ftProgress, new Point(tx + padding, ty + padding + rowHeight * 2));
        }
    }
}
