using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Win32;
using FileCopyUtility.Services;
using FileCopyUtility.Models;
using FileCopyUtility.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Windows.Media.Animation;

namespace FileCopyUtility;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // This class now only contains the declaration
    // All functionality has been moved to partial classes:
    // - MainWindow.Initialization.cs (constructor and initialization)
    // - MainWindow.Events.cs (event handlers)
    // - MainWindow.FileOperations.cs (file operations)
    // - MainWindow.UIHandlers.cs (UI-related methods)
    // - MainWindow.Helpers.cs (helper functions)
}