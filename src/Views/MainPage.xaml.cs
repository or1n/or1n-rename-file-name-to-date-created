using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Or1nRenameFileNameToDateCreated.Views
{
    /// <summary>
    /// Main page with responsive layout, entrance animations, and enhanced accessibility.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<string> _logLines = new();
        private bool _folderSelected = false;
        private string _selectedFolderPath = string.Empty;
        private const string DEFAULT_ACTION_TEXT = "Action: Select a folder to start";

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        /// <summary>
        /// Performs entrance animations when page is loaded.
        /// </summary>
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[MainPage_Loaded] Page loaded");
            
            // Staggered entrance animations for a polished appearance
            AnimateElementEntrance(TitleText, TitleTransform, 0);
            AnimateElementEntrance(DescriptionText, DescriptionTransform, 40);
            AnimateElementEntrance(ActionRow, ThemeComboTransform, 80);
            AnimateElementEntrance(LogBorder, LogBorderTransform, 160);

            SetAction(DEFAULT_ACTION_TEXT);

            SetThemeComboToSystemPreference();
            
            // Add keyboard navigation support for arrow keys
            this.KeyDown += MainPage_KeyDown;
        }
        
        /// <summary>
        /// Handles keyboard navigation with arrow keys and Enter/Space.
        /// </summary>
        private void MainPage_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var key = e.Key;
            var focusedElement = FocusManager.GetFocusedElement(this.XamlRoot) as Control;
            
            if (focusedElement == null) return;

            // Handle arrow key navigation
            if (key == Windows.System.VirtualKey.Right || key == Windows.System.VirtualKey.Down)
            {
                FocusManager.TryMoveFocus(Microsoft.UI.Xaml.Input.FocusNavigationDirection.Next);
                e.Handled = true;
            }
            else if (key == Windows.System.VirtualKey.Left || key == Windows.System.VirtualKey.Up)
            {
                FocusManager.TryMoveFocus(Microsoft.UI.Xaml.Input.FocusNavigationDirection.Previous);
                e.Handled = true;
            }
        }

        private void SetThemeComboToSystemPreference()
        {
            if (ThemeComboBox == null) return;

            // Try to load saved theme preference
            var task = Task.Run(async () =>
            {
                var settings = await Or1nRenameFileNameToDateCreated.Helpers.WindowSettings.LoadAsync();
                return settings?.Theme;
            });
            task.Wait();
            var savedTheme = task.Result;

            if (!string.IsNullOrEmpty(savedTheme))
            {
                // Use saved theme
                ThemeComboBox.SelectedIndex = savedTheme == "Dark" ? 1 : 0;
            }
            else
            {
                // Use system preference
                var systemTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark
                    ? ElementTheme.Dark
                    : ElementTheme.Light;

                ThemeComboBox.SelectedIndex = systemTheme == ElementTheme.Dark ? 1 : 0;
            }
        }

        /// <summary>
        /// Animates an element's entrance with slide-up and fade-in.
        /// </summary>
        /// <param name="element">The UI element to animate.</param>
        /// <param name="transform">The TranslateTransform to animate.</param>
        /// <param name="delayMs">Delay before animation starts in milliseconds.</param>
        private void AnimateElementEntrance(FrameworkElement element, TranslateTransform transform, int delayMs)
        {
            if (element == null || transform == null) return;

            // Initial state
            element.Opacity = 0;
            transform.Y = 20;
            
            var storyboard = new Storyboard();
            storyboard.BeginTime = TimeSpan.FromMilliseconds(delayMs);

            // Slide up animation
            var slideAnimation = new DoubleAnimation
            {
                From = 20,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(slideAnimation, transform);
            Storyboard.SetTargetProperty(slideAnimation, "Y");
            storyboard.Children.Add(slideAnimation);

            // Fade in animation
            var fadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(250)
            };
            Storyboard.SetTarget(fadeAnimation, element);
            Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
            storyboard.Children.Add(fadeAnimation);

            storyboard.Begin();
        }

        private static void SetWindowSize(int width, int height)
        {
            System.Diagnostics.Debug.WriteLine($"[SetWindowSize] Setting window size to {width}x{height}");
        }

        private void Log(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            _logLines.Add($"[{timestamp}] {message}");
            
            // Remove from BEGINNING (oldest entries) when exceeding 100 lines
            while (_logLines.Count > 100)
                _logLines.RemoveAt(0);
            
            if (InfoTextBlock != null)
            {
                // Show LAST 10 lines (most recent)
                InfoTextBlock.Text = string.Join("\n", _logLines.Skip(Math.Max(0, _logLines.Count - 10)));
                AutoScrollLog();
            }
        }

        private void SetAction(string actionText)
        {
            if (ActionTextBlock != null)
            {
                ActionTextBlock.Text = actionText;
            }
        }

        private void AutoScrollLog()
        {
            if (LogScrollViewer == null) return;

            // Force immediate scroll to bottom with slight delay to ensure content is rendered
            LogScrollViewer.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                LogScrollViewer.UpdateLayout();
                LogScrollViewer.ChangeView(null, LogScrollViewer.ScrollableHeight, null, true);
            });
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[ThemeComboBox_SelectionChanged] Selection changed");
            try
            {
                if (ThemeComboBox != null && ThemeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
                {
                    System.Diagnostics.Debug.WriteLine($"[ThemeComboBox_SelectionChanged] Selected tag: {tag}");
                    ElementTheme theme = tag switch
                    {
                        "Light" => ElementTheme.Light,
                        "Dark" => ElementTheme.Dark,
                        _ => ElementTheme.Default
                    };
                    
                    // Apply theme to the Page
                    this.RequestedTheme = theme;
                    
                    // Also apply theme to the Window's content root for title bar update
                    if (this.XamlRoot?.Content is FrameworkElement root)
                    {
                        root.RequestedTheme = theme;
                    }
                    
                    // Get MainWindow and trigger title bar update
                    var window = WindowHelper.GetWindowForElement(this);
                    if (window is MainWindow mainWindow)
                    {
                        mainWindow.UpdateTitleBarTheme(theme);
                    }

                    // Save theme preference
                    _ = Or1nRenameFileNameToDateCreated.Helpers.WindowSettings.SaveThemeAsync(tag);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeComboBox_SelectionChanged] Exception: {ex}");
                throw;
            }
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Log("Function \"Open file explorer to select a folder\" is not implemented yet.");
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_folderSelected || string.IsNullOrWhiteSpace(_selectedFolderPath))
            {
                Log("Please select a folder first.");
                return;
            }
            try
            {
                Log($"Scanning folder: {_selectedFolderPath}");
                var dir = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(_selectedFolderPath);
                var files = await dir.GetFilesAsync();
                var groups = files.GroupBy(f => f.FileType.ToUpperInvariant())
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count);
                foreach (var group in groups)
                {
                    Log($"{group.Type}: {group.Count}");
                }
                Log($"Scan complete. {files.Count} files found.");
            }
            catch (ArgumentException ex)
            {
                Log($"Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Hover/Press Animation Handlers for WinUI 3 Motion System
        /// </summary>
        private void Button_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Get or create ScaleTransform
                if (button.RenderTransform == null || button.RenderTransform is not ScaleTransform)
                {
                    button.RenderTransform = new ScaleTransform { CenterX = 20, CenterY = 20 };
                    button.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
                }
                if (button.RenderTransform is ScaleTransform scale)
                {
                    AnimateScale(scale, 1.05, 100); // Scale up slightly on hover
                }
            }
        }

        private void Button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button button && button.RenderTransform is ScaleTransform scale)
            {
                AnimateScale(scale, 1.0, 100); // Return to normal size
            }
        }

        private void Button_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button button && button.RenderTransform is ScaleTransform scale)
            {
                AnimateScale(scale, 0.95, 50); // Scale down on press
            }
        }

        private void Button_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button button && button.RenderTransform is ScaleTransform scale)
            {
                AnimateScale(scale, 1.05, 50); // Return to hover size
            }
        }

        private void AnimateScale(ScaleTransform transform, double toScale, int durationMs)
        {
            var storyboard = new Storyboard();
            
            var scaleXAnimation = new DoubleAnimation
            {
                To = toScale,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleXAnimation, transform);
            Storyboard.SetTargetProperty(scaleXAnimation, "ScaleX");
            storyboard.Children.Add(scaleXAnimation);

            var scaleYAnimation = new DoubleAnimation
            {
                To = toScale,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleYAnimation, transform);
            Storyboard.SetTargetProperty(scaleYAnimation, "ScaleY");
            storyboard.Children.Add(scaleYAnimation);

            storyboard.Begin();
        }
    }
}
