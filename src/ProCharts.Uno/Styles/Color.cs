using System;

namespace ProCharts.Uno.Styles
{
    public struct Color
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public byte Alpha
        {
            get => A;
            set => A = value;
        }

        public byte Red
        {
            get => R;
            set => R = value;
        }

        public byte Green
        {
            get => G;
            set => G = value;
        }

        public byte Blue
        {
            get => B;
            set => B = value;
        }

        public Color(byte red, byte green, byte blue, byte alpha = 255)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        public Color(Windows.UI.Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        public static implicit operator Windows.UI.Color(Color color) => Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
        public static implicit operator Color(Windows.UI.Color color) => new Color(color);

        public Windows.UI.Color ToColor() => (Windows.UI.Color)this;

        public static Color FromArgb(byte a, byte r, byte g, byte b) => new Color(r, g, b, a);
        public static Color FromRgb(byte r, byte g, byte b) => new Color(r, g, b, 255);

        public static Color Parse(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return new Color(0, 0, 0, 0);
            hex = hex.TrimStart('#');
            if (hex.Length == 8)
            {
                byte a = Convert.ToByte(hex.Substring(0, 2), 16);
                byte r = Convert.ToByte(hex.Substring(2, 2), 16);
                byte g = Convert.ToByte(hex.Substring(4, 2), 16);
                byte b = Convert.ToByte(hex.Substring(6, 2), 16);
                return new Color(r, g, b, a);
            }
            else if (hex.Length == 6)
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                return new Color(r, g, b, 255);
            }
            else if (hex.Length == 4)
            {
                string aStr = new string(hex[0], 2);
                string rStr = new string(hex[1], 2);
                string gStr = new string(hex[2], 2);
                string bStr = new string(hex[3], 2);
                byte a = Convert.ToByte(aStr, 16);
                byte r = Convert.ToByte(rStr, 16);
                byte g = Convert.ToByte(gStr, 16);
                byte b = Convert.ToByte(bStr, 16);
                return new Color(r, g, b, a);
            }
            else if (hex.Length == 3)
            {
                string rStr = new string(hex[0], 2);
                string gStr = new string(hex[1], 2);
                string bStr = new string(hex[2], 2);
                byte r = Convert.ToByte(rStr, 16);
                byte g = Convert.ToByte(gStr, 16);
                byte b = Convert.ToByte(bStr, 16);
                return new Color(r, g, b, 255);
            }
            return new Color(0, 0, 0, 255);
        }
    }
}
