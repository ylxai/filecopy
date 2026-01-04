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
    /// Partial class for AnimationHelper - Scale animations
    /// </summary>
    public static partial class AnimationHelper
    {
        /// <summary>
        /// Scale element in with bounce effect
        /// </summary>
        public static async Task ScaleInAsync(UIElement element, Duration? duration = null)
        {
            duration ??= Normal;

            await element.Dispatcher.InvokeAsync(() =>
            {
                EnsureTransforms(element);

                var transformGroup = (TransformGroup)element.RenderTransform;
                var scaleTransform = transformGroup.Children[0] as ScaleTransform;

                if (scaleTransform == null) return;

                var storyboard = new Storyboard();

                var scaleXAnimation = new DoubleAnimation
                {
                    From = 0.95,
                    To = 1.0,
                    Duration = duration.Value,
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
                };

                var scaleYAnimation = new DoubleAnimation
                {
                    From = 0.95,
                    To = 1.0,
                    Duration = duration.Value,
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
                };

                var fadeAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = duration.Value,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(scaleXAnimation, element);
                Storyboard.SetTarget(scaleYAnimation, element);
                Storyboard.SetTarget(fadeAnimation, element);

                Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
                Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
                Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));

                storyboard.Children.Add(scaleXAnimation);
                storyboard.Children.Add(scaleYAnimation);
                storyboard.Children.Add(fadeAnimation);

                scaleTransform.ScaleX = 0.95;
                scaleTransform.ScaleY = 0.95;
                element.Opacity = 0;

                storyboard.Begin();
            });

            await Task.Delay(duration.Value.TimeSpan);
        }
    }
}