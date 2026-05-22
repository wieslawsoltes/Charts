using System;
using Microsoft.UI.Xaml;

namespace ProCharts.Uno.Series
{
    /// <summary>
    /// Represents a data series in a Nightingale Rose polar area chart.
    /// </summary>
    public partial class RoseSeries : ChartSeries
    {
        // --- DEPENDENCY PROPERTIES ---

        /// <summary>
        /// Defines the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(RoseSeries), new PropertyMetadata(0.0, OnPropertyChanged));

        // --- PROPERTIES ---

        /// <summary>
        /// Gets or sets the radial magnitude (length) of the segment.
        /// </summary>
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        // --- CONSTRUCTOR ---

        /// <summary>
        /// Initializes a new instance of the RoseSeries class.
        /// </summary>
        public RoseSeries()
        {
        }
    }
}
