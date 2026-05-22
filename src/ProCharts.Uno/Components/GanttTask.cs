using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace ProCharts.Uno.Components
{
    /// <summary>
    /// Represents a scheduling task in the GanttChart timeline.
    /// </summary>
    public partial class GanttTask : DependencyObject
    {
        // --- DEPENDENCY PROPERTIES ---

        /// <summary>
        /// Defines the Name dependency property.
        /// </summary>
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register(nameof(Name), typeof(string), typeof(GanttTask), new PropertyMetadata(string.Empty, OnPropertyChanged));

        /// <summary>
        /// Defines the Start dependency property.
        /// </summary>
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register(nameof(Start), typeof(DateTime), typeof(GanttTask), new PropertyMetadata(DateTime.MinValue, OnPropertyChanged));

        /// <summary>
        /// Defines the End dependency property.
        /// </summary>
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register(nameof(End), typeof(DateTime), typeof(GanttTask), new PropertyMetadata(DateTime.MinValue, OnPropertyChanged));

        /// <summary>
        /// Defines the Progress dependency property.
        /// </summary>
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(double), typeof(GanttTask), new PropertyMetadata(0.0, OnPropertyChanged));

        /// <summary>
        /// Defines the CustomBrush dependency property.
        /// </summary>
        public static readonly DependencyProperty CustomBrushProperty =
            DependencyProperty.Register(nameof(CustomBrush), typeof(Brush), typeof(GanttTask), new PropertyMetadata(null, OnPropertyChanged));

        // --- PROPERTIES ---

        /// <summary>
        /// Gets or sets the name/label of the task.
        /// </summary>
        public string Name
        {
            get => (string)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        /// <summary>
        /// Gets or sets the starting date and time of the task.
        /// </summary>
        public DateTime Start
        {
            get => (DateTime)GetValue(StartProperty);
            set => SetValue(StartProperty, value);
        }

        /// <summary>
        /// Gets or sets the ending date and time of the task.
        /// </summary>
        public DateTime End
        {
            get => (DateTime)GetValue(EndProperty);
            set => SetValue(EndProperty, value);
        }

        /// <summary>
        /// Gets or sets the completion ratio from 0.0 to 1.0.
        /// </summary>
        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        /// <summary>
        /// Gets or sets the customized brush to fill the task progress.
        /// </summary>
        public Brush? CustomBrush
        {
            get => (Brush?)GetValue(CustomBrushProperty);
            set => SetValue(CustomBrushProperty, value);
        }

        /// <summary>
        /// Occurs when any property changes.
        /// </summary>
        public event EventHandler? Changed;

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GanttTask self)
            {
                self.Changed?.Invoke(self, EventArgs.Empty);
            }
        }
    }
}
