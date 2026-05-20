using Avalonia;
using Avalonia.Media;

namespace ProCharts.Components
{
    public enum AxisPosition
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public class Axis : AvaloniaObject
    {
        public static readonly StyledProperty<string?> TitleProperty =
            AvaloniaProperty.Register<Axis, string?>(nameof(Title));

        public static readonly StyledProperty<double?> MinimumProperty =
            AvaloniaProperty.Register<Axis, double?>(nameof(Minimum));

        public static readonly StyledProperty<double?> MaximumProperty =
            AvaloniaProperty.Register<Axis, double?>(nameof(Maximum));

        public static readonly StyledProperty<double?> TickIntervalProperty =
            AvaloniaProperty.Register<Axis, double?>(nameof(TickInterval));

        public static readonly StyledProperty<string> LabelFormatProperty =
            AvaloniaProperty.Register<Axis, string>(nameof(LabelFormat), "G");

        public static readonly StyledProperty<bool> IsVisibleProperty =
            AvaloniaProperty.Register<Axis, bool>(nameof(IsVisible), true);

        public static readonly StyledProperty<AxisPosition> PositionProperty =
            AvaloniaProperty.Register<Axis, AxisPosition>(nameof(Position), AxisPosition.Bottom);

        public static readonly StyledProperty<bool> IsLogarithmicProperty =
            AvaloniaProperty.Register<Axis, bool>(nameof(IsLogarithmic), false);

        public static readonly StyledProperty<bool> IsReversedProperty =
            AvaloniaProperty.Register<Axis, bool>(nameof(IsReversed), false);

        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public double? Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double? Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public double? TickInterval
        {
            get => GetValue(TickIntervalProperty);
            set => SetValue(TickIntervalProperty, value);
        }

        public string LabelFormat
        {
            get => GetValue(LabelFormatProperty);
            set => SetValue(LabelFormatProperty, value);
        }

        public bool IsVisible
        {
            get => GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        public AxisPosition Position
        {
            get => GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public bool IsLogarithmic
        {
            get => GetValue(IsLogarithmicProperty);
            set => SetValue(IsLogarithmicProperty, value);
        }

        public bool IsReversed
        {
            get => GetValue(IsReversedProperty);
            set => SetValue(IsReversedProperty, value);
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
