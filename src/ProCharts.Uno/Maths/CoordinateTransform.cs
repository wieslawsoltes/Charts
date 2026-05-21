using System;
using Microsoft.UI.Xaml;
using Windows.Foundation;


namespace ProCharts.Uno.Maths
{
    public class CoordinateTransform
    {
        public double XMin { get; }
        public double XMax { get; }
        public double YMin { get; }
        public double YMax { get; }
        
        public Rect PlotArea { get; }
        
        public bool IsXLogarithmic { get; }
        public bool IsYLogarithmic { get; }
        
        public bool IsXReversed { get; }
        public bool IsYReversed { get; }

        private readonly double _xRange;
        private readonly double _yRange;
        private readonly double _logXMin;
        private readonly double _logXMax;
        private readonly double _logXRange;
        private readonly double _logYMin;
        private readonly double _logYMax;
        private readonly double _logYRange;

        public CoordinateTransform(
            Rect plotArea,
            double xMin, double xMax,
            double yMin, double yMax,
            bool isXLog = false, bool isYLog = false,
            bool isXRev = false, bool isYRev = false)
        {
            PlotArea = plotArea;
            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;
            
            IsXLogarithmic = isXLog;
            IsYLogarithmic = isYLog;
            IsXReversed = isXRev;
            IsYReversed = isYRev;

            // Handle equal min/max to avoid division by zero
            if (Math.Abs(XMax - XMin) < 1e-9)
            {
                XMax = XMin + 1.0;
            }
            if (Math.Abs(YMax - YMin) < 1e-9)
            {
                YMax = YMin + 1.0;
            }

            _xRange = XMax - XMin;
            _yRange = YMax - YMin;

            if (IsXLogarithmic)
            {
                var adjustedXMin = XMin <= 0 ? 1e-5 : XMin;
                var adjustedXMax = XMax <= 0 ? 1.0 : XMax;
                _logXMin = Math.Log10(adjustedXMin);
                _logXMax = Math.Log10(adjustedXMax);
                _logXRange = _logXMax - _logXMin;
                if (Math.Abs(_logXRange) < 1e-9) _logXRange = 1.0;
            }

            if (IsYLogarithmic)
            {
                var adjustedYMin = YMin <= 0 ? 1e-5 : YMin;
                var adjustedYMax = YMax <= 0 ? 1.0 : YMax;
                _logYMin = Math.Log10(adjustedYMin);
                _logYMax = Math.Log10(adjustedYMax);
                _logYRange = _logYMax - _logYMin;
                if (Math.Abs(_logYRange) < 1e-9) _logYRange = 1.0;
            }
        }

        /// <summary>
        /// Transforms a data value (X, Y) into a screen pixel Point.
        /// </summary>
        public Point ToScreen(double x, double y)
        {
            double pctX = GetPctX(x);
            double pctY = GetPctY(y);

            double px = PlotArea.Left + pctX * PlotArea.Width;
            double py = PlotArea.Top + (1.0 - pctY) * PlotArea.Height; // Invert Y for screen coordinates

            return new Point(px, py);
        }

        /// <summary>
        /// Transforms a screen pixel Point back into data values (X, Y).
        /// </summary>
        public Point ToData(Point screenPoint)
        {
            double pctX = (screenPoint.X - PlotArea.Left) / PlotArea.Width;
            double pctY = 1.0 - (screenPoint.Y - PlotArea.Top) / PlotArea.Height; // Invert Y back

            double x = GetDataX(pctX);
            double y = GetDataY(pctY);

            return new Point(x, y);
        }

        private double GetPctX(double x)
        {
            double pct;
            if (IsXLogarithmic)
            {
                var val = x <= 0 ? 1e-5 : x;
                pct = (Math.Log10(val) - _logXMin) / _logXRange;
            }
            else
            {
                pct = (x - XMin) / _xRange;
            }

            if (IsXReversed)
            {
                pct = 1.0 - pct;
            }

            return Math.Clamp(pct, 0.0, 1.0);
        }

        private double GetPctY(double y)
        {
            double pct;
            if (IsYLogarithmic)
            {
                var val = y <= 0 ? 1e-5 : y;
                pct = (Math.Log10(val) - _logYMin) / _logYRange;
            }
            else
            {
                pct = (y - YMin) / _yRange;
            }

            if (IsYReversed)
            {
                pct = 1.0 - pct;
            }

            return Math.Clamp(pct, 0.0, 1.0);
        }

        private double GetDataX(double pct)
        {
            if (IsXReversed)
            {
                pct = 1.0 - pct;
            }

            if (IsXLogarithmic)
            {
                double logVal = _logXMin + pct * _logXRange;
                return Math.Pow(10.0, logVal);
            }
            else
            {
                return XMin + pct * _xRange;
            }
        }

        private double GetDataY(double pct)
        {
            if (IsYReversed)
            {
                pct = 1.0 - pct;
            }

            if (IsYLogarithmic)
            {
                double logVal = _logYMin + pct * _logYRange;
                return Math.Pow(10.0, logVal);
            }
            else
            {
                return YMin + pct * _yRange;
            }
        }
    }
}