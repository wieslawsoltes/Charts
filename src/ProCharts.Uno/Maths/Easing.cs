using Windows.Foundation;
using System;

namespace ProCharts.Uno.Maths
{
    public enum EasingType
    {
        Linear,
        QuadraticEaseIn,
        QuadraticEaseOut,
        QuadraticEaseInOut,
        CubicEaseIn,
        CubicEaseOut,
        CubicEaseInOut,
        QuarticEaseOut,
        QuinticEaseOut,
        ElasticEaseOut,
        BounceEaseOut
    }

    public static class Easing
    {
        public static double Interpolate(double t, EasingType type)
        {
            // Clamp t between 0 and 1
            t = Math.Max(0.0, Math.Min(1.0, t));

            return type switch
            {
                EasingType.Linear => t,
                
                EasingType.QuadraticEaseIn => t * t,
                
                EasingType.QuadraticEaseOut => t * (2 - t),
                
                EasingType.QuadraticEaseInOut => t < 0.5 
                    ? 2 * t * t 
                    : -1 + (4 - 2 * t) * t,
                
                EasingType.CubicEaseIn => t * t * t,
                
                EasingType.CubicEaseOut => 1.0 - Math.Pow(1.0 - t, 3.0),
                
                EasingType.CubicEaseInOut => t < 0.5 
                    ? 4.0 * t * t * t 
                    : 1.0 - Math.Pow(-2.0 * t + 2.0, 3.0) / 2.0,
                
                EasingType.QuarticEaseOut => 1.0 - Math.Pow(1.0 - t, 4.0),
                
                EasingType.QuinticEaseOut => 1.0 - Math.Pow(1.0 - t, 5.0),
                
                EasingType.ElasticEaseOut => t == 0.0 ? 0.0 : t == 1.0 ? 1.0 :
                    Math.Pow(2.0, -10.0 * t) * Math.Sin((t * 10.0 - 0.75) * (2.0 * Math.PI) / 3.0) + 1.0,
                
                EasingType.BounceEaseOut => Bounce(t),
                
                _ => t
            };
        }

        private static double Bounce(double t)
        {
            const double n1 = 7.5625;
            const double d1 = 2.75;

            if (t < 1.0 / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2.0 / d1)
            {
                return n1 * (t -= 1.5 / d1) * t + 0.75;
            }
            else if (t < 2.5 / d1)
            {
                return n1 * (t -= 2.25 / d1) * t + 0.9375;
            }
            else
            {
                return n1 * (t -= 2.625 / d1) * t + 0.984375;
            }
        }
    }
}