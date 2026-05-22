using System;
using Avalonia;

namespace ProCharts.Avalonia.Series
{
    /// <summary>
    /// Represents a data series in a Nightingale Rose polar area chart.
    /// </summary>
    public class RoseSeries : ChartSeries
    {
        // --- DEPENDENCY PROPERTIES ---

        /// <summary>
        /// Defines the Value dependency property.
        /// </summary>
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<RoseSeries, double>(nameof(Value), 0.0);

        // --- PROPERTIES ---

        /// <summary>
        /// Gets or sets the radial magnitude (length) of the segment.
        /// </summary>
        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        // --- CONSTRUCTOR ---

        /// <summary>
        /// Initializes a new instance of the RoseSeries class.
        /// </summary>
        public RoseSeries()
        {
            ValueProperty.Changed.AddClassHandler<RoseSeries>((x, e) => x.RaiseSeriesChanged());
        }
    }
}
