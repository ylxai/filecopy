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
    /// Partial class for AnimationHelper - Slide animations
    /// </summary>
    public static partial class AnimationHelper
    {
        /// <summary>
        /// Slide element from specified direction
        /// </summary>
        public static async Task SlideInAsync(UIElement element, SlideDirection direction, double distance = 30, Duration? duration = null)
        {
            duration ??= Normal;

            await element.Dispatcher.InvokeAsync(() =>
            {
                EnsureTransforms(element);

                var transformGroup = (TransformGroup)element.RenderTransform;
                var translateTransform = transformGroup.Children[2] as TranslateTransform;

                if (translateTransform == null) return;

                var storyboard = new Storyboard();

                // Set initial position
                switch (direction)
                {
                    case SlideDirection.Up:
                        translateTransform.Y = distance;
                        break;
                    case SlideDirection.Down:
                        translateTransform.Y = -distance;
                        break;
                    case SlideDirection.Left:
                        translateTransform.X = distance;
                        break;
                    case SlideDirection.Right:
                        translateTransform.X = -distance;
                        break;
                }

                // Create animations
                var slideAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = duration.Value,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                var fadeAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = duration.Value,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(slideAnimation, element);
                Storyboard.SetTarget(fadeAnimation, element);

                switch (direction)
                {
                    case SlideDirection.Up:
                    case SlideDirection.Down:
                        Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(TranslateTransform.Y)"));
                        break;
                    case SlideDirection.Left:
                    case SlideDirection.Right:
                        Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(TranslateTransform.X)"));
                        break;
                }

                Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));

                storyboard.Children.Add(slideAnimation);
                storyboard.Children.Add(fadeAnimation);

                element.Opacity = 0;
                storyboard.Begin();
            });

            await Task.Delay(duration.Value.TimeSpan);
        }

        /// <summary>
        /// Slide element out in specified direction
        /// </summary>
        private static async Task SlideOutAsync(UIElement element, SlideDirection direction, double distance = 100, Duration? duration = null)
        {
            duration ??= Fast;
            EnsureTransforms(element);

            var storyboard = new Storyboard();
            var slideAnimation = new DoubleAnimation
            {
                Duration = duration.Value,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var fadeAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = duration.Value,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            Storyboard.SetTarget(slideAnimation, element);
            Storyboard.SetTarget(fadeAnimation, element);

            switch (direction)
            {
                case SlideDirection.Up:
                    slideAnimation.To = -distance;
                    Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(TranslateTransform.Y)"));
                    break;
                case SlideDirection.Down:
                    slideAnimation.To = distance;
                    Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(TranslateTransform.Y)"));
                    break;
                case SlideDirection.Left:
                    slideAnimation.To = -distance;
                    Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(TranslateTransform.X)"));
                    break;
                case SlideDirection.Right:
                    slideAnimation.To = distance;
                    Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(TranslateTransform.X)"));
                    break;
            }

            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));

            storyboard.Children.Add(slideAnimation);
            storyboard.Children.Add(fadeAnimation);

            storyboard.Begin();
            await Task.Delay(duration.Value.TimeSpan);
        }
    }
}