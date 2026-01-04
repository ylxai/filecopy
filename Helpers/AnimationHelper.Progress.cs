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
    /// Partial class for AnimationHelper - Progress and staggered animations
    /// </summary>
    public static partial class AnimationHelper
    {
        /// <summary>
        /// Animate progress bar with smooth transition
        /// </summary>
        public static async Task AnimateProgressAsync(System.Windows.Controls.ProgressBar progressBar, double targetValue, Duration? duration = null)
        {
            duration ??= Normal;

            var storyboard = new Storyboard();
            var progressAnimation = new DoubleAnimation
            {
                From = progressBar.Value,
                To = targetValue,
                Duration = duration.Value,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(progressAnimation, progressBar);
            Storyboard.SetTargetProperty(progressAnimation, new PropertyPath("Value"));
            storyboard.Children.Add(progressAnimation);

            storyboard.Begin();
            await Task.Delay(duration.Value.TimeSpan);
        }

        /// <summary>
        /// Create staggered animations for a collection of elements
        /// </summary>
        public static async Task StaggeredAnimationAsync(IEnumerable<UIElement> elements, AnimationType animationType, int delayMs = 100)
        {
            var tasks = new List<Task>();
            var delay = 0;

            foreach (var element in elements)
            {
                var currentDelay = delay;
                var task = Task.Run(async () =>
                {
                    await Task.Delay(currentDelay);

                    switch (animationType)
                    {
                        case AnimationType.FadeIn:
                            await FadeInAsync(element);
                            break;
                        case AnimationType.SlideUp:
                            await SlideInAsync(element, SlideDirection.Up);
                            break;
                        case AnimationType.ScaleIn:
                            await ScaleInAsync(element);
                            break;
                    }
                });

                tasks.Add(task);
                delay += delayMs;
            }

            await Task.WhenAll(tasks);
        }
    }
}