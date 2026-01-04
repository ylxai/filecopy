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
    /// Partial class for CssToXamlHelper - Core functionality
    /// </summary>
    public static partial class CssToXamlHelper
    {
        /// <summary>
        /// Converts a Tailwind CSS file to XAML ResourceDictionary
        /// </summary>
        public static string ConvertCssFileToXaml(string cssFilePath)
        {
            var cssContent = File.ReadAllText(cssFilePath);
            return ConvertCssToXaml(cssContent);
        }

        /// <summary>
        /// Converts CSS content string to XAML ResourceDictionary
        /// </summary>
        public static string ConvertCssToXaml(string cssContent)
        {
            var xamlBuilder = new StringBuilder();

            xamlBuilder.AppendLine("<!-- Generated from Tailwind CSS v4 -->");
            xamlBuilder.AppendLine("<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            xamlBuilder.AppendLine("                    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
            xamlBuilder.AppendLine();

            // Parse CSS content
            var cssRules = ParseCssRules(cssContent);
            var styleCounter = 0;

            foreach (var rule in cssRules)
            {
                var xamlStyle = ConvertCssRuleToXamlStyle(rule, ref styleCounter);
                if (!string.IsNullOrEmpty(xamlStyle))
                {
                    xamlBuilder.AppendLine(xamlStyle);
                    xamlBuilder.AppendLine();
                }
            }

            xamlBuilder.AppendLine("</ResourceDictionary>");
            return xamlBuilder.ToString();
        }

        /// <summary>
        /// Converts individual Tailwind classes to XAML style setters
        /// </summary>
        public static string ConvertTailwindClassesToXaml(string tailwindClasses, string targetType = "FrameworkElement")
        {
            var properties = TailwindToXamlConverter.ParseTailwindClasses(tailwindClasses);
            var xamlBuilder = new StringBuilder();

            xamlBuilder.AppendLine($"<Style TargetType=\"{targetType}\">");

            foreach (var prop in properties)
            {
                var xamlValue = ConvertPropertyValueToXaml(prop.Value);
                xamlBuilder.AppendLine($"    <Setter Property=\"{prop.Key}\" Value=\"{xamlValue}\" />");
            }

            xamlBuilder.AppendLine("</Style>");
            return xamlBuilder.ToString();
        }

        /// <summary>
        /// Save converted XAML to file
        /// </summary>
        public static void SaveXamlToFile(string xamlContent, string outputPath)
        {
            File.WriteAllText(outputPath, xamlContent, Encoding.UTF8);
        }

        /// <summary>
        /// Convert CSS file and save as XAML ResourceDictionary
        /// </summary>
        public static void ConvertCssFile(string inputCssPath, string outputXamlPath)
        {
            var xamlContent = ConvertCssFileToXaml(inputCssPath);
            SaveXamlToFile(xamlContent, outputXamlPath);
        }
    }
}