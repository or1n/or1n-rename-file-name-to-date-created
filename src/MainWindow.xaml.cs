using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Or1nRenameFileNameToDateCreated.Views;
using Windows.UI;

namespace Or1nRenameFileNameToDateCreated
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
#pragma warning disable CS0103
            InitializeComponent();
            
            // Register this window
            WindowHelper.ActiveWindows.Add(this);
            this.Closed += (s, e) => WindowHelper.ActiveWindows.Remove(this);
            
            SetupTitleBar();
            RootFrame.Navigate(typeof(MainPage));
            
            // Listen for theme changes
            RootFrame.ActualThemeChanged += RootFrame_ActualThemeChanged;
#pragma warning restore CS0103
        }

        private void SetupTitleBar()
        {
            // Set title
            Title = "or1n Rename File Name To Date Created";
            
            // Get the AppWindow
            var appWindow = AppWindow;
            if (appWindow != null)
            {
                var titleBar = appWindow.TitleBar;
                if (titleBar != null)
                {
                    // Apply initial theme to title bar
#pragma warning disable CS0103
                    UpdateTitleBarTheme(RootFrame.ActualTheme);
#pragma warning restore CS0103
                }
            }
        }

        private void RootFrame_ActualThemeChanged(FrameworkElement sender, object args)
        {
            UpdateTitleBarTheme(sender.ActualTheme);
        }

        public void UpdateTitleBarTheme(ElementTheme theme)
        {
            var appWindow = AppWindow;
            if (appWindow?.TitleBar == null) return;

            var titleBar = appWindow.TitleBar;

            // Determine if we're in dark mode
            bool isDark = theme == ElementTheme.Dark ||
                         (theme == ElementTheme.Default && 
                          Application.Current.RequestedTheme == ApplicationTheme.Dark);

            if (isDark)
            {
                // Dark theme colors
                titleBar.BackgroundColor = Color.FromArgb(255, 26, 26, 26);        // #1A1A1A
                titleBar.ForegroundColor = Color.FromArgb(255, 255, 255, 255);     // White text
                titleBar.InactiveBackgroundColor = Color.FromArgb(255, 26, 26, 26);
                titleBar.InactiveForegroundColor = Color.FromArgb(255, 160, 160, 160);
                
                // Button colors - set explicit dark background instead of transparent
                titleBar.ButtonBackgroundColor = Color.FromArgb(255, 26, 26, 26);  // Match title bar
                titleBar.ButtonForegroundColor = Color.FromArgb(255, 255, 255, 255);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 50, 50, 50);
                titleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 255, 255, 255);
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 70, 70, 70);
                titleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 200, 200, 200);
                
                titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 26, 26, 26);
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 160, 160, 160);
            }
            else
            {
                // Light theme colors
                titleBar.BackgroundColor = Color.FromArgb(255, 255, 255, 255);     // White
                titleBar.ForegroundColor = Color.FromArgb(255, 26, 26, 26);        // Dark text
                titleBar.InactiveBackgroundColor = Color.FromArgb(255, 255, 255, 255);
                titleBar.InactiveForegroundColor = Color.FromArgb(255, 150, 150, 150);
                
                // Button colors - set explicit white background instead of transparent
                titleBar.ButtonBackgroundColor = Color.FromArgb(255, 255, 255, 255);  // Match title bar
                titleBar.ButtonForegroundColor = Color.FromArgb(255, 26, 26, 26);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 240, 240, 240);
                titleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 26, 26, 26);
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 220, 220, 220);
                titleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 50, 50, 50);
                
                titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 255, 255, 255);
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 150, 150, 150);
            }
        }
    }
}
