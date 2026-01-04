using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace FileCopyUtility.Services;

/// <summary>
/// Windows 11 style toast notification service
/// </summary>
public class ToastNotificationService
{
    private Window? _parentWindow;
    private readonly DispatcherTimer _autoHideTimer;

    public ToastNotificationService(Window parentWindow)
    {
        _parentWindow = parentWindow;
        _autoHideTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _autoHideTimer.Tick += AutoHideTimer_Tick;
    }

    /// <summary>
    /// Show success toast notification
    /// </summary>
    public void ShowSuccess(string title, string message, int autoHideSeconds = 5)
    {
        ShowToast(title, message, ToastType.Success, autoHideSeconds);
    }

    /// <summary>
    /// Show error toast notification
    /// </summary>
    public void ShowError(string title, string message, int autoHideSeconds = 8)
    {
        ShowToast(title, message, ToastType.Error, autoHideSeconds);
    }

    /// <summary>
    /// Show info toast notification
    /// </summary>
    public void ShowInfo(string title, string message, int autoHideSeconds = 4)
    {
        ShowToast(title, message, ToastType.Info, autoHideSeconds);
    }

    /// <summary>
    /// Show warning toast notification
    /// </summary>
    public void ShowWarning(string title, string message, int autoHideSeconds = 6)
    {
        ShowToast(title, message, ToastType.Warning, autoHideSeconds);
    }

    private void ShowToast(string title, string message, ToastType type, int autoHideSeconds)
    {
        if (_parentWindow == null) return;

        var toast = CreateToastWindow(title, message, type);
        
        // Position toast in bottom-right corner
        toast.Left = SystemParameters.WorkArea.Width - toast.Width - 20;
        toast.Top = SystemParameters.WorkArea.Height - toast.Height - 20;

        // Show with slide-up animation
        toast.Show();
        AnimateToastIn(toast);

        // Set auto-hide timer
        _autoHideTimer.Interval = TimeSpan.FromSeconds(autoHideSeconds);
        _autoHideTimer.Tag = toast;
        _autoHideTimer.Start();
    }

    private Window CreateToastWindow(string title, string message, ToastType type)
    {
        var toast = new Window
        {
            Width = 350,
            Height = 100,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            Topmost = true,
            ShowInTaskbar = false,
            AllowsTransparency = true,
            Background = System.Windows.Media.Brushes.Transparent
        };

        var mainBorder = new Border
        {
            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(250, 255, 255, 255)),
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 0, 0, 0)),
            Padding = new Thickness(16, 12, 16, 12)
        };

        mainBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = System.Windows.Media.Colors.Black,
            BlurRadius = 20,
            ShadowDepth = 4,
            Opacity = 0.15
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });

        // Icon
        var icon = new TextBlock
        {
            Text = GetToastIcon(type),
            FontSize = 24,
            Foreground = new SolidColorBrush(GetToastColor(type)),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        Grid.SetColumn(icon, 0);

        // Content
        var contentPanel = new StackPanel
        {
            Margin = new Thickness(12, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        var titleBlock = new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x1f, 0x1f, 0x1f)),
            Margin = new Thickness(0, 0, 0, 4)
        };

        var messageBlock = new TextBlock
        {
            Text = message,
            FontSize = 12,
            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6b, 0x6b, 0x6b)),
            TextWrapping = TextWrapping.Wrap
        };

        contentPanel.Children.Add(titleBlock);
        contentPanel.Children.Add(messageBlock);
        Grid.SetColumn(contentPanel, 1);

        // Close button
        var closeButton = new System.Windows.Controls.Button
        {
            Content = "✕",
            Width = 24,
            Height = 24,
            Background = System.Windows.Media.Brushes.Transparent,
            BorderThickness = new Thickness(0),
            FontSize = 12,
            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6b, 0x6b, 0x6b)),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        closeButton.Click += (s, e) => AnimateToastOut(toast);
        Grid.SetColumn(closeButton, 2);

        grid.Children.Add(icon);
        grid.Children.Add(contentPanel);
        grid.Children.Add(closeButton);
        mainBorder.Child = grid;
        toast.Content = mainBorder;

        return toast;
    }

    private void AnimateToastIn(Window toast)
    {
        // Start from below screen
        var startTop = toast.Top + 100;
        toast.Top = startTop;
        toast.Opacity = 0;

        // Slide up animation
        var slideAnimation = new DoubleAnimation
        {
            From = startTop,
            To = startTop - 100,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        // Fade in animation
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300)
        };

        toast.BeginAnimation(Window.TopProperty, slideAnimation);
        toast.BeginAnimation(Window.OpacityProperty, fadeAnimation);
    }

    private void AnimateToastOut(Window toast)
    {
        _autoHideTimer.Stop();

        // Slide down and fade out
        var slideAnimation = new DoubleAnimation
        {
            From = toast.Top,
            To = toast.Top + 100,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300)
        };

        slideAnimation.Completed += (s, e) => toast.Close();

        toast.BeginAnimation(Window.TopProperty, slideAnimation);
        toast.BeginAnimation(Window.OpacityProperty, fadeAnimation);
    }

    private void AutoHideTimer_Tick(object? sender, EventArgs e)
    {
        if (_autoHideTimer.Tag is Window toast)
        {
            AnimateToastOut(toast);
        }
        _autoHideTimer.Stop();
    }

    private static string GetToastIcon(ToastType type)
    {
        return type switch
        {
            ToastType.Success => "✅",
            ToastType.Error => "❌",
            ToastType.Warning => "⚠️",
            ToastType.Info => "ℹ️",
            _ => "ℹ️"
        };
    }

    private static System.Windows.Media.Color GetToastColor(ToastType type)
    {
        return type switch
        {
            ToastType.Success => System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50),
            ToastType.Error => System.Windows.Media.Color.FromRgb(0xF4, 0x43, 0x36),
            ToastType.Warning => System.Windows.Media.Color.FromRgb(0xFF, 0x98, 0x00),
            ToastType.Info => System.Windows.Media.Color.FromRgb(0x21, 0x96, 0xF3),
            _ => System.Windows.Media.Color.FromRgb(0x21, 0x96, 0xF3)
        };
    }
}

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}