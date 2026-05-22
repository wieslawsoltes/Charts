using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace ProCharts.Uno.Components
{
    /// <summary>
    /// Represents a geospatial bubble item plotted in the BubbleMapChart.
    /// </summary>
    public partial class MapBubbleItem : DependencyObject
    {
        // --- DEPENDENCY PROPERTIES ---

        /// <summary>
        /// Defines the Label dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(MapBubbleItem), new PropertyMetadata(string.Empty, OnPropertyChanged));

        /// <summary>
        /// Defines the Latitude dependency property. Bounds range from -90.0 to 90.0.
        /// </summary>
        public static readonly DependencyProperty LatitudeProperty =
            DependencyProperty.Register(nameof(Latitude), typeof(double), typeof(MapBubbleItem), new PropertyMetadata(0.0, OnPropertyChanged));

        /// <summary>
        /// Defines the Longitude dependency property. Bounds range from -180.0 to 180.0.
        /// </summary>
        public static readonly DependencyProperty LongitudeProperty =
            DependencyProperty.Register(nameof(Longitude), typeof(double), typeof(MapBubbleItem), new PropertyMetadata(0.0, OnPropertyChanged));

        /// <summary>
        /// Defines the Value dependency property, representing the data scale magnitude of the bubble.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(MapBubbleItem), new PropertyMetadata(0.0, OnPropertyChanged));

        /// <summary>
        /// Defines the Color dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Brush), typeof(MapBubbleItem), new PropertyMetadata(null, OnPropertyChanged));

        // --- PROPERTIES ---

        /// <summary>
        /// Gets or sets the label/identifier for the mapped location.
        /// </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the geographic latitude.
        /// </summary>
        public double Latitude
        {
            get => (double)GetValue(LatitudeProperty);
            set => SetValue(LatitudeProperty, value);
        }

        /// <summary>
        /// Gets or sets the geographic longitude.
        /// </summary>
        public double Longitude
        {
            get => (double)GetValue(LongitudeProperty);
            set => SetValue(LongitudeProperty, value);
        }

        /// <summary>
        /// Gets or sets the value used to determine the bubble size.
        /// </summary>
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the customized fill brush of the map bubble marker.
        /// </summary>
        public Brush? Color
        {
            get => (Brush?)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        /// <summary>
        /// Occurs when any property changes.
        /// </summary>
        public event EventHandler? Changed;

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MapBubbleItem self)
            {
                self.Changed?.Invoke(self, EventArgs.Empty);
            }
        }
    }
}
