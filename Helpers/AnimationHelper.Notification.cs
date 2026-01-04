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
    /// Partial class for AnimationHelper - Notification animations
    /// </summary>
    public static partial class AnimationHelper
    {
        /// <summary>
        /// Show notification toast with animation
        /// </summary>
        public static async Task ShowNotificationAsync(System.Windows.Controls.Panel parent, string message, NotificationType type = NotificationType.Info, int autoHideMs = 3000)
        {
            var border = new Border();
            var textBlock = new TextBlock
            {
                Text = message,
                Foreground = System.Windows.Media.Brushes.Black,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };

            border.Child = textBlock;

            // Apply style based on type
            switch (type)
            {
                case NotificationType.Success:
                    border.Style = (Style)System.Windows.Application.Current.FindResource("GlassSuccessToast");
                    break;
                case NotificationType.Error:
                    border.Style = (Style)System.Windows.Application.Current.FindResource("GlassErrorToast");
                    break;
                case NotificationType.Warning:
                    border.Style = (Style)System.Windows.Application.Current.FindResource("GlassWarningToast");
                    break;
                default:
                    border.Style = (Style)System.Windows.Application.Current.FindResource("GlassNotificationToast");
                    break;
            }

            parent.Children.Add(border);

            // Auto-hide after specified time
            if (autoHideMs > 0)
            {
                await Task.Delay(autoHideMs);

                // Slide out animation
                await SlideOutAsync(border, SlideDirection.Up);
                parent.Children.Remove(border);
            }
        }
    }
}