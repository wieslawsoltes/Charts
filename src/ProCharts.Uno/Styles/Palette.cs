using System.Collections.Generic;
using Windows.Foundation;
using SkiaSharp;

namespace ProCharts.Uno.Styles
{
    public class Palette
    {
        public string Name { get; }
        public IReadOnlyList<SKPaint> SKPaintes { get; }

        public Palette(string name, IEnumerable<SKPaint> brushes)
        {
            Name = name;
            SKPaintes = new List<SKPaint>(brushes).AsReadOnly();
        }

        public Palette(string name, IEnumerable<SKColor> colors)
        {
            Name = name;
            var list = new List<SKPaint>();
            foreach (var color in colors)
            {
                list.Add(new SKPaint { Color = color, Style = SKPaintStyle.Fill, IsAntialias = true });
            }
            SKPaintes = list.AsReadOnly();
        }

        public SKPaint GetSKPaint(int index)
        {
            if (SKPaintes.Count == 0)
                return new SKPaint { Color = SKColor.Parse("#2196F3"), Style = SKPaintStyle.Fill, IsAntialias = true };

            return SKPaintes[index % SKPaintes.Count];
        }

        // --- PREBUILT PREMIUM PALETTES ---

        /// <summary>
        /// A sleek, professional modern palette with rich blues, teals, and purples.
        /// </summary>
        public static Palette Modern { get; } = new Palette("Modern", new[]
        {
            SKColor.Parse("#4F46E5"), // Indigo
            SKColor.Parse("#06B6D4"), // Cyan
            SKColor.Parse("#10B981"), // Emerald
            SKColor.Parse("#F59E0B"), // Amber
            SKColor.Parse("#8B5CF6"), // Purple
            SKColor.Parse("#EC4899"), // Pink
            SKColor.Parse("#3B82F6")  // Blue
        });

        /// <summary>
        /// A fresh organic palette containing shades of mint, sage, and deep greens.
        /// </summary>
        public static Palette Emerald { get; } = new Palette("Emerald", new[]
        {
            SKColor.Parse("#059669"), // Deep Emerald
            SKColor.Parse("#34D399"), // Mint
            SKColor.Parse("#10B981"), // Emerald
            SKColor.Parse("#A7F3D0"), // Soft Green
            SKColor.Parse("#065F46"), // Forest
            SKColor.Parse("#6EE7B7")  // Light Green
        });

        /// <summary>
        /// Slate Dark is suited for premium dark mode designs with muted lavenders, steel blues, and charcoal highlights.
        /// </summary>
        public static Palette SlateDark { get; } = new Palette("SlateDark", new[]
        {
            SKColor.Parse("#6366F1"), // Cool Indigo
            SKColor.Parse("#818CF8"), // Soft Indigo
            SKColor.Parse("#38BDF8"), // Sky Blue
            SKColor.Parse("#A5B4FC"), // Lavender
            SKColor.Parse("#94A3B8"), // Slate
            SKColor.Parse("#C084FC")  // Orchid
        });

        /// <summary>
        /// A retro, warm pastel palette with salmon, terracotta, mustard, and sage.
        /// </summary>
        public static Palette Retro { get; } = new Palette("Retro", new[]
        {
            SKColor.Parse("#E11D48"), // Rose
            SKColor.Parse("#F97316"), // Orange
            SKColor.Parse("#FACC15"), // Yellow
            SKColor.Parse("#14B8A6"), // Teal
            SKColor.Parse("#65A30D"), // Lime
            SKColor.Parse("#D946EF")  // Magenta
        });

        /// <summary>
        /// High-contrast neon cyberpunk palette with neon pink, neon blue, and electric yellow.
        /// </summary>
        public static Palette Cyberpunk { get; } = new Palette("Cyberpunk", new[]
        {
            SKColor.Parse("#FF007F"), // Neon Pink
            SKColor.Parse("#00F0FF"), // Neon Cyan
            SKColor.Parse("#FFEA00"), // Electric Yellow
            SKColor.Parse("#BD00FF"), // Electric Purple
            SKColor.Parse("#00FF66"), // Neon Green
            SKColor.Parse("#FF5E00")  // Neon Orange
        });

        /// <summary>
        /// Default fallback palette.
        /// </summary>
        public static Palette Default => Modern;
    }
}