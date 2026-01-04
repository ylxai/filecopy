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
    /// Partial class for AnimationHelper - Core functionality
    /// </summary>
    public static partial class AnimationHelper
    {
        // Standard durations matching Tailwind CSS
        public static readonly Duration Fast = TimeSpan.FromMilliseconds(150);
        public static readonly Duration Normal = TimeSpan.FromMilliseconds(300);
        public static readonly Duration Slow = TimeSpan.FromMilliseconds(500);
        public static readonly Duration VerySlow = TimeSpan.FromMilliseconds(700);

        /// <summary>
        /// Ensures element has proper transforms for animation
        /// </summary>
        private static void EnsureTransforms(UIElement element)
        {
            if (element.RenderTransform == null || element.RenderTransform == Transform.Identity)
            {
                element.RenderTransform = new TransformGroup
                {
                    Children =
                    {
                        new ScaleTransform(),
                        new RotateTransform(),
                        new TranslateTransform()
                    }
                };
                element.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            }
        }
    }
}