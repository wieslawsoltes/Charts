using System;
using Microsoft.UI.Xaml;

namespace ProCharts.Uno.Series
{
    /// <summary>
    /// Represents a grid-based curved isoline series in a Carpet Plot chart.
    /// </summary>
    public partial class CarpetSeries : ChartSeries
    {
        // --- DEPENDENCY PROPERTIES ---

        /// <summary>
        /// Defines the Rows dependency property.
        /// </summary>
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(nameof(Rows), typeof(int), typeof(CarpetSeries), new PropertyMetadata(0, OnPropertyChanged));

        /// <summary>
        /// Defines the Columns dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(int), typeof(CarpetSeries), new PropertyMetadata(0, OnPropertyChanged));

        /// <summary>
        /// Defines the XValues dependency property. Mapped coordinates in horizontal space.
        /// </summary>
        public static readonly DependencyProperty XValuesProperty =
            DependencyProperty.Register(nameof(XValues), typeof(double[]), typeof(CarpetSeries), new PropertyMetadata(Array.Empty<double>(), OnPropertyChanged));

        /// <summary>
        /// Defines the YValues dependency property. Mapped coordinates in vertical space.
        /// </summary>
        public static readonly DependencyProperty YValuesProperty =
            DependencyProperty.Register(nameof(YValues), typeof(double[]), typeof(CarpetSeries), new PropertyMetadata(Array.Empty<double>(), OnPropertyChanged));

        // --- PROPERTIES ---

        /// <summary>
        /// Gets or sets the count of rows in the parameter grid.
        /// </summary>
        public int Rows
        {
            get => (int)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        /// <summary>
        /// Gets or sets the count of columns in the parameter grid.
        /// </summary>
        public int Columns
        {
            get => (int)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        /// <summary>
        /// Gets or sets the mapped horizontal values as a flat array of length Rows * Columns.
        /// </summary>
        public double[] XValues
        {
            get => (double[])GetValue(XValuesProperty);
            set => SetValue(XValuesProperty, value);
        }

        /// <summary>
        /// Gets or sets the mapped vertical values as a flat array of length Rows * Columns.
        /// </summary>
        public double[] YValues
        {
            get => (double[])GetValue(YValuesProperty);
            set => SetValue(YValuesProperty, value);
        }

        // --- CONSTRUCTOR ---

        /// <summary>
        /// Initializes a new instance of the CarpetSeries class.
        /// </summary>
        public CarpetSeries()
        {
        }
    }
}
