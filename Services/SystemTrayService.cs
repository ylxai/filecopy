using System;
using System.Windows;

namespace FileCopyUtility.Services;

/// <summary>
/// Service untuk system tray / notification area support (WPF version)
/// Uses Hardcodet.NotifyIcon.Wpf for system tray implementation
/// </summary>
public class SystemTrayService : IDisposable
{
    private Window? _mainWindow;


    public void Initialize(Window mainWindow)
    {
        _mainWindow = mainWindow;
        // Can be implemented using Hardcodet.NotifyIcon.Wpf if needed
    }

    public void ShowInTray()
    {
        // Placeholder - would show icon in system tray
        // Implementation using Hardcodet.NotifyIcon.Wpf package
    }

    public void HideFromTray()
    {
        // Placeholder - would hide icon from system tray
    }

    public void ShowBalloonTip(string title, string message)
    {
        // Placeholder - would show notification balloon
        // Can use Windows notification system or MessageBox as fallback
        System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
