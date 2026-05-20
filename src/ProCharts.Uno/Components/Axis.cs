using System;
using Microsoft.UI.Xaml;
using Windows.Foundation;

namespace ProCharts.Uno.Components
{
    public enum AxisPosition
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public partial class Axis : DependencyObject
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(Axis), new PropertyMetadata(default(string), OnPropertyChanged));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double?), typeof(Axis), new PropertyMetadata(default(double?), OnPropertyChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double?), typeof(Axis), new PropertyMetadata(default(double?), OnPropertyChanged));

        public static readonly DependencyProperty TickIntervalProperty =
            DependencyProperty.Register(nameof(TickInterval), typeof(double?), typeof(Axis), new PropertyMetadata(default(double?), OnPropertyChanged));

        public static readonly DependencyProperty LabelFormatProperty =
            DependencyProperty.Register(nameof(LabelFormat), typeof(string), typeof(Axis), new PropertyMetadata("G", OnPropertyChanged));

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register(nameof(IsVisible), typeof(bool), typeof(Axis), new PropertyMetadata(true, OnPropertyChanged));

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(AxisPosition), typeof(Axis), new PropertyMetadata(AxisPosition.Bottom, OnPropertyChanged));

        public static readonly DependencyProperty IsLogarithmicProperty =
            DependencyProperty.Register(nameof(IsLogarithmic), typeof(bool), typeof(Axis), new PropertyMetadata(false, OnPropertyChanged));

        public static readonly DependencyProperty IsReversedProperty =
            DependencyProperty.Register(nameof(IsReversed), typeof(bool), typeof(Axis), new PropertyMetadata(false, OnPropertyChanged));

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public double? Minimum
        {
            get => (double?)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double? Maximum
        {
            get => (double?)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public double? TickInterval
        {
            get => (double?)GetValue(TickIntervalProperty);
            set => SetValue(TickIntervalProperty, value);
        }

        public string LabelFormat
        {
            get => (string)GetValue(LabelFormatProperty);
            set => SetValue(LabelFormatProperty, value);
        }

        public bool IsVisible
        {
            get => (bool)GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        public AxisPosition Position
        {
            get => (AxisPosition)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public bool IsLogarithmic
        {
            get => (bool)GetValue(IsLogarithmicProperty);
            set => SetValue(IsLogarithmicProperty, value);
        }

        public bool IsReversed
        {
            get => (bool)GetValue(IsReversedProperty);
            set => SetValue(IsReversedProperty, value);
        }

        public event EventHandler? Changed;

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Axis self)
            {
                self.Changed?.Invoke(self, EventArgs.Empty);
            }
        }

        // --- MATH HELPERS ---

        /// <summary>
        /// Generates a set of tick values between min and max based on standard spacing or user setting.
        /// </summary>
        public double[] GetTicks(double min, double max)
        {
            if (IsLogarithmic)
            {
                var list = new System.Collections.Generic.List<double>();
                double adjMin = min <= 0 ? 1e-5 : min;
                double adjMax = max <= 0 ? 1.0 : max;
                if (adjMin > adjMax) adjMin = adjMax / 10.0;

                double logMin = Math.Log10(adjMin);
                double logMax = Math.Log10(adjMax);

                int startDec = (int)Math.Floor(logMin);
                int endDec = (int)Math.Ceiling(logMax);

                int decadeCount = endDec - startDec;
                if (decadeCount > 10)
                {
                    // Wide span: major decades skipping steps to avoid label overlaps
                    int step = (int)Math.Ceiling(decadeCount / 10.0);
                    for (int d = startDec; d <= endDec; d += step)
                    {
                        double val = Math.Pow(10, d);
                        if (val >= adjMin - 1e-9 && val <= adjMax + 1e-9)
                        {
                            list.Add(val);
                        }
                    }
                }
                else if (decadeCount > 3)
                {
                    // Moderate span: major decades (e.g. 1, 10, 100, 1000)
                    for (int d = startDec; d <= endDec; d++)
                    {
                        double val = Math.Pow(10, d);
                        if (val >= adjMin - 1e-9 && val <= adjMax + 1e-9)
                        {
                            list.Add(val);
                        }
                    }
                }
                else
                {
                    // Narrow span: sub-decade steps (e.g. 1, 2, 3, ..., 9, 10)
                    for (int d = startDec - 1; d <= endDec + 1; d++)
                    {
                        double baseVal = Math.Pow(10, d);
                        for (int i = 1; i <= 9; i++)
                        {
                            double val = baseVal * i;
                            if (val >= adjMin - 1e-9 && val <= adjMax + 1e-9)
                            {
                                if (!list.Contains(val)) list.Add(val);
                            }
                        }
                    }
                }

                if (list.Count == 0)
                {
                    list.Add(adjMin);
                    list.Add(adjMax);
                }

                return list.ToArray();
            }
            else
            {
                double interval = TickInterval ?? CalculateDefaultInterval(min, max);
                if (interval <= 0) interval = 1.0;

                // Align start to the nearest interval tick
                double start = Math.Ceiling(min / interval) * interval;
                
                var list = new System.Collections.Generic.List<double>();
                for (double val = start; val <= max + 1e-9; val += interval)
                {
                    list.Add(val);
                    // Prevent infinite loop if interval is extremely tiny
                    if (list.Count > 100) break;
                }

                return list.ToArray();
            }
        }

        private double CalculateDefaultInterval(double min, double max)
        {
            double range = max - min;
            if (range <= 0) return 1.0;

            // Nice intervals: 1, 2, 5, 10, etc.
            double tempInterval = range / 5.0; // aim for 5 ticks
            double exponent = Math.Floor(Math.Log10(tempInterval));
            double fraction = tempInterval / Math.Pow(10, exponent);

            double niceFraction = fraction switch
            {
                < 1.5 => 1.0,
                < 3.0 => 2.0,
                < 7.0 => 5.0,
                _ => 10.0
            };

            return niceFraction * Math.Pow(10, exponent);
        }
    }
}