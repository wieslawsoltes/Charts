using System;
using Avalonia;
using Avalonia.Media;

namespace ProCharts.Avalonia.Components
{
    /// <summary>
    /// Represents a geospatial bubble item plotted in the BubbleMapChart.
    /// </summary>
    public class MapBubbleItem : AvaloniaObject
    {
        // --- DEPENDENCY PROPERTIES ---

        /// <summary>
        /// Defines the Label dependency property.
        /// </summary>
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<MapBubbleItem, string>(nameof(Label), string.Empty);

        /// <summary>
        /// Defines the Latitude dependency property. Bounds range from -90.0 to 90.0.
        /// </summary>
        public static readonly StyledProperty<double> LatitudeProperty =
            AvaloniaProperty.Register<MapBubbleItem, double>(nameof(Latitude), 0.0);

        /// <summary>
        /// Defines the Longitude dependency property. Bounds range from -180.0 to 180.0.
        /// </summary>
        public static readonly StyledProperty<double> LongitudeProperty =
            AvaloniaProperty.Register<MapBubbleItem, double>(nameof(Longitude), 0.0);

        /// <summary>
        /// Defines the Value dependency property, representing the data scale magnitude of the bubble.
        /// </summary>
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<MapBubbleItem, double>(nameof(Value), 0.0);

        /// <summary>
        /// Defines the Color dependency property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> ColorProperty =
            AvaloniaProperty.Register<MapBubbleItem, IBrush?>(nameof(Color), null);

        // --- PROPERTIES ---

        /// <summary>
        /// Gets or sets the label/identifier for the mapped location.
        /// </summary>
        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the geographic latitude.
        /// </summary>
        public double Latitude
        {
            get => GetValue(LatitudeProperty);
            set => SetValue(LatitudeProperty, value);
        }

        /// <summary>
        /// Gets or sets the geographic longitude.
        /// </summary>
        public double Longitude
        {
            get => GetValue(LongitudeProperty);
            set => SetValue(LongitudeProperty, value);
        }

        /// <summary>
        /// Gets or sets the value used to determine the bubble size.
        /// </summary>
        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the customized fill brush of the map bubble marker.
        /// </summary>
        public IBrush? Color
        {
            get => GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }
    }
}
