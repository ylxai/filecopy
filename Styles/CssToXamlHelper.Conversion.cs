using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace FileCopyUtility.Styles
{
    /// <summary>
    /// Partial class for CssToXamlHelper - Property conversion functionality
    /// </summary>
    public static partial class CssToXamlHelper
    {
        /// <summary>
        /// Maps CSS properties to WPF properties
        /// </summary>
        private static string? ConvertCssPropertyToXaml(string cssProperty)
        {
            return cssProperty.ToLower() switch
            {
                "color" => "Foreground",
                "background-color" => "Background",
                "background" => "Background",
                "border-color" => "BorderBrush",
                "border" => "BorderThickness",
                "border-width" => "BorderThickness",
                "border-radius" => "CornerRadius",
                "margin" => "Margin",
                "margin-top" => "Margin",
                "margin-bottom" => "Margin",
                "margin-left" => "Margin",
                "margin-right" => "Margin",
                "padding" => "Padding",
                "padding-top" => "Padding",
                "padding-bottom" => "Padding",
                "padding-left" => "Padding",
                "padding-right" => "Padding",
                "width" => "Width",
                "height" => "Height",
                "min-width" => "MinWidth",
                "min-height" => "MinHeight",
                "max-width" => "MaxWidth",
                "max-height" => "MaxHeight",
                "font-size" => "FontSize",
                "font-weight" => "FontWeight",
                "font-family" => "FontFamily",
                "text-align" => "HorizontalAlignment",
                "opacity" => "Opacity",
                "visibility" => "Visibility",
                "cursor" => "Cursor",
                _ => null
            };
        }

        /// <summary>
        /// Converts CSS values to WPF values
        /// </summary>
        private static string ConvertCssValueToXaml(string cssProperty, string cssValue)
        {
            // Handle color values
            if (cssProperty.Contains("color") || cssProperty == "background")
            {
                return ConvertColorValue(cssValue);
            }

            // Handle spacing values (margin, padding)
            if (cssProperty.Contains("margin") || cssProperty.Contains("padding"))
            {
                return ConvertSpacingValue(cssValue);
            }

            // Handle border-radius
            if (cssProperty == "border-radius")
            {
                return ConvertBorderRadiusValue(cssValue);
            }

            // Handle font-weight
            if (cssProperty == "font-weight")
            {
                return ConvertFontWeightValue(cssValue);
            }

            // Handle text-align
            if (cssProperty == "text-align")
            {
                return ConvertTextAlignValue(cssValue);
            }

            // Handle dimensions (px, rem, etc.)
            if (cssProperty.Contains("width") || cssProperty.Contains("height") || cssProperty == "font-size")
            {
                return ConvertDimensionValue(cssValue);
            }

            // Return as-is for other values
            return cssValue;
        }

        private static string ConvertColorValue(string cssColor)
        {
            // Handle Tailwind color references
            if (TailwindToXamlConverter.TailwindColors.ContainsKey(cssColor))
            {
                return $"{{StaticResource {cssColor.Replace("-", "")}Brush}}";
            }

            // Handle hex colors
            if (cssColor.StartsWith("#"))
            {
                return cssColor;
            }

            // Handle rgb/rgba
            if (cssColor.StartsWith("rgb"))
            {
                // Parse rgb(255, 255, 255) or rgba(255, 255, 255, 0.5)
                var match = Regex.Match(cssColor, @"rgba?\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+))?\s*\)");
                if (match.Success)
                {
                    var r = byte.Parse(match.Groups[1].Value);
                    var g = byte.Parse(match.Groups[2].Value);
                    var b = byte.Parse(match.Groups[3].Value);
                    var a = match.Groups[4].Success ? (byte)(double.Parse(match.Groups[4].Value) * 255) : (byte)255;

                    return $"#{a:X2}{r:X2}{g:X2}{b:X2}";
                }
            }

            return cssColor;
        }

        private static string ConvertSpacingValue(string cssValue)
        {
            // Handle Tailwind spacing scale
            if (TailwindToXamlConverter.TailwindSpacing.ContainsKey(cssValue))
            {
                var spacing = TailwindToXamlConverter.TailwindSpacing[cssValue];
                return spacing.ToString();
            }

            // Handle px values
            if (cssValue.EndsWith("px"))
            {
                var value = cssValue.Replace("px", "");
                if (double.TryParse(value, out double result))
                {
                    return result.ToString();
                }
            }

            // Handle rem values (convert to px, 1rem = 16px)
            if (cssValue.EndsWith("rem"))
            {
                var value = cssValue.Replace("rem", "");
                if (double.TryParse(value, out double rem))
                {
                    return (rem * 16).ToString();
                }
            }

            return cssValue;
        }

        private static string ConvertBorderRadiusValue(string cssValue)
        {
            var spacing = ConvertSpacingValue(cssValue);
            return spacing;
        }

        private static string ConvertFontWeightValue(string cssValue)
        {
            return cssValue switch
            {
                "100" => "Thin",
                "200" => "ExtraLight",
                "300" => "Light",
                "400" or "normal" => "Normal",
                "500" => "Medium",
                "600" => "SemiBold",
                "700" or "bold" => "Bold",
                "800" => "ExtraBold",
                "900" => "Black",
                _ => cssValue
            };
        }

        private static string ConvertTextAlignValue(string cssValue)
        {
            return cssValue switch
            {
                "left" => "Left",
                "center" => "Center",
                "right" => "Right",
                "justify" => "Stretch",
                _ => cssValue
            };
        }

        private static string ConvertDimensionValue(string cssValue)
        {
            return ConvertSpacingValue(cssValue);
        }

        private static string ConvertPropertyValueToXaml(object value)
        {
            return value switch
            {
                null => string.Empty,
                SolidColorBrush brush => $"{{StaticResource {FindBrushResourceKey(brush)}}}",
                Thickness thickness => thickness.ToString() ?? string.Empty,
                CornerRadius corner => corner.ToString() ?? string.Empty,
                _ => value.ToString() ?? string.Empty
            };
        }

        private static string FindBrushResourceKey(SolidColorBrush brush)
        {
            // This would need to be implemented to find the resource key for a brush
            // For now, return a default value to avoid null reference
            return "UnknownBrush";
        }

        private static string GenerateStyleName(string selector, ref int counter)
        {
            // Clean up selector and generate a valid XAML key
            var cleanSelector = Regex.Replace(selector, @"[^\w-]", "");
            cleanSelector = string.Join("", cleanSelector.Split('-').Select(s =>
                char.ToUpper(s[0]) + s.Substring(1).ToLower()));

            if (string.IsNullOrEmpty(cleanSelector))
            {
                cleanSelector = $"GeneratedStyle{++counter}";
            }

            return $"Tailwind{cleanSelector}Style";
        }

        private static string DetermineTargetType(string selector)
        {
            // Try to determine appropriate WPF control type from CSS selector
            if (selector.Contains("button") || selector.Contains("btn"))
                return "Button";
            if (selector.Contains("input") || selector.Contains("textbox"))
                return "TextBox";
            if (selector.Contains("label"))
                return "Label";
            if (selector.Contains("card") || selector.Contains("container"))
                return "Border";

            return "FrameworkElement";
        }

        private class CssRule
        {
            public string Selector { get; set; } = "";
            public Dictionary<string, string> Declarations { get; set; } = new();
        }
    }
}