using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;

namespace ProCharts.Uno.Styles
{
    public class Palette
    {
        public string Name { get; }
        public IReadOnlyList<Brush> Brushes { get; }

        public Palette(string name, IEnumerable<Brush> brushes)
        {
            Name = name;
            Brushes = new List<Brush>(brushes).AsReadOnly();
        }

        public Palette(string name, IEnumerable<Color> colors)
        {
            Name = name;
            var list = new List<Brush>();
            foreach (var color in colors)
            {
                list.Add(new SolidColorBrush(color));
            }
            Brushes = list.AsReadOnly();
        }

        public Brush GetBrush(int index)
        {
            if (Brushes.Count == 0)
                return new SolidColorBrush(Color.Parse("#2196F3"));

            return Brushes[index % Brushes.Count];
        }

        // --- PREBUILT PREMIUM PALETTES ---

        /// <summary>
        /// A sleek, professional modern palette with rich blues, teals, and purples.
        /// </summary>
        public static Palette Modern { get; } = new Palette("Modern", new[]
        {
            Color.Parse("#4F46E5"), // Indigo
            Color.Parse("#06B6D4"), // Cyan
            Color.Parse("#10B981"), // Emerald
            Color.Parse("#F59E0B"), // Amber
            Color.Parse("#8B5CF6"), // Purple
            Color.Parse("#EC4899"), // Pink
            Color.Parse("#3B82F6")  // Blue
        });

        /// <summary>
        /// A fresh organic palette containing shades of mint, sage, and deep greens.
        /// </summary>
        public static Palette Emerald { get; } = new Palette("Emerald", new[]
        {
            Color.Parse("#059669"), // Deep Emerald
            Color.Parse("#34D399"), // Mint
            Color.Parse("#10B981"), // Emerald
            Color.Parse("#A7F3D0"), // Soft Green
            Color.Parse("#065F46"), // Forest
            Color.Parse("#6EE7B7")  // Light Green
        });

        /// <summary>
        /// Slate Dark is suited for premium dark mode designs with muted lavenders, steel blues, and charcoal highlights.
        /// </summary>
        public static Palette SlateDark { get; } = new Palette("SlateDark", new[]
        {
            Color.Parse("#6366F1"), // Cool Indigo
            Color.Parse("#818CF8"), // Soft Indigo
            Color.Parse("#38BDF8"), // Sky Blue
            Color.Parse("#A5B4FC"), // Lavender
            Color.Parse("#94A3B8"), // Slate
            Color.Parse("#C084FC")  // Orchid
        });

        /// <summary>
        /// A retro, warm pastel palette with salmon, terracotta, mustard, and sage.
        /// </summary>
        public static Palette Retro { get; } = new Palette("Retro", new[]
        {
            Color.Parse("#E11D48"), // Rose
            Color.Parse("#F97316"), // Orange
            Color.Parse("#FACC15"), // Yellow
            Color.Parse("#14B8A6"), // Teal
            Color.Parse("#65A30D"), // Lime
            Color.Parse("#D946EF")  // Magenta
        });

        /// <summary>
        /// High-contrast neon cyberpunk palette with neon pink, neon blue, and electric yellow.
        /// </summary>
        public static Palette Cyberpunk { get; } = new Palette("Cyberpunk", new[]
        {
            Color.Parse("#FF007F"), // Neon Pink
            Color.Parse("#00F0FF"), // Neon Cyan
            Color.Parse("#FFEA00"), // Electric Yellow
            Color.Parse("#BD00FF"), // Electric Purple
            Color.Parse("#00FF66"), // Neon Green
            Color.Parse("#FF5E00")  // Neon Orange
        });

        /// <summary>
        /// Default fallback palette.
        /// </summary>
        public static Palette Default => Modern;
    }
}