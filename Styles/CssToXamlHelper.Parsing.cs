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
    /// Partial class for CssToXamlHelper - CSS parsing functionality
    /// </summary>
    public static partial class CssToXamlHelper
    {
        /// <summary>
        /// Parses CSS content and extracts rules
        /// </summary>
        private static List<CssRule> ParseCssRules(string cssContent)
        {
            var rules = new List<CssRule>();

            // Remove comments
            cssContent = Regex.Replace(cssContent, @"/\*.*?\*/", "", RegexOptions.Singleline);

            // Match CSS rules using regex
            var rulePattern = @"([^{]+)\s*{\s*([^}]+)\s*}";
            var matches = Regex.Matches(cssContent, rulePattern, RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                var selector = match.Groups[1].Value.Trim();
                var declarations = match.Groups[2].Value.Trim();

                // Skip @media, @import, etc.
                if (selector.StartsWith("@")) continue;

                var rule = new CssRule
                {
                    Selector = selector,
                    Declarations = ParseCssDeclarations(declarations)
                };

                rules.Add(rule);
            }

            return rules;
        }

        /// <summary>
        /// Parses CSS declarations (property: value pairs)
        /// </summary>
        private static Dictionary<string, string> ParseCssDeclarations(string declarations)
        {
            var result = new Dictionary<string, string>();
            var declarationPairs = declarations.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in declarationPairs)
            {
                var colonIndex = pair.IndexOf(':');
                if (colonIndex > 0)
                {
                    var property = pair.Substring(0, colonIndex).Trim();
                    var value = pair.Substring(colonIndex + 1).Trim();
                    result[property] = value;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a CSS rule to XAML Style
        /// </summary>
        private static string ConvertCssRuleToXamlStyle(CssRule rule, ref int styleCounter)
        {
            var xamlBuilder = new StringBuilder();
            var styleName = GenerateStyleName(rule.Selector, ref styleCounter);
            var targetType = DetermineTargetType(rule.Selector);

            xamlBuilder.AppendLine($"<!-- {rule.Selector} -->");
            xamlBuilder.AppendLine($"<Style x:Key=\"{styleName}\" TargetType=\"{targetType}\">");

            foreach (var declaration in rule.Declarations)
            {
                var xamlProperty = ConvertCssPropertyToXaml(declaration.Key);
                var xamlValue = ConvertCssValueToXaml(declaration.Key, declaration.Value);

                if (!string.IsNullOrEmpty(xamlProperty) && !string.IsNullOrEmpty(xamlValue))
                {
                    xamlBuilder.AppendLine($"    <Setter Property=\"{xamlProperty}\" Value=\"{xamlValue}\" />");
                }
            }

            xamlBuilder.AppendLine("</Style>");
            return xamlBuilder.ToString();
        }
    }
}