using System;
using Avalonia;

namespace ProCharts.Avalonia.Series
{
    /// <summary>
    /// Represents a grid-based curved isoline series in a Carpet Plot chart.
    /// </summary>
    public class CarpetSeries : ChartSeries
    {
        // --- DEPENDENCY PROPERTIES ---

        /// <summary>
        /// Defines the Rows dependency property.
        /// </summary>
        public static readonly StyledProperty<int> RowsProperty =
            AvaloniaProperty.Register<CarpetSeries, int>(nameof(Rows), 0);

        /// <summary>
        /// Defines the Columns dependency property.
        /// </summary>
        public static readonly StyledProperty<int> ColumnsProperty =
            AvaloniaProperty.Register<CarpetSeries, int>(nameof(Columns), 0);

        /// <summary>
        /// Defines the XValues dependency property. Mapped coordinates in horizontal space.
        /// </summary>
        public static readonly StyledProperty<double[]> XValuesProperty =
            AvaloniaProperty.Register<CarpetSeries, double[]>(nameof(XValues), Array.Empty<double>());

        /// <summary>
        /// Defines the YValues dependency property. Mapped coordinates in vertical space.
        /// </summary>
        public static readonly StyledProperty<double[]> YValuesProperty =
            AvaloniaProperty.Register<CarpetSeries, double[]>(nameof(YValues), Array.Empty<double>());

        // --- PROPERTIES ---

        /// <summary>
        /// Gets or sets the count of rows in the parameter grid.
        /// </summary>
        public int Rows
        {
            get => GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        /// <summary>
        /// Gets or sets the count of columns in the parameter grid.
        /// </summary>
        public int Columns
        {
            get => GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        /// <summary>
        /// Gets or sets the mapped horizontal values as a flat array of length Rows * Columns.
        /// </summary>
        public double[] XValues
        {
            get => GetValue(XValuesProperty);
            set => SetValue(XValuesProperty, value);
        }

        /// <summary>
        /// Gets or sets the mapped vertical values as a flat array of length Rows * Columns.
        /// </summary>
        public double[] YValues
        {
            get => GetValue(YValuesProperty);
            set => SetValue(YValuesProperty, value);
        }

        // --- CONSTRUCTOR ---

        /// <summary>
        /// Initializes a new instance of the CarpetSeries class.
        /// </summary>
        public CarpetSeries()
        {
            RowsProperty.Changed.AddClassHandler<CarpetSeries>((x, e) => x.RaiseSeriesChanged());
            ColumnsProperty.Changed.AddClassHandler<CarpetSeries>((x, e) => x.RaiseSeriesChanged());
            XValuesProperty.Changed.AddClassHandler<CarpetSeries>((x, e) => x.RaiseSeriesChanged());
            YValuesProperty.Changed.AddClassHandler<CarpetSeries>((x, e) => x.RaiseSeriesChanged());
        }
    }
}
