using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Or1nRenameFileNameToDateCreated.Helpers
{
    /// <summary>
    /// Applies theme changes consistently across all active windows and dialogs.
    /// </summary>
    public static class ThemeManager
    {
        /// <summary>
        /// Applies Light/Dark theme to all tracked windows and updates title bars.
        /// </summary>
        /// <param name="theme">"Light" or "Dark"</param>
        public static void ApplyThemeToAllWindows(string theme)
        {
            var elementTheme = ParseTheme(theme);

            foreach (var window in WindowHelper.ActiveWindows)
            {
                if (window.Content is FrameworkElement root)
                {
                    var dispatcher = root.DispatcherQueue;
                    if (dispatcher != null)
                    {
                        dispatcher.TryEnqueue(() =>
                        {
                            ForceApplyTheme(root, elementTheme);
                        });
                    }
                    else
                    {
                        ForceApplyTheme(root, elementTheme);
                    }
                }

                if (window is MainWindow mainWindow)
                {
                    mainWindow.UpdateTitleBarTheme(elementTheme);
                }
                else if (window is Views.SettingsWindow settingsWindow)
                {
                    settingsWindow.UpdateTitleBarTheme(elementTheme);
                }
                else if (window is Views.FolderBrowserDialog folderDialog)
                {
                    folderDialog.UpdateTitleBarTheme(elementTheme);
                }
            }
        }

        private static void ForceApplyTheme(FrameworkElement root, ElementTheme theme)
        {
            root.RequestedTheme = ElementTheme.Default;
            root.RequestedTheme = theme;

            if (root is Frame frame && frame.Content is FrameworkElement pageRoot)
            {
                pageRoot.RequestedTheme = ElementTheme.Default;
                pageRoot.RequestedTheme = theme;

                if (pageRoot is Views.MainPage mainPage)
                {
                    mainPage.RefreshLogColors();
                }
            }

            root.InvalidateMeasure();
            root.UpdateLayout();
        }

        /// <summary>
        /// Converts stored theme string to ElementTheme.
        /// </summary>
        public static ElementTheme ParseTheme(string theme)
        {
            return theme == "Dark" ? ElementTheme.Dark : ElementTheme.Light;
        }

        /// <summary>
        /// Resolves a theme-aware color resource key to a <see cref="Color"/>.
        /// </summary>
        public static Color GetThemeColor(string key, Color fallback)
        {
            if (Application.Current.Resources.TryGetValue(key, out var value))
            {
                if (value is Color color)
                {
                    return color;
                }

                if (value is SolidColorBrush brush)
                {
                    return brush.Color;
                }
            }

            return fallback;
        }
    }
}
