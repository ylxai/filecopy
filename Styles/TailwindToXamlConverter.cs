using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace FileCopyUtility.Styles
{
    /// <summary>
    /// Converts Tailwind CSS v4 color palette and design tokens to XAML Resources
    /// </summary>
    public static class TailwindToXamlConverter
    {
        // Tailwind v4 Color Palette
        public static readonly Dictionary<string, System.Windows.Media.Color> TailwindColors = new()
        {
            // Gray Scale
            ["slate-50"] = ColorFromHex("#f8fafc"),
            ["slate-100"] = ColorFromHex("#f1f5f9"),
            ["slate-200"] = ColorFromHex("#e2e8f0"),
            ["slate-300"] = ColorFromHex("#cbd5e1"),
            ["slate-400"] = ColorFromHex("#94a3b8"),
            ["slate-500"] = ColorFromHex("#64748b"),
            ["slate-600"] = ColorFromHex("#475569"),
            ["slate-700"] = ColorFromHex("#334155"),
            ["slate-800"] = ColorFromHex("#1e293b"),
            ["slate-900"] = ColorFromHex("#0f172a"),
            ["slate-950"] = ColorFromHex("#020617"),

            // Gray
            ["gray-50"] = ColorFromHex("#f9fafb"),
            ["gray-100"] = ColorFromHex("#f3f4f6"),
            ["gray-200"] = ColorFromHex("#e5e7eb"),
            ["gray-300"] = ColorFromHex("#d1d5db"),
            ["gray-400"] = ColorFromHex("#9ca3af"),
            ["gray-500"] = ColorFromHex("#6b7280"),
            ["gray-600"] = ColorFromHex("#4b5563"),
            ["gray-700"] = ColorFromHex("#374151"),
            ["gray-800"] = ColorFromHex("#1f2937"),
            ["gray-900"] = ColorFromHex("#111827"),
            ["gray-950"] = ColorFromHex("#030712"),

            // Blue
            ["blue-50"] = ColorFromHex("#eff6ff"),
            ["blue-100"] = ColorFromHex("#dbeafe"),
            ["blue-200"] = ColorFromHex("#bfdbfe"),
            ["blue-300"] = ColorFromHex("#93c5fd"),
            ["blue-400"] = ColorFromHex("#60a5fa"),
            ["blue-500"] = ColorFromHex("#3b82f6"),
            ["blue-600"] = ColorFromHex("#2563eb"),
            ["blue-700"] = ColorFromHex("#1d4ed8"),
            ["blue-800"] = ColorFromHex("#1e40af"),
            ["blue-900"] = ColorFromHex("#1e3a8a"),
            ["blue-950"] = ColorFromHex("#172554"),

            // Green
            ["green-50"] = ColorFromHex("#f0fdf4"),
            ["green-100"] = ColorFromHex("#dcfce7"),
            ["green-200"] = ColorFromHex("#bbf7d0"),
            ["green-300"] = ColorFromHex("#86efac"),
            ["green-400"] = ColorFromHex("#4ade80"),
            ["green-500"] = ColorFromHex("#22c55e"),
            ["green-600"] = ColorFromHex("#16a34a"),
            ["green-700"] = ColorFromHex("#15803d"),
            ["green-800"] = ColorFromHex("#166534"),
            ["green-900"] = ColorFromHex("#14532d"),
            ["green-950"] = ColorFromHex("#052e16"),

            // Red
            ["red-50"] = ColorFromHex("#fef2f2"),
            ["red-100"] = ColorFromHex("#fee2e2"),
            ["red-200"] = ColorFromHex("#fecaca"),
            ["red-300"] = ColorFromHex("#fca5a5"),
            ["red-400"] = ColorFromHex("#f87171"),
            ["red-500"] = ColorFromHex("#ef4444"),
            ["red-600"] = ColorFromHex("#dc2626"),
            ["red-700"] = ColorFromHex("#b91c1c"),
            ["red-800"] = ColorFromHex("#991b1b"),
            ["red-900"] = ColorFromHex("#7f1d1d"),
            ["red-950"] = ColorFromHex("#450a0a"),

            // Yellow/Amber
            ["amber-50"] = ColorFromHex("#fffbeb"),
            ["amber-100"] = ColorFromHex("#fef3c7"),
            ["amber-200"] = ColorFromHex("#fde68a"),
            ["amber-300"] = ColorFromHex("#fcd34d"),
            ["amber-400"] = ColorFromHex("#fbbf24"),
            ["amber-500"] = ColorFromHex("#f59e0b"),
            ["amber-600"] = ColorFromHex("#d97706"),
            ["amber-700"] = ColorFromHex("#b45309"),
            ["amber-800"] = ColorFromHex("#92400e"),
            ["amber-900"] = ColorFromHex("#78350f"),
            ["amber-950"] = ColorFromHex("#451a03"),

            // Purple
            ["purple-50"] = ColorFromHex("#faf5ff"),
            ["purple-100"] = ColorFromHex("#f3e8ff"),
            ["purple-200"] = ColorFromHex("#e9d5ff"),
            ["purple-300"] = ColorFromHex("#d8b4fe"),
            ["purple-400"] = ColorFromHex("#c084fc"),
            ["purple-500"] = ColorFromHex("#a855f7"),
            ["purple-600"] = ColorFromHex("#9333ea"),
            ["purple-700"] = ColorFromHex("#7c3aed"),
            ["purple-800"] = ColorFromHex("#6b21a8"),
            ["purple-900"] = ColorFromHex("#581c87"),
            ["purple-950"] = ColorFromHex("#3b0764"),

            // Indigo
            ["indigo-50"] = ColorFromHex("#eef2ff"),
            ["indigo-100"] = ColorFromHex("#e0e7ff"),
            ["indigo-200"] = ColorFromHex("#c7d2fe"),
            ["indigo-300"] = ColorFromHex("#a5b4fc"),
            ["indigo-400"] = ColorFromHex("#818cf8"),
            ["indigo-500"] = ColorFromHex("#6366f1"),
            ["indigo-600"] = ColorFromHex("#4f46e5"),
            ["indigo-700"] = ColorFromHex("#4338ca"),
            ["indigo-800"] = ColorFromHex("#3730a3"),
            ["indigo-900"] = ColorFromHex("#312e81"),
            ["indigo-950"] = ColorFromHex("#1e1b4b"),
        };

        // Tailwind Spacing Scale (converted to WPF units)
        public static readonly Dictionary<string, double> TailwindSpacing = new()
        {
            ["0"] = 0,
            ["px"] = 1,
            ["0.5"] = 2,
            ["1"] = 4,
            ["1.5"] = 6,
            ["2"] = 8,
            ["2.5"] = 10,
            ["3"] = 12,
            ["3.5"] = 14,
            ["4"] = 16,
            ["5"] = 20,
            ["6"] = 24,
            ["7"] = 28,
            ["8"] = 32,
            ["9"] = 36,
            ["10"] = 40,
            ["11"] = 44,
            ["12"] = 48,
            ["14"] = 56,
            ["16"] = 64,
            ["20"] = 80,
            ["24"] = 96,
            ["28"] = 112,
            ["32"] = 128,
            ["36"] = 144,
            ["40"] = 160,
            ["44"] = 176,
            ["48"] = 192,
            ["52"] = 208,
            ["56"] = 224,
            ["60"] = 240,
            ["64"] = 256,
            ["72"] = 288,
            ["80"] = 320,
            ["96"] = 384,
        };

        // Tailwind Shadow Definitions
        public static readonly Dictionary<string, string> TailwindShadows = new()
        {
            ["shadow-sm"] = "0 1 2 0 rgba(0, 0, 0, 0.05)",
            ["shadow"] = "0 1 3 0 rgba(0, 0, 0, 0.1), 0 1 2 -1 rgba(0, 0, 0, 0.1)",
            ["shadow-md"] = "0 4 6 -1 rgba(0, 0, 0, 0.1), 0 2 4 -2 rgba(0, 0, 0, 0.1)",
            ["shadow-lg"] = "0 10 15 -3 rgba(0, 0, 0, 0.1), 0 4 6 -4 rgba(0, 0, 0, 0.1)",
            ["shadow-xl"] = "0 20 25 -5 rgba(0, 0, 0, 0.1), 0 8 10 -6 rgba(0, 0, 0, 0.1)",
            ["shadow-2xl"] = "0 25 50 -12 rgba(0, 0, 0, 0.25)",
        };

        public static System.Windows.Media.Color ColorFromHex(string hex)
        {
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
        }

        public static SolidColorBrush BrushFromTailwind(string tailwindColor)
        {
            if (TailwindColors.TryGetValue(tailwindColor, out System.Windows.Media.Color color))
            {
                return new SolidColorBrush(color);
            }
            return new SolidColorBrush(System.Windows.Media.Colors.Transparent);
        }

        public static Thickness ThicknessFromTailwind(string className)
        {
            // Parse Tailwind margin/padding classes
            var parts = className.Split('-');
            if (parts.Length < 2) return new Thickness(0);

            var direction = parts[0]; // p, m, px, py, pt, pb, pl, pr, mx, my, mt, mb, ml, mr
            var value = parts[1];

            if (!TailwindSpacing.TryGetValue(value, out double spacing))
                return new Thickness(0);

            return direction switch
            {
                "p" => new Thickness(spacing), // padding all
                "px" => new Thickness(spacing, 0, spacing, 0), // padding horizontal
                "py" => new Thickness(0, spacing, 0, spacing), // padding vertical
                "pt" => new Thickness(0, spacing, 0, 0), // padding top
                "pb" => new Thickness(0, 0, 0, spacing), // padding bottom
                "pl" => new Thickness(spacing, 0, 0, 0), // padding left
                "pr" => new Thickness(0, 0, spacing, 0), // padding right
                "m" => new Thickness(spacing), // margin all
                "mx" => new Thickness(spacing, 0, spacing, 0), // margin horizontal
                "my" => new Thickness(0, spacing, 0, spacing), // margin vertical
                "mt" => new Thickness(0, spacing, 0, 0), // margin top
                "mb" => new Thickness(0, 0, 0, spacing), // margin bottom
                "ml" => new Thickness(spacing, 0, 0, 0), // margin left
                "mr" => new Thickness(0, 0, spacing, 0), // margin right
                _ => new Thickness(0)
            };
        }

        public static CornerRadius BorderRadiusFromTailwind(string className)
        {
            var radiusMap = new Dictionary<string, double>
            {
                ["rounded-none"] = 0,
                ["rounded-sm"] = 2,
                ["rounded"] = 4,
                ["rounded-md"] = 6,
                ["rounded-lg"] = 8,
                ["rounded-xl"] = 12,
                ["rounded-2xl"] = 16,
                ["rounded-3xl"] = 24,
                ["rounded-full"] = 9999,
            };

            if (radiusMap.TryGetValue(className, out double radius))
            {
                return new CornerRadius(radius);
            }

            return new CornerRadius(0);
        }

        /// <summary>
        /// Converts a CSS-like class string to WPF Style properties
        /// </summary>
        public static Dictionary<string, object> ParseTailwindClasses(string classes)
        {
            var properties = new Dictionary<string, object>();
            var classList = classes.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var cls in classList)
            {
                // Background colors
                if (cls.StartsWith("bg-") && TailwindColors.ContainsKey(cls[3..]))
                {
                    properties["Background"] = BrushFromTailwind(cls[3..]);
                }
                // Text colors
                else if (cls.StartsWith("text-") && TailwindColors.ContainsKey(cls[5..]))
                {
                    properties["Foreground"] = BrushFromTailwind(cls[5..]);
                }
                // Border colors
                else if (cls.StartsWith("border-") && TailwindColors.ContainsKey(cls[7..]))
                {
                    properties["BorderBrush"] = BrushFromTailwind(cls[7..]);
                }
                // Padding
                else if (cls.StartsWith("p-") || cls.StartsWith("px-") || cls.StartsWith("py-") || 
                        cls.StartsWith("pt-") || cls.StartsWith("pb-") || cls.StartsWith("pl-") || cls.StartsWith("pr-"))
                {
                    properties["Padding"] = ThicknessFromTailwind(cls);
                }
                // Margin
                else if (cls.StartsWith("m-") || cls.StartsWith("mx-") || cls.StartsWith("my-") || 
                        cls.StartsWith("mt-") || cls.StartsWith("mb-") || cls.StartsWith("ml-") || cls.StartsWith("mr-"))
                {
                    properties["Margin"] = ThicknessFromTailwind(cls);
                }
                // Border radius
                else if (cls.StartsWith("rounded"))
                {
                    properties["CornerRadius"] = BorderRadiusFromTailwind(cls);
                }
            }

            return properties;
        }
    }
}