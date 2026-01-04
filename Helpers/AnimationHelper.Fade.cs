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
    /// Partial class for AnimationHelper - Fade animations
    /// </summary>
    public static partial class AnimationHelper
    {
        /// <summary>
        /// Fade in element with smooth transition
        /// </summary>
        public static async Task FadeInAsync(UIElement element, Duration? duration = null)
        {
            duration ??= Normal;

            await element.Dispatcher.InvokeAsync(() =>
            {
                var storyboard = new Storyboard();
                var fadeAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = duration.Value,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(fadeAnimation, element);
                Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
                storyboard.Children.Add(fadeAnimation);

                element.Opacity = 0;
                storyboard.Begin();
            });

            await Task.Delay(duration.Value.TimeSpan);
        }

        /// <summary>
        /// Fade out element with smooth transition
        /// </summary>
        public static async Task FadeOutAsync(UIElement element, Duration? duration = null)
        {
            duration ??= Fast;

            var storyboard = new Storyboard();
            var fadeAnimation = new DoubleAnimation
            {
                From = element.Opacity,
                To = 0,
                Duration = duration.Value,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            Storyboard.SetTarget(fadeAnimation, element);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeAnimation);

            storyboard.Begin();
            await Task.Delay(duration.Value.TimeSpan);
        }
    }
}