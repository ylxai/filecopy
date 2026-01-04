using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FileCopyUtility.Helpers
{
    /// <summary>
    /// Helper class for applying Tailwind-inspired animations to WPF elements
    /// Provides easy-to-use methods for common animation patterns
    /// </summary>
    public static partial class AnimationHelper
    {
        // This partial class contains only the declaration
        // All functionality has been moved to partial classes:
        // - AnimationHelper.Core.cs (core functionality and constants)
        // - AnimationHelper.Fade.cs (fade animations)
        // - AnimationHelper.Slide.cs (slide animations)
        // - AnimationHelper.Scale.cs (scale animations)
        // - AnimationHelper.Hover.cs (hover effects)
        // - AnimationHelper.Progress.cs (progress and staggered animations)
        // - AnimationHelper.Notification.cs (notification animations)
    }

    public enum SlideDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum AnimationType
    {
        FadeIn,
        SlideUp,
        ScaleIn
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}