using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Shapes;

namespace FileCopyUtility.Services;

/// <summary>
/// Advanced progress visualization service with animated indicators
/// </summary>
public class AdvancedProgressService
{
    private readonly DispatcherTimer _animationTimer;
    private readonly Dictionary<string, ProgressIndicator> _indicators;
    
    public AdvancedProgressService()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _animationTimer.Tick += AnimationTimer_Tick;
        _indicators = new Dictionary<string, ProgressIndicator>();
    }

    /// <summary>
    /// Create enhanced progress bar with speed visualization
    /// </summary>
    public ProgressIndicator CreateSpeedProgressBar(Grid container, string name)
    {
        var indicator = new ProgressIndicator
        {
            Name = name,
            Container = container
        };

        // Main progress bar
        var mainProgress = new System.Windows.Controls.ProgressBar
        {
            Height = 8,
            Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xe8, 0xe5, 0xf0)),
            Foreground = new LinearGradientBrush(
                System.Windows.Media.Color.FromRgb(0xb8, 0x54, 0x93), 
                System.Windows.Media.Color.FromRgb(0x22, 0xc5, 0x5e), 0.0),
            BorderThickness = new Thickness(0),
            Margin = new Thickness(0, 5, 0, 5)
        };

        // Speed indicator overlay
        var speedOverlay = new System.Windows.Shapes.Rectangle
        {
            Height = 4,
            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(120, 0x22, 0xc5, 0x5e)),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 2, 0, 0)
        };

        // Pulse animation for active copying
        var pulseAnimation = new DoubleAnimation
        {
            From = 0.6,
            To = 1.0,
            Duration = TimeSpan.FromSeconds(1),
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };

        indicator.MainProgress = mainProgress;
        indicator.SpeedOverlay = speedOverlay;
        indicator.PulseAnimation = pulseAnimation;

        container.Children.Add(mainProgress);
        container.Children.Add(speedOverlay);

        _indicators[name] = indicator;
        return indicator;
    }

    /// <summary>
    /// Create circular progress indicator with percentage
    /// </summary>
    public CircularProgress CreateCircularProgress(System.Windows.Controls.Panel container, string name, double size = 80)
    {
        var progress = new CircularProgress
        {
            Name = name,
            Size = size,
            StrokeThickness = 6,
            BackgroundStroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xe8, 0xe5, 0xf0)),
            ProgressStroke = new LinearGradientBrush(
                System.Windows.Media.Color.FromRgb(0xb8, 0x54, 0x93),
                System.Windows.Media.Color.FromRgb(0x22, 0xc5, 0x5e), 0.0)
        };

        container.Children.Add(progress.Canvas);
        return progress;
    }

    /// <summary>
    /// Update progress with smooth animations
    /// </summary>
    public void UpdateProgress(string indicatorName, double progress, double speed = 0)
    {
        if (!_indicators.TryGetValue(indicatorName, out var indicator))
            return;

        // Animate progress bar
        var progressAnimation = new DoubleAnimation
        {
            To = progress,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        indicator.MainProgress?.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, progressAnimation);

        // Update speed overlay
        if (speed > 0)
        {
            var speedWidth = Math.Min(100, (speed / 200.0) * 100); // Normalize to 200 MB/s max
            var widthAnimation = new DoubleAnimation
            {
                To = speedWidth,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            indicator.SpeedOverlay?.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);

            // Start pulse animation if not already running
            if (!indicator.IsAnimating)
            {
                indicator.MainProgress?.BeginAnimation(UIElement.OpacityProperty, indicator.PulseAnimation);
                indicator.IsAnimating = true;
            }
        }
        else
        {
            // Stop pulse animation
            if (indicator.IsAnimating)
            {
                indicator.MainProgress?.BeginAnimation(UIElement.OpacityProperty, null);
                if (indicator.MainProgress != null)
                    indicator.MainProgress.Opacity = 1.0;
                indicator.IsAnimating = false;
            }
        }
    }

    /// <summary>
    /// Start all animations
    /// </summary>
    public void StartAnimations()
    {
        _animationTimer.Start();
    }

    /// <summary>
    /// Stop all animations
    /// </summary>
    public void StopAnimations()
    {
        _animationTimer.Stop();
        
        foreach (var indicator in _indicators.Values)
        {
            if (indicator.IsAnimating)
            {
                indicator.MainProgress?.BeginAnimation(UIElement.OpacityProperty, null);
                if (indicator.MainProgress != null)
                    indicator.MainProgress.Opacity = 1.0;
                indicator.IsAnimating = false;
            }
        }
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        // Update any continuous animations here
        foreach (var indicator in _indicators.Values)
        {
            if (indicator.IsAnimating)
            {
                // Add sparkle effects or other continuous animations
            }
        }
    }
}

#region Progress Models

public class ProgressIndicator
{
    public string Name { get; set; } = string.Empty;
    public Grid? Container { get; set; }
    public System.Windows.Controls.ProgressBar? MainProgress { get; set; }
    public System.Windows.Shapes.Rectangle? SpeedOverlay { get; set; }
    public DoubleAnimation? PulseAnimation { get; set; }
    public bool IsAnimating { get; set; }
}

public class CircularProgress
{
    public string Name { get; set; } = string.Empty;
    public double Size { get; set; }
    public double StrokeThickness { get; set; }
    public System.Windows.Media.Brush? BackgroundStroke { get; set; }
    public System.Windows.Media.Brush? ProgressStroke { get; set; }
    public Canvas Canvas { get; set; }
    public double Progress { get; private set; }

    public CircularProgress()
    {
        Canvas = new Canvas();
        CreateCircle();
    }

    private void CreateCircle()
    {
        var background = new Ellipse
        {
            Width = Size,
            Height = Size,
            Stroke = BackgroundStroke,
            StrokeThickness = StrokeThickness,
            Fill = System.Windows.Media.Brushes.Transparent
        };

        Canvas.Children.Add(background);
    }

    public void UpdateProgress(double progress)
    {
        Progress = Math.Max(0, Math.Min(100, progress));
        // Implementation for circular progress update
    }
}

#endregion