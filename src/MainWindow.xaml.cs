using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Or1nRenameFileNameToDateCreated.Helpers;
using Or1nRenameFileNameToDateCreated.Views;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.Graphics;

namespace Or1nRenameFileNameToDateCreated
{
    /// <summary>
    /// Main application window with Mica Alt backdrop and custom title bar.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private int _minWidth = 710;   // Based on user testing: minimum to show title + description (2 lines) + buttons (1 row) + log area without cutoff
        private int _minHeight = 640;  // Based on user testing: minimum to show all UI elements including full log area with 7+ lines
        private bool _isInitializing = true;
        
        // Backdrop diagnostics for UI display
        public static string BackdropStatus { get; private set; } = "Not initialized";

        public MainWindow()
        {
#pragma warning disable CS0103
            InitializeComponent();
            
            // Clear and start debug logging
            WindowSettings.ClearDebugLog();
            
            // Apply Mica Alt backdrop for enhanced visual hierarchy with safe fallback
            ApplySystemBackdrop();
            
            // Register this window
            WindowHelper.ActiveWindows.Add(this);
            this.Closed += (s, e) => WindowHelper.ActiveWindows.Remove(this);
            
            SetupTitleBar();
            RootFrame.Navigate(typeof(MainPage));

            ApplyMinimumWindowSize();
            
            // Restore or center window position
            RestoreOrCenterWindow();
            
            // Ensure window starts at minimum size before it becomes visible
            this.Activated += (s, e) => 
            {
                var appWindow = AppWindow;
                if (appWindow != null)
                {
                    EnforceMinimumWindowSize(appWindow);
                }
            };

            // Save window position when it's moved or resized
            var appWindow = AppWindow;
            if (appWindow != null)
            {
                appWindow.Changed += AppWindow_Changed_SavePosition;
            }

            // Save position before closing - use synchronous wrapper to ensure save completes
            this.Closed += (s, e) => SaveWindowPositionSync();
            
            // Listen for theme changes
            RootFrame.ActualThemeChanged += RootFrame_ActualThemeChanged;
            
            _isInitializing = false;
#pragma warning restore CS0103
        }

        /// <summary>
        /// Applies the preferred system backdrop with a safe fallback when unsupported.
        /// Desktop Acrylic provides a more visible frosted glass effect compared to subtle Mica.
        /// </summary>
        private void ApplySystemBackdrop()
        {
            try
            {
                SystemBackdrop = new DesktopAcrylicBackdrop();
                BackdropStatus = "DesktopAcrylic (frosted glass) - Applied";
                return;
            }
            catch
            {
                BackdropStatus = "DesktopAcrylic failed - Trying Mica...";
            }

            // Fallback to Mica Alt if Acrylic not supported
            if (MicaController.IsSupported())
            {
                SystemBackdrop = new MicaBackdrop
                {
                    Kind = MicaKind.BaseAlt
                };
                BackdropStatus += " | Mica Alt - IsSupported=TRUE, Applied=YES";
                return;
            }
            else
            {
                BackdropStatus += " | Mica - IsSupported=FALSE";
            }

            SystemBackdrop = null;
            BackdropStatus += " | Final: NO BACKDROP (both unsupported)";
        }

        private void ApplyMinimumWindowSize()
        {
            var appWindow = AppWindow;
            if (appWindow == null) return;

            var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            var workArea = displayArea.WorkArea;

            // Minimum sizing (tested and verified by user):
            // Width: 710px - ensures title fully visible, description wraps at 2 lines, buttons in one row
            // Height: 640px - ensures log area shows 7+ lines without cutoff
            const int preferredMinWidth = 710;
            const int preferredMinHeight = 640;

            _minWidth = Math.Min(preferredMinWidth, workArea.Width);
            _minHeight = Math.Min(preferredMinHeight, workArea.Height);

            appWindow.Changed += AppWindow_Changed;
            EnforceMinimumWindowSize(appWindow);
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (!args.DidSizeChange) return;

            EnforceMinimumWindowSize(sender);
        }

        private void AppWindow_Changed_SavePosition(AppWindow sender, AppWindowChangedEventArgs args)
        {
            // Skip saving during initialization
            if (_isInitializing) return;

            if (args.DidPositionChange || args.DidSizeChange)
            {
                SaveWindowPosition();
            }
        }

        /// <summary>
        /// Restores the window to its last saved position, or centers it if no saved position exists.
        /// </summary>
        private void RestoreOrCenterWindow()
        {
            var appWindow = AppWindow;
            if (appWindow == null) return;

            var task = Task.Run(async () =>
            {
                // Small delay to ensure window is ready
                await Task.Delay(100);
                
                var savedState = await WindowSettings.LoadAsync();
                
                if (savedState != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] Loaded settings: X={savedState.X}, Y={savedState.Y}, W={savedState.Width}, H={savedState.Height}, Theme={savedState.Theme}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] No saved settings found, will center window at default size");
                }
                
                // Always restore size if available - it's a separate concern from position validity
                if (savedState != null && savedState.Width > 0 && savedState.Height > 0)
                {
                    // First, set the size
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] Restoring saved size: {savedState.Width}x{savedState.Height}");
                    var beforeResize = appWindow.Size;
                    appWindow.Resize(new Windows.Graphics.SizeInt32(savedState.Width, savedState.Height));
                    var afterResize = appWindow.Size;
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] Window size before={beforeResize.Width}x{beforeResize.Height}, after={afterResize.Width}x{afterResize.Height}");
                    
                    // Then validate and restore position
                    if (WindowSettings.IsValidPosition(savedState, new Windows.Graphics.SizeInt32 { Width = savedState.Width, Height = savedState.Height }))
                    {
                        // Position is valid - restore it
                        System.Diagnostics.Debug.WriteLine($"[MainWindow] Restoring saved position: {savedState.X},{savedState.Y}");
                        appWindow.Move(new Windows.Graphics.PointInt32(savedState.X, savedState.Y));
                    }
                    else
                    {
                        // Position is invalid - center the window but keep the restored size
                        System.Diagnostics.Debug.WriteLine($"[MainWindow] Saved position invalid, centering with restored size");
                        var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
                        var centeredPos = WindowSettings.GetCenteredPosition(displayArea, new Windows.Graphics.SizeInt32 { Width = savedState.Width, Height = savedState.Height });
                        appWindow.Move(centeredPos);
                    }
                }
                else
                {
                    // No valid saved state - center on primary display at default size
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] No valid saved size, centering window on primary display");
                    var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
                    var centeredPos = WindowSettings.GetCenteredPosition(displayArea, appWindow.Size);
                    appWindow.Move(centeredPos);
                }
            });
            task.Wait();
        }

        /// <summary>
        /// Saves the current window position and size to local storage.
        /// </summary>
        private void SaveWindowPosition()
        {
            var appWindow = AppWindow;
            if (appWindow == null) return;

            var position = appWindow.Position;
            var size = appWindow.Size;

            _ = WindowSettings.SaveAsync(position.X, position.Y, size.Width, size.Height);
        }

        /// <summary>
        /// Saves the window position synchronously (blocks until save completes).
        /// Used for the Closed event to ensure settings are saved before app exits.
        /// </summary>
        private void SaveWindowPositionSync()
        {
            var appWindow = AppWindow;
            if (appWindow == null) return;

            var position = appWindow.Position;
            var size = appWindow.Size;

            // Block until the async save completes - short timeout for instant close
            var task = WindowSettings.SaveAsync(position.X, position.Y, size.Width, size.Height);
            task.Wait(TimeSpan.FromMilliseconds(200)); // 200ms timeout for fast close
        }

        private void EnforceMinimumWindowSize(AppWindow appWindow)
        {
            var size = appWindow.Size;
            var width = Math.Max(_minWidth, size.Width);
            var height = Math.Max(_minHeight, size.Height);

            if (width == size.Width && height == size.Height) return;

            appWindow.Resize(new SizeInt32
            {
                Width = width,
                Height = height
            });
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
