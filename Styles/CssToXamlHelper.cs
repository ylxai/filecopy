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
    /// Helper utility to convert Tailwind CSS v4 classes to WPF XAML styles
    /// Usage: CssToXamlHelper.ConvertCssFile("index.css", "TailwindStyles.xaml")
    /// </summary>
    public static partial class CssToXamlHelper
    {
        // This partial class contains only the declaration
        // All functionality has been moved to partial classes:
        // - CssToXamlHelper.Core.cs (core functionality)
        // - CssToXamlHelper.Parsing.cs (CSS parsing functionality)
        // - CssToXamlHelper.Conversion.cs (property conversion functionality)
    }
}