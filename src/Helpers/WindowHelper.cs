using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;

namespace Or1nRenameFileNameToDateCreated
{
    /// <summary>
    /// Provides helper methods for locating and tracking active windows.
    /// </summary>
    public static class WindowHelper
    {
        /// <summary>
        /// Gets the window that owns the provided UI element.
        /// </summary>
        /// <param name="element">The element to resolve to its owning window.</param>
        /// <returns>The owning window when found; otherwise, <c>null</c>.</returns>
        public static Window? GetWindowForElement(UIElement element)
        {
            if (element.XamlRoot != null)
            {
                foreach (Window window in ActiveWindows)
                {
                    if (element.XamlRoot == window.Content.XamlRoot)
                    {
                        return window;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the list of windows currently tracked by the app.
        /// </summary>
        public static List<Window> ActiveWindows { get; } = new List<Window>();
    }
}
