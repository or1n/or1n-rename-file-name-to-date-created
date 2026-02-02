using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Documents;
using Windows.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

namespace Or1nRenameFileNameToDateCreated.Views
{
    /// <summary>
    /// Main page with responsive layout, entrance animations, and enhanced accessibility.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly List<LogEntry> _logEntries = new();
        private bool _folderSelected = false;
        private string _selectedFolderPath = string.Empty;
        private const string DEFAULT_ACTION_TEXT = "Action: Select a folder to start processing";
        private const double LOG_LINE_HEIGHT = 20;
        private bool _isAutoScrollEnabled = true;
        private bool _isSnappingScroll = false;
        private DispatcherQueueTimer? _resizeLogTimer;
        private Windows.Foundation.Size _pendingWindowSize;

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
            SetThemeComboToSystemPreference();
            SetAction(DEFAULT_ACTION_TEXT);
            // Add keyboard navigation support for arrow keys
            this.KeyDown += MainPage_KeyDown;

            InitializeResizeLogging();
        }

        private void InitializeResizeLogging()
        {

            _resizeLogTimer = DispatcherQueue.CreateTimer();
            _resizeLogTimer.Interval = TimeSpan.FromMilliseconds(500);
            _resizeLogTimer.IsRepeating = false;
            _resizeLogTimer.Tick += (_, _) =>
            {
                Log($"Window resized to {(int)_pendingWindowSize.Width}x{(int)_pendingWindowSize.Height}");
            };
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
                Duration = TimeSpan.FromMilliseconds(167),
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
                Duration = TimeSpan.FromMilliseconds(167)
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
            var timestamp = DateTime.Now;
            var level = GetLogLevel(message);
            _logEntries.Add(new LogEntry(timestamp, message, level));
            
            // Remove from BEGINNING (oldest entries) when exceeding 100 lines
            while (_logEntries.Count > 100)
                _logEntries.RemoveAt(0);
            
            UpdateLogText();
        }

        private void UpdateLogText()
        {
            if (InfoRichTextBlock == null) return;

            InfoRichTextBlock.Blocks.Clear();

            foreach (var entry in _logEntries)
            {
                AppendLogEntry(entry);
            }

            ForceScrollToBottom();
        }

        private void SetAction(string actionText)
        {
            if (ActionTextBlock != null)
            {
                ActionTextBlock.Text = actionText;
            }
        }

        private void CopyAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var text = string.Join(Environment.NewLine, _logEntries.Select(FormatLogLine));
            var dataPackage = new DataPackage();
            dataPackage.SetText(text ?? string.Empty);
            Clipboard.SetContent(dataPackage);
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (InfoRichTextBlock != null && !string.IsNullOrEmpty(InfoRichTextBlock.SelectedText))
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(InfoRichTextBlock.SelectedText);
                Clipboard.SetContent(dataPackage);
            }
        }

        private void SelectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (InfoRichTextBlock != null)
            {
                InfoRichTextBlock.SelectAll();
            }
        }

        private static LogLevel GetLogLevel(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return LogLevel.Info;

            var normalized = message.ToLowerInvariant();
            if (normalized.Contains("error") || normalized.StartsWith("error")) return LogLevel.Error;
            if (normalized.Contains("warn")) return LogLevel.Warning;
            if (normalized.Contains("success") || normalized.Contains("complete")) return LogLevel.Success;
            if (normalized.Contains("debug") || normalized.Contains("trace")) return LogLevel.Debug;
            return LogLevel.Info;
        }

        private void AppendLogEntry(LogEntry entry)
        {
            if (InfoRichTextBlock == null) return;

            var timestamp = entry.Timestamp.ToString("yy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 0), LineHeight = LOG_LINE_HEIGHT };

            // Get timestamp brush - light gray in both themes
            var timestampBrush = GetThemeAwareBrush("LogTimestampBrush");
            paragraph.Inlines.Add(new Run
            {
                Text = $"[{timestamp}] ",
                Foreground = timestampBrush
            });

            // Get message brush - theme-aware via resource lookup or theme
            var messageBrush = GetThemeAwareBrush(GetBrushKey(entry.Level));
            paragraph.Inlines.Add(new Run
            {
                Text = entry.Message,
                Foreground = messageBrush
            });

            InfoRichTextBlock.Blocks.Add(paragraph);
        }

        private static string FormatLogLine(LogEntry entry)
        {
            var timestamp = entry.Timestamp.ToString("yy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            return $"[{timestamp}] {entry.Message}";
        }

        private static string GetBrushKey(LogLevel level)
        {
            return level switch
            {
                LogLevel.Error => "LogErrorBrush",
                LogLevel.Warning => "LogWarningBrush",
                LogLevel.Success => "LogSuccessBrush",
                LogLevel.Debug => "LogDebugBrush",
                _ => "LogInfoBrush"
            };
        }

        private SolidColorBrush GetThemeAwareBrush(string resourceKey)
        {
            // Always determine theme dynamically - don't cache theme at load time
            // this.ActualTheme will be correct at render time
            var isLightTheme = this.ActualTheme == ElementTheme.Light;

            return resourceKey switch
            {
                "LogTimestampBrush" => new SolidColorBrush(new Color { A = 255, R = 97, G = 97, B = 97 }),
                "LogInfoBrush" => new SolidColorBrush(new Color { A = 255, R = (byte)(isLightTheme ? 26 : 255), G = (byte)(isLightTheme ? 26 : 255), B = (byte)(isLightTheme ? 26 : 255) }),
                "LogWarningBrush" => new SolidColorBrush(new Color { A = 255, R = (byte)(isLightTheme ? 184 : 255), G = (byte)(isLightTheme ? 134 : 201), B = (byte)(isLightTheme ? 11 : 60) }),
                "LogErrorBrush" => new SolidColorBrush(new Color { A = 255, R = (byte)(isLightTheme ? 180 : 255), G = (byte)(isLightTheme ? 55 : 107), B = (byte)(isLightTheme ? 59 : 107) }),
                "LogSuccessBrush" => new SolidColorBrush(new Color { A = 255, R = (byte)(isLightTheme ? 11 : 81), G = (byte)(isLightTheme ? 102 : 207), B = (byte)(isLightTheme ? 35 : 102) }),
                "LogDebugBrush" => new SolidColorBrush(new Color { A = 255, R = (byte)(isLightTheme ? 74 : 197), G = (byte)(isLightTheme ? 74 : 197), B = (byte)(isLightTheme ? 74 : 197) }),
                _ => new SolidColorBrush(new Color { A = 255, R = (byte)(isLightTheme ? 26 : 255), G = (byte)(isLightTheme ? 26 : 255), B = (byte)(isLightTheme ? 26 : 255) })
            };
        }

        private static SolidColorBrush GetLogBrush(string resourceKey, SolidColorBrush fallback)
        {
            return Application.Current.Resources[resourceKey] as SolidColorBrush ?? fallback;
        }

        private enum LogLevel
        {
            Info,
            Warning,
            Error,
            Success,
            Debug
        }

        private sealed record LogEntry(DateTime Timestamp, string Message, LogLevel Level);


        private void ForceScrollToBottom()
        {
            if (LogScrollViewer == null) return;

            // Always force scroll to bottom to show latest content
            LogScrollViewer.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                LogScrollViewer.UpdateLayout();
                LogScrollViewer.ChangeView(null, LogScrollViewer.ScrollableHeight, null, true);
            });
        }

        private void AutoScrollLog()
        {
            if (LogScrollViewer == null) return;

            if (!_isAutoScrollEnabled) return;

            // Force immediate scroll to bottom with slight delay to ensure content is rendered
            LogScrollViewer.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                LogScrollViewer.UpdateLayout();
                LogScrollViewer.ChangeView(null, LogScrollViewer.ScrollableHeight, null, true);
            });
        }

        private void LogScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (LogScrollViewer == null) return;

            if (!e.IsIntermediate)
            {
                _isAutoScrollEnabled = true;

                if (!_isSnappingScroll && LogScrollViewer.ScrollableHeight > 0)
                {
                    var snappedOffset = Math.Round(LogScrollViewer.VerticalOffset / LOG_LINE_HEIGHT) * LOG_LINE_HEIGHT;
                    if (Math.Abs(snappedOffset - LogScrollViewer.VerticalOffset) > 0.5)
                    {
                        _isSnappingScroll = true;
                        LogScrollViewer.ChangeView(null, snappedOffset, null, true);
                        _isSnappingScroll = false;
                    }
                }
            }
        }

        private void LogScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ForceScrollToBottom();
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

                    Log($"Theme changed to {tag}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeComboBox_SelectionChanged] Exception: {ex}");
                throw;
            }
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _pendingWindowSize = e.NewSize;
            if (_resizeLogTimer == null)
            {
                InitializeResizeLogging();
            }

            _resizeLogTimer?.Stop();
            _resizeLogTimer?.Start();
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log("=== [FOLDER PICKER] Button clicked ===");
                Log("[FOLDER PICKER] Creating custom WinUI 3 folder browser dialog...");

                // Show custom folder browser window (now a proper Window, not ContentDialog)
                var folderBrowserDialog = new FolderBrowserDialog();
                var result = await folderBrowserDialog.ShowAsync();

                if (result)
                {
                    string selectedPath = folderBrowserDialog.SelectedPath;
                    Log("[FOLDER PICKER] Folder selected successfully");
                    
                    try
                    {
                        _selectedFolderPath = selectedPath;
                        _folderSelected = true;
                        Log($"[FOLDER PICKER] Path: {_selectedFolderPath}");

                        // Get files in folder using sync Directory API (works in unpackaged apps)
                        Log("[FOLDER PICKER] Querying folder contents...");
                        var files = Directory.GetFiles(_selectedFolderPath, "*", SearchOption.TopDirectoryOnly);
                        Log($"[FOLDER PICKER] Total files: {files.Length}");

                        if (files.Length > 0)
                        {
                            var grouped = files
                                .GroupBy(f => Path.GetExtension(f).ToLowerInvariant())
                                .OrderByDescending(g => g.Count())
                                .Take(10)
                                .ToList();
                            
                            Log($"[FOLDER PICKER] File types: {grouped.Count} found");
                            foreach (var group in grouped)
                            {
                                string ext = string.IsNullOrEmpty(group.Key) ? "[no extension]" : group.Key;
                                Log($"[FOLDER PICKER]   {ext}: {group.Count()}");
                            }

                            if (grouped.Count > 10)
                                Log($"[FOLDER PICKER]   ... and more");
                        }

                        // Update UI
                        SetAction("Action: Scan folder to analyze files");

                        Log($"[FOLDER PICKER] === SUCCESS ===");
                        Log($"[FOLDER PICKER] Folder ready for processing");
                    }
                    catch (Exception ex)
                    {
                        Log($"[FOLDER PICKER] ERROR: {ex.Message}");
                    }
                }
                else
                {
                    Log("[FOLDER PICKER] Folder selection cancelled by user");
                }
            }
            catch (Exception ex)
            {
                Log($"[FOLDER PICKER] CRITICAL ERROR: {ex.GetType().Name}");
                Log($"[FOLDER PICKER] Message: {(string.IsNullOrEmpty(ex.Message) ? "(empty)" : ex.Message)}");
                if (ex is COMException comEx)
                {
                    Log($"[FOLDER PICKER] HResult: 0x{comEx.HResult:X8}");
                }
                if (ex.InnerException != null)
                {
                    Log($"[FOLDER PICKER] Inner: {ex.InnerException.Message}");
                }
            }
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
                Log($"Scan complete | {files.Count} files found");
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
        private void InteractiveControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Control control)
            {
                // Get or create ScaleTransform
                if (control.RenderTransform == null || control.RenderTransform is not ScaleTransform)
                {
                    control.RenderTransform = new ScaleTransform { CenterX = 20, CenterY = 20 };
                    control.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
                }
                if (control.RenderTransform is ScaleTransform scale)
                {
                    AnimateScale(scale, 1.02, 100); // Hover response
                }
            }
        }

        private void InteractiveControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Control control && control.RenderTransform is ScaleTransform scale)
            {
                AnimateScale(scale, 1.0, 100); // Return to normal size
            }
        }

        private void InteractiveControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Control control && control.RenderTransform is ScaleTransform scale)
            {
                AnimateScale(scale, 0.98, 100); // Press feedback
            }
        }

        private void InteractiveControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Control control && control.RenderTransform is ScaleTransform scale)
            {
                AnimateScale(scale, 1.02, 100); // Return to hover size
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
