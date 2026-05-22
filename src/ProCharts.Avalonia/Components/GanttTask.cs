using System;
using Avalonia;
using Avalonia.Media;

namespace ProCharts.Avalonia.Components
{
    /// <summary>
    /// Represents a scheduling task in the GanttChart timeline.
    /// </summary>
    public class GanttTask : AvaloniaObject
    {
        // --- DEPENDENCY PROPERTIES ---

        /// <summary>
        /// Defines the Name dependency property.
        /// </summary>
        public static readonly StyledProperty<string> NameProperty =
            AvaloniaProperty.Register<GanttTask, string>(nameof(Name), string.Empty);

        /// <summary>
        /// Defines the Start dependency property.
        /// </summary>
        public static readonly StyledProperty<DateTime> StartProperty =
            AvaloniaProperty.Register<GanttTask, DateTime>(nameof(Start), DateTime.MinValue);

        /// <summary>
        /// Defines the End dependency property.
        /// </summary>
        public static readonly StyledProperty<DateTime> EndProperty =
            AvaloniaProperty.Register<GanttTask, DateTime>(nameof(End), DateTime.MinValue);

        /// <summary>
        /// Defines the Progress dependency property.
        /// </summary>
        public static readonly StyledProperty<double> ProgressProperty =
            AvaloniaProperty.Register<GanttTask, double>(nameof(Progress), 0.0);

        /// <summary>
        /// Defines the CustomBrush dependency property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> CustomBrushProperty =
            AvaloniaProperty.Register<GanttTask, IBrush?>(nameof(CustomBrush), null);

        // --- PROPERTIES ---

        /// <summary>
        /// Gets or sets the name/label of the task.
        /// </summary>
        public string Name
        {
            get => GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        /// <summary>
        /// Gets or sets the starting date and time of the task.
        /// </summary>
        public DateTime Start
        {
            get => GetValue(StartProperty);
            set => SetValue(StartProperty, value);
        }

        /// <summary>
        /// Gets or sets the ending date and time of the task.
        /// </summary>
        public DateTime End
        {
            get => GetValue(EndProperty);
            set => SetValue(EndProperty, value);
        }

        /// <summary>
        /// Gets or sets the completion ratio from 0.0 to 1.0.
        /// </summary>
        public double Progress
        {
            get => GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        /// <summary>
        /// Gets or sets the customized brush to fill the task progress.
        /// </summary>
        public IBrush? CustomBrush
        {
            get => GetValue(CustomBrushProperty);
            set => SetValue(CustomBrushProperty, value);
        }
    }
}
