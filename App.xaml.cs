using System;
using System.Windows;

namespace FileCopyUtility;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Set global exception handlers
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            System.Windows.MessageBox.Show($"Unhandled exception: {args.ExceptionObject}",
                "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        };
    }
}
