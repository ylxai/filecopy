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
    /// Partial class for AnimationHelper - Hover effects
    /// </summary>
    public static partial class AnimationHelper
    {
        /// <summary>
        /// Apply hover lift effect to element
        /// </summary>
        public static void ApplyHoverLift(UIElement element, double liftDistance = 2)
        {
            EnsureTransforms(element);

            element.MouseEnter += (s, e) =>
            {
                var storyboard = new Storyboard();
                var liftAnimation = new DoubleAnimation
                {
                    To = -liftDistance,
                    Duration = Fast,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(liftAnimation, element);
                Storyboard.SetTargetProperty(liftAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(TranslateTransform.Y)"));
                storyboard.Children.Add(liftAnimation);
                storyboard.Begin();
            };

            element.MouseLeave += (s, e) =>
            {
                var storyboard = new Storyboard();
                var dropAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = Fast,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(dropAnimation, element);
                Storyboard.SetTargetProperty(dropAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(TranslateTransform.Y)"));
                storyboard.Children.Add(dropAnimation);
                storyboard.Begin();
            };
        }

        /// <summary>
        /// Apply hover scale effect to element
        /// </summary>
        public static void ApplyHoverScale(UIElement element, double scaleAmount = 1.02)
        {
            EnsureTransforms(element);

            element.MouseEnter += (s, e) =>
            {
                var storyboard = new Storyboard();

                var scaleXAnimation = new DoubleAnimation
                {
                    To = scaleAmount,
                    Duration = Fast,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                var scaleYAnimation = new DoubleAnimation
                {
                    To = scaleAmount,
                    Duration = Fast,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(scaleXAnimation, element);
                Storyboard.SetTarget(scaleYAnimation, element);

                Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
                Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

                storyboard.Children.Add(scaleXAnimation);
                storyboard.Children.Add(scaleYAnimation);
                storyboard.Begin();
            };

            element.MouseLeave += (s, e) =>
            {
                var storyboard = new Storyboard();

                var scaleXAnimation = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = Fast,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                var scaleYAnimation = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = Fast,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(scaleXAnimation, element);
                Storyboard.SetTarget(scaleYAnimation, element);

                Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
                Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

                storyboard.Children.Add(scaleXAnimation);
                storyboard.Children.Add(scaleYAnimation);
                storyboard.Begin();
            };
        }
    }
}