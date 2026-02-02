using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Documents;
using Windows.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using Or1nRenameFileNameToDateCreated.Helpers;

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
        private bool _isMiddleMouseScrolling = false;
        private double _middleScrollStartOffset = 0;
        private Windows.Foundation.Point _middleScrollStartPoint;
        private bool _isScanning = false;
        private readonly ConcurrentQueue<LogEntry> _logQueue = new();
        private DispatcherQueueTimer? _logFlushTimer;
        private DateTime _lastProgressUiUpdate = DateTime.MinValue;
        private DateTime _lastProgressLogUpdate = DateTime.MinValue;
        private double _lastProgressLogPercent = 0;
        private const int PROGRESS_UI_THROTTLE_MS = 100;
        private const int PROGRESS_LOG_THROTTLE_MS = 1000;
        private const double PROGRESS_PERCENT_THRESHOLD = 1.0;
        private bool _enableAnimations = true;
        private double _animationSpeedMultiplier = 1.0;
        private bool _smoothScrollingEnabled = true;
        private string _logTimestampFormat = "24Hour";
        private readonly Dictionary<Run, LogLevel> _messageRuns = new();
        private readonly HashSet<Run> _timestampRuns = new();
        
        // Cached brushes to avoid recursive lookups during theme changes
        private SolidColorBrush? _cachedTimestampBrush;
        private SolidColorBrush? _cachedInfoBrush;
        private SolidColorBrush? _cachedWarningBrush;
        private SolidColorBrush? _cachedErrorBrush;
        private SolidColorBrush? _cachedSuccessBrush;
        private SolidColorBrush? _cachedDebugBrush;
        private bool _isThemeTransitionInProgress = false;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
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
            InitializeLogBuffer();

            ApplySettingsFromService();
            SettingsService.Instance.PropertyChanged += SettingsService_PropertyChanged;
            ActualThemeChanged += MainPage_ActualThemeChanged;
            
            // Cache theme brushes to avoid recursive lookups
            CacheThemeBrushes();
            
            // Display current application version without the "v" prefix
            if (VersionText != null)
            {
                var fullVersion = Or1nRenameFileNameToDateCreated.Helpers.VersionService.GetCurrentVersion();
                // Remove the "v" prefix (e.g., "v2026.02.02..." -> "2026.02.02...")
                var versionWithoutPrefix = fullVersion.TrimStart('v');
                VersionText.Text = $"version {versionWithoutPrefix}";
            }
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SettingsService.Instance.PropertyChanged -= SettingsService_PropertyChanged;
            ActualThemeChanged -= MainPage_ActualThemeChanged;
        }

        private async void MainPage_ActualThemeChanged(FrameworkElement sender, object args)
        {
            if (_isThemeTransitionInProgress)
            {
                return;
            }

            _isThemeTransitionInProgress = true;

            // Update brushes IMMEDIATELY so color change is instant and synchronized
            CacheThemeBrushes();
            RefreshLogBrushes();

            // Then animate the transition smoothly over the already-updated colors
            if (_enableAnimations)
            {
                await AnimateThemeTransitionAsync();
            }

            _isThemeTransitionInProgress = false;
        }

        /// <summary>
        /// Performs a smooth theme transition across the whole page with a subtle pulse effect.
        /// Uses a single fluid animation for perfect synchronization.
        /// </summary>
        private async Task AnimateThemeTransitionAsync()
        {
            if (RootGrid == null)
            {
                return;
            }

            // Fast, smooth pulse: 150ms total with QuadraticEase for butter-smooth motion
            var duration = GetAnimationDurationMs(150);

            // Single pulse animation that goes: 100% → 96% → 100%
            // This creates a subtle "acknowledgment" effect that's faster and smoother than two separate fades
            var storyboard = new Storyboard { Duration = TimeSpan.FromMilliseconds(duration) };
            
            var opacityAnim = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(duration) };
            
            // Start at 100%
            opacityAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 1.0 });
            
            // Smooth ease to 96% at midpoint (75ms) - subtle dip
            opacityAnim.KeyFrames.Add(new EasingDoubleKeyFrame 
            { 
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration * 0.5)), 
                Value = 0.96,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
            
            // Smooth ease back to 100% at end (150ms) - polished return
            opacityAnim.KeyFrames.Add(new EasingDoubleKeyFrame 
            { 
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(duration)), 
                Value = 1.0,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            });

            Storyboard.SetTarget(opacityAnim, RootGrid);
            Storyboard.SetTargetProperty(opacityAnim, "Opacity");
            storyboard.Children.Add(opacityAnim);

            var tcs = new TaskCompletionSource<bool>();
            storyboard.Completed += (s, e) => tcs.SetResult(true);
            storyboard.Begin();
            await tcs.Task;
        }

        /// <summary>
        /// Animates opacity for a UI element with easing.
        /// </summary>
        private async Task AnimateOpacityAsync(UIElement element, double from, double to, int durationMs)
        {
            var storyboard = new Storyboard { Duration = TimeSpan.FromMilliseconds(durationMs) };
            var doubleAnim = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(doubleAnim, element);
            Storyboard.SetTargetProperty(doubleAnim, "Opacity");
            storyboard.Children.Add(doubleAnim);

            var tcs = new TaskCompletionSource<bool>();
            storyboard.Completed += (s, e) => tcs.SetResult(true);
            storyboard.Begin();
            await tcs.Task;
        }

        private void SettingsService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var settings = SettingsService.Instance;

            switch (e.PropertyName)
            {
                case nameof(SettingsService.EnableAnimations):
                    _enableAnimations = settings.EnableAnimations;
                    break;
                case nameof(SettingsService.AnimationSpeedMultiplier):
                    _animationSpeedMultiplier = Math.Max(0.1, settings.AnimationSpeedMultiplier);
                    break;
                case nameof(SettingsService.SmoothScrolling):
                    _smoothScrollingEnabled = settings.SmoothScrolling;
                    if (LogScrollViewer != null)
                    {
                        LogScrollViewer.IsScrollInertiaEnabled = _smoothScrollingEnabled;
                    }
                    break;
                case nameof(SettingsService.AutoScrollLog):
                    _isAutoScrollEnabled = settings.AutoScrollLog;
                    break;
                case nameof(SettingsService.LogTimestampFormat):
                    _logTimestampFormat = settings.LogTimestampFormat;
                    UpdateLogText();
                    break;
                case nameof(SettingsService.LogColorScheme):
                    RefreshLogBrushes();
                    break;
                case nameof(SettingsService.Theme):
                    ApplyThemeFromSettings(settings.Theme);
                    break;
            }
        }

        private void ApplySettingsFromService()
        {
            var settings = SettingsService.Instance;

            _enableAnimations = settings.EnableAnimations;
            _animationSpeedMultiplier = Math.Max(0.1, settings.AnimationSpeedMultiplier);
            _smoothScrollingEnabled = settings.SmoothScrolling;
            _isAutoScrollEnabled = settings.AutoScrollLog;
            _logTimestampFormat = settings.LogTimestampFormat;

            if (LogScrollViewer != null)
            {
                LogScrollViewer.IsScrollInertiaEnabled = _smoothScrollingEnabled;
            }

            ApplyThemeFromSettings(settings.Theme);
        }

        private void ApplyThemeFromSettings(string theme)
        {
            RequestedTheme = ThemeManager.ParseTheme(theme);
            RefreshLogBrushes();
        }

        /// <summary>
        /// Rebuilds the log output so existing entries use current theme brushes.
        /// </summary>
        public void RefreshLogColors()
        {
            RefreshLogBrushes();
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

            var theme = SettingsService.Instance.Theme;
            ThemeComboBox.SelectedIndex = theme == "Dark" ? 1 : 0;
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

            if (!_enableAnimations)
            {
                element.Opacity = 1;
                transform.Y = 0;
                return;
            }

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
                Duration = TimeSpan.FromMilliseconds(GetAnimationDurationMs(167)),
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
                Duration = TimeSpan.FromMilliseconds(GetAnimationDurationMs(167))
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

            if (level == LogLevel.Debug && !SettingsService.Instance.DebugMode)
            {
                return;
            }

            _logQueue.Enqueue(new LogEntry(timestamp, message, level));

            if (DispatcherQueue.HasThreadAccess)
            {
                FlushLogQueue();
            }
        }

        private void UpdateLogText()
        {
            if (InfoRichTextBlock == null) return;

            _messageRuns.Clear();
            _timestampRuns.Clear();
            InfoRichTextBlock.Blocks.Clear();

            foreach (var entry in _logEntries)
            {
                AppendLogEntry(entry);
            }

            AutoScrollLog();
        }

        /// <summary>
        /// Caches all theme-aware brushes to avoid recursive dictionary lookups.
        /// Call this once at startup and whenever the theme changes.
        /// </summary>
        private void CacheThemeBrushes()
        {
            _cachedTimestampBrush = GetThemeAwareBrush("LogTimestampBrush");
            _cachedInfoBrush = GetThemeAwareBrush("LogInfoBrush");
            _cachedWarningBrush = GetThemeAwareBrush("LogWarningBrush");
            _cachedErrorBrush = GetThemeAwareBrush("LogErrorBrush");
            _cachedSuccessBrush = GetThemeAwareBrush("LogSuccessBrush");
            _cachedDebugBrush = GetThemeAwareBrush("LogDebugBrush");
        }

        private void RefreshLogBrushes()
        {
            if (InfoRichTextBlock == null) return;

            // Use cached brushes instead of recursive lookups
            var timestampBrush = _cachedTimestampBrush ?? GetThemeAwareBrush("LogTimestampBrush");

            foreach (var run in _timestampRuns)
            {
                run.Foreground = timestampBrush;
            }

            foreach (var kvp in _messageRuns)
            {
                var brushKey = GetBrushKey(kvp.Value);
                var messageBrush = brushKey switch
                {
                    "LogTimestampBrush" => _cachedTimestampBrush ?? GetThemeAwareBrush(brushKey),
                    "LogInfoBrush" => _cachedInfoBrush ?? GetThemeAwareBrush(brushKey),
                    "LogWarningBrush" => _cachedWarningBrush ?? GetThemeAwareBrush(brushKey),
                    "LogErrorBrush" => _cachedErrorBrush ?? GetThemeAwareBrush(brushKey),
                    "LogSuccessBrush" => _cachedSuccessBrush ?? GetThemeAwareBrush(brushKey),
                    "LogDebugBrush" => _cachedDebugBrush ?? GetThemeAwareBrush(brushKey),
                    _ => GetThemeAwareBrush(brushKey)
                };
                kvp.Key.Foreground = messageBrush;
            }
        }

        private void InitializeLogBuffer()
        {
            _logFlushTimer = DispatcherQueue.CreateTimer();
            _logFlushTimer.Interval = TimeSpan.FromMilliseconds(200);
            _logFlushTimer.IsRepeating = true;
            _logFlushTimer.Tick += (_, _) => FlushLogQueue();
            _logFlushTimer.Start();
        }

        private void FlushLogQueue()
        {
            if (!DispatcherQueue.HasThreadAccess)
            {
                DispatcherQueue.TryEnqueue(FlushLogQueue);
                return;
            }

            if (InfoRichTextBlock == null) return;

            bool added = false;
            while (_logQueue.TryDequeue(out var entry))
            {
                _logEntries.Add(entry);
                AppendLogEntry(entry);
                added = true;
            }

            if (added)
            {
                AutoScrollLog();
            }
        }

        /// <summary>
        /// Clears all log entries from the display and internal storage.
        /// This is called by the Settings window when clearing logs.
        /// </summary>
        public void ClearLog()
        {
            if (!DispatcherQueue.HasThreadAccess)
            {
                DispatcherQueue.TryEnqueue(ClearLog);
                return;
            }

            _logEntries.Clear();
            _logQueue.Clear();
            _messageRuns.Clear();
            _timestampRuns.Clear();
            if (InfoRichTextBlock != null)
            {
                InfoRichTextBlock.Blocks.Clear();
            }
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

            var timestampText = GetTimestampText(entry.Timestamp);
            var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 0), LineHeight = LOG_LINE_HEIGHT };

            if (!string.IsNullOrWhiteSpace(timestampText))
            {
                // Get timestamp brush - light gray in both themes
                var timestampBrush = GetThemeAwareBrush("LogTimestampBrush");
                var timestampRun = new Run
                {
                    Text = $"[{timestampText}] ",
                    Foreground = timestampBrush
                };
                _timestampRuns.Add(timestampRun);
                paragraph.Inlines.Add(timestampRun);
            }

            // Get message brush - theme-aware via resource lookup or theme
            var messageBrush = GetThemeAwareBrush(GetBrushKey(entry.Level));
            var messageRun = new Run
            {
                Text = entry.Message,
                Foreground = messageBrush
            };
            _messageRuns[messageRun] = entry.Level;
            paragraph.Inlines.Add(messageRun);

            InfoRichTextBlock.Blocks.Add(paragraph);
        }

        private string FormatLogLine(LogEntry entry)
        {
            var timestampText = GetTimestampText(entry.Timestamp);
            return string.IsNullOrWhiteSpace(timestampText)
                ? entry.Message
                : $"[{timestampText}] {entry.Message}";
        }

        private string GetTimestampText(DateTime timestamp)
        {
            return _logTimestampFormat switch
            {
                "None" => string.Empty,
                "12Hour" => timestamp.ToString("yy/MM/dd hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture),
                "ISO" => timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture),
                _ => timestamp.ToString("yy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
            };
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
            var themeKey = ActualTheme == ElementTheme.Dark ? "Dark" : "Light";

            if (TryGetThemedBrush(Application.Current.Resources, themeKey, resourceKey, out var themedBrush))
            {
                return themedBrush;
            }

            var fallback = ActualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            return new SolidColorBrush(fallback);
        }

        private static bool TryGetThemedBrush(ResourceDictionary dictionary, string themeKey, string resourceKey, out SolidColorBrush brush)
        {
            if (dictionary.ThemeDictionaries.TryGetValue(themeKey, out var themeDictObj) && themeDictObj is ResourceDictionary themeDict)
            {
                if (TryGetBrush(themeDict, resourceKey, out brush))
                {
                    return true;
                }
            }

            foreach (var merged in dictionary.MergedDictionaries)
            {
                if (TryGetThemedBrush(merged, themeKey, resourceKey, out brush))
                {
                    return true;
                }
            }

            return TryGetBrush(dictionary, resourceKey, out brush);
        }

        private static bool TryGetBrush(ResourceDictionary dictionary, string resourceKey, out SolidColorBrush brush)
        {
            if (dictionary.TryGetValue(resourceKey, out var value))
            {
                if (value is SolidColorBrush foundBrush)
                {
                    brush = foundBrush;
                    return true;
                }

                if (value is Color color)
                {
                    brush = new SolidColorBrush(color);
                    return true;
                }
            }

            brush = null!;
            return false;
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
                LogScrollViewer.ChangeView(null, LogScrollViewer.ScrollableHeight, null, !_smoothScrollingEnabled);
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
                LogScrollViewer.ChangeView(null, LogScrollViewer.ScrollableHeight, null, !_smoothScrollingEnabled);
            });
        }

        private void LogScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (LogScrollViewer == null) return;

            if (!e.IsIntermediate)
            {
                var distanceToBottom = LogScrollViewer.ScrollableHeight - LogScrollViewer.VerticalOffset;
                _isAutoScrollEnabled = distanceToBottom <= LOG_LINE_HEIGHT;

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

        private void LogScrollViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (LogScrollViewer == null) return;

            var point = e.GetCurrentPoint(LogScrollViewer);
            if (point.Properties.IsMiddleButtonPressed)
            {
                _isMiddleMouseScrolling = true;
                _middleScrollStartPoint = point.Position;
                _middleScrollStartOffset = LogScrollViewer.VerticalOffset;
                LogScrollViewer.CapturePointer(e.Pointer);
                e.Handled = true;
            }
        }

        private void LogScrollViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isMiddleMouseScrolling || LogScrollViewer == null) return;

            var point = e.GetCurrentPoint(LogScrollViewer);
            var deltaY = point.Position.Y - _middleScrollStartPoint.Y;
            var targetOffset = _middleScrollStartOffset - deltaY;
            targetOffset = Math.Max(0, Math.Min(targetOffset, LogScrollViewer.ScrollableHeight));

            LogScrollViewer.ChangeView(null, targetOffset, null, true);
            e.Handled = true;
        }

        private void LogScrollViewer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!_isMiddleMouseScrolling || LogScrollViewer == null) return;

            var point = e.GetCurrentPoint(LogScrollViewer);
            if (!point.Properties.IsMiddleButtonPressed)
            {
                _isMiddleMouseScrolling = false;
                LogScrollViewer.ReleasePointerCapture(e.Pointer);
                e.Handled = true;
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[ThemeComboBox_SelectionChanged] Selection changed");
            try
            {
                if (ThemeComboBox != null && ThemeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
                {
                    System.Diagnostics.Debug.WriteLine($"[ThemeComboBox_SelectionChanged] Selected tag: {tag}");
                    SettingsService.Instance.Theme = tag == "Dark" ? "Dark" : "Light";
                    ApplyThemeFromSettings(SettingsService.Instance.Theme);

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
                Log("[SCAN] Please select a folder first.");
                return;
            }

            if (SettingsService.Instance.AutoClearLog)
            {
                ClearLog();
            }
            try
            {
                var fileExtensions = GetFolderExtensions(_selectedFolderPath);
                if (fileExtensions.Count == 0)
                {
                    Log("[SCAN] No files found in the selected folder.");
                    return;
                }

                var selectedExtensions = await ShowScanFilterDialogAsync(fileExtensions);
                if (selectedExtensions == null || selectedExtensions.Count == 0)
                {
                    Log("[SCAN] Scan cancelled or no file types selected.");
                    return;
                }

                SetScanningState(true);
                SetScanProgressActive(true);
                UpdateScanProgress(0, 0, 0, TimeSpan.Zero, TimeSpan.Zero, "Preparing scan...");

                Log("=== [SCAN] Metadata scan started ===");
                Log($"[SCAN] Folder: {_selectedFolderPath}");
                Log($"[SCAN] Selected types: {string.Join(", ", selectedExtensions)}");
                Log("[SCAN] Reading file metadata (EXIF for images, tags for media, fallback to file system dates)...");

                Action<Or1nRenameFileNameToDateCreated.Helpers.MetadataScanService.ScanProgress> progressReporter = scanProgress =>
                {
                    var now = DateTime.UtcNow;
                    
                    // THROTTLE #1: UI progress updates (100ms cadence or on completion)
                    if (now - _lastProgressUiUpdate > TimeSpan.FromMilliseconds(PROGRESS_UI_THROTTLE_MS) || scanProgress.Index == scanProgress.Total)
                    {
                        _lastProgressUiUpdate = now;
                        UpdateScanProgress(scanProgress.Index, scanProgress.Total, scanProgress.Percent, scanProgress.Elapsed, scanProgress.EstimatedRemaining,
                            $"{scanProgress.Index}/{scanProgress.Total} ({scanProgress.Percent:0.0}%) | ETA {scanProgress.EstimatedRemaining:mm\\:ss}");
                    }

                    // THROTTLE #2: Progress logging (only log if percent changed >1% or >1 second elapsed or on completion)
                    bool shouldLogProgress = scanProgress.Index == scanProgress.Total // Always log completion
                        || Math.Abs(scanProgress.Percent - _lastProgressLogPercent) >= PROGRESS_PERCENT_THRESHOLD
                        || now - _lastProgressLogUpdate > TimeSpan.FromMilliseconds(PROGRESS_LOG_THROTTLE_MS);

                    if (shouldLogProgress)
                    {
                        _lastProgressLogUpdate = now;
                        _lastProgressLogPercent = scanProgress.Percent;
                        Log($"[SCAN] {scanProgress.Index}/{scanProgress.Total} ({scanProgress.Percent:0.0}%) | ETA {scanProgress.EstimatedRemaining:mm\\:ss} | {scanProgress.FileName}");
                    }
                };

                var scanResult = await Task.Run(() => Or1nRenameFileNameToDateCreated.Helpers.MetadataScanService.ScanFolder(_selectedFolderPath, selectedExtensions, progressReporter));

                Log($"[SCAN] Total files: {scanResult.Summary.TotalFiles}");
                LogScanSummary(scanResult.Summary);
                LogScanResults(scanResult.Files);
                LogFileTypeDateSourceSummary(scanResult.Files);

                Log("[SCAN] SUCCESS: Metadata scan complete");
                SetAction("Action: Review scan output and prepare rename rules");
            }
            catch (ArgumentException ex)
            {
                Log($"[SCAN] ERROR: {ex.Message}");
                throw;
            }
            finally
            {
                SetScanProgressActive(false);
                SetScanningState(false);
            }
        }

        private static List<string> GetFolderExtensions(string folderPath)
        {
            return System.IO.Directory.EnumerateFiles(folderPath, "*", SearchOption.TopDirectoryOnly)
                .Select(path => Path.GetExtension(path))
                .Where(ext => !string.IsNullOrWhiteSpace(ext))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(ext => ext, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<HashSet<string>?> ShowScanFilterDialogAsync(IReadOnlyList<string> extensions)
        {
            var rootGrid = new Grid
            {
                ColumnSpacing = 16
            };

            int columnCount = (int)Math.Ceiling(extensions.Count / 10.0);
            for (int i = 0; i < columnCount; i++)
            {
                rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            var checkBoxes = new List<CheckBox>();
            for (int i = 0; i < extensions.Count; i++)
            {
                int columnIndex = i / 10;
                if (rootGrid.Children.OfType<StackPanel>().FirstOrDefault(panel => Grid.GetColumn(panel) == columnIndex) is not StackPanel columnPanel)
                {
                    columnPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 8
                    };
                    Grid.SetColumn(columnPanel, columnIndex);
                    rootGrid.Children.Add(columnPanel);
                }

                var checkBox = new CheckBox
                {
                    Content = extensions[i],
                    IsChecked = true
                };

                checkBoxes.Add(checkBox);
                columnPanel.Children.Add(checkBox);
            }

            var dialog = new ContentDialog
            {
                Title = "Select file types to scan",
                Content = rootGrid,
                PrimaryButtonText = "Confirm",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot,
                RequestedTheme = this.ActualTheme
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return null;
            }

            var selected = checkBoxes
                .Where(cb => cb.IsChecked == true && cb.Content is string)
                .Select(cb => cb.Content!.ToString()!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return selected;
        }

        private void LogScanSummary(Or1nRenameFileNameToDateCreated.Helpers.MetadataScanService.ScanSummary summary)
        {
            Log("[SCAN] Summary by category:");
            foreach (var entry in summary.CategoryCounts.OrderByDescending(c => c.Value))
            {
                Log($"[SCAN]   {entry.Key}: {entry.Value}");
            }

            Log("[SCAN] Summary by date source:");
            foreach (var entry in summary.DateSourceCounts.OrderByDescending(c => c.Value))
            {
                Log($"[SCAN]   {entry.Key}: {entry.Value}");
            }

            Log("[SCAN] Top file extensions:");
            foreach (var entry in summary.ExtensionCounts.Take(10))
            {
                var extLabel = string.IsNullOrWhiteSpace(entry.Key) ? "[no extension]" : entry.Key;
                Log($"[SCAN]   {extLabel}: {entry.Value}");
            }
        }

        private void LogFileTypeDateSourceSummary(IReadOnlyList<Or1nRenameFileNameToDateCreated.Helpers.MetadataScanService.FileMetadataResult> files)
        {
            Log("[SCAN] Summary by filetype and date source:");

            var groups = files
                .GroupBy(file => file.Extension)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

            foreach (var group in groups)
            {
                var extLabel = string.IsNullOrWhiteSpace(group.Key) ? "[no extension]" : group.Key;
                var sourceCounts = group
                    .GroupBy(file => file.SelectedDateSource)
                    .OrderByDescending(sourceGroup => sourceGroup.Count())
                    .Select(sourceGroup => $"{sourceGroup.Key}: {sourceGroup.Count()}");

                Log($"[SCAN]   {extLabel} -> {string.Join(", ", sourceCounts)}");
            }
        }

        private void SetScanProgressActive(bool isActive)
        {
            if (!DispatcherQueue.HasThreadAccess)
            {
                DispatcherQueue.TryEnqueue(() => SetScanProgressActive(isActive));
                return;
            }

            if (ScanProgressPanel != null && ActionTextBlock != null)
            {
                ScanProgressPanel.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
                ActionTextBlock.Visibility = isActive ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void UpdateScanProgress(int index, int total, double percent, TimeSpan elapsed, TimeSpan eta, string statusText)
        {
            if (!DispatcherQueue.HasThreadAccess)
            {
                DispatcherQueue.TryEnqueue(() => UpdateScanProgress(index, total, percent, elapsed, eta, statusText));
                return;
            }

            if (ScanProgressBar != null)
            {
                ScanProgressBar.Maximum = 100;
                ScanProgressBar.Value = Math.Min(100, Math.Max(0, percent));
            }

            if (ScanProgressText != null)
            {
                ScanProgressText.Text = $"{statusText} | Elapsed {elapsed:mm\\:ss}";
            }
        }

        private void SetScanningState(bool isScanning)
        {
            if (!DispatcherQueue.HasThreadAccess)
            {
                DispatcherQueue.TryEnqueue(() => SetScanningState(isScanning));
                return;
            }

            _isScanning = isScanning;

            if (ScanButton != null)
            {
                ScanButton.IsEnabled = !isScanning;
            }

            this.ProtectedCursor = isScanning
                ? InputSystemCursor.Create(InputSystemCursorShape.Wait)
                : InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }

        private void LogScanResults(IReadOnlyList<Or1nRenameFileNameToDateCreated.Helpers.MetadataScanService.FileMetadataResult> files)
        {
            Log("[SCAN] Detailed file list:");
            foreach (var file in files)
            {
                var takenLabel = file.TakenDate.HasValue ? file.TakenDate.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") : "n/a";
                var taggedLabel = file.MediaTaggedDate.HasValue ? file.MediaTaggedDate.Value.ToString("yyyy-MM-dd") : "n/a";

                Log($"[SCAN] File: {file.FileName} | Ext: {file.Extension} | Type: {file.Category} | Size: {file.SizeDisplay}");
                Log($"[SCAN]   Date Selected: {file.DateParts} ({file.SelectedDateSource})");
                Log($"[SCAN]   Date Created: {file.CreatedDate:yyyy-MM-dd HH:mm:ss.fff} | Modified: {file.ModifiedDate:yyyy-MM-dd HH:mm:ss.fff}");
                Log($"[SCAN]   Date Taken: {takenLabel} | Media Tagged: {taggedLabel}");
                Log($"[SCAN]   Date Parts: Y={file.SelectedDate:yyyy} M={file.SelectedDate:MM} D={file.SelectedDate:dd} H={file.SelectedDate:HH} m={file.SelectedDate:mm} s={file.SelectedDate:ss} ms={file.SelectedDate:fff}");
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
            if (!_enableAnimations)
            {
                transform.ScaleX = toScale;
                transform.ScaleY = toScale;
                return;
            }

            var storyboard = new Storyboard();
            
            var scaleXAnimation = new DoubleAnimation
            {
                To = toScale,
                Duration = TimeSpan.FromMilliseconds(GetAnimationDurationMs(durationMs)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleXAnimation, transform);
            Storyboard.SetTargetProperty(scaleXAnimation, "ScaleX");
            storyboard.Children.Add(scaleXAnimation);

            var scaleYAnimation = new DoubleAnimation
            {
                To = toScale,
                Duration = TimeSpan.FromMilliseconds(GetAnimationDurationMs(durationMs)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleYAnimation, transform);
            Storyboard.SetTargetProperty(scaleYAnimation, "ScaleY");
            storyboard.Children.Add(scaleYAnimation);

            storyboard.Begin();
        }

        private int GetAnimationDurationMs(int baseDurationMs)
        {
            if (_animationSpeedMultiplier <= 0)
            {
                return baseDurationMs;
            }

            var adjusted = baseDurationMs / _animationSpeedMultiplier;
            return (int)Math.Max(40, Math.Min(1000, adjusted));
        }

        /// <summary>
        /// GitHub Link - Hover and Click Handlers
        /// </summary>
        private void GitHubLink_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is TextBlock link && link.RenderTransform is ScaleTransform scale)
            {
                // Subtle scale animation on hover
                AnimateScale(scale, 1.05, 100);
            }
        }

        private void GitHubLink_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is TextBlock link && link.RenderTransform is ScaleTransform scale)
            {
                // Return to normal size
                AnimateScale(scale, 1.0, 100);
            }
        }

        private async void GitHubLink_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                const string GITHUB_URL = "https://github.com/or1n/or1n-rename-file-name-to-date-created";
                Log($"Opening GitHub repository: {GITHUB_URL}");
                
                var uri = new Uri(GITHUB_URL);
                await Windows.System.Launcher.LaunchUriAsync(uri);
            }
            catch (Exception ex)
            {
                Log($"Error: Failed to open GitHub page - {ex.Message}");
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.Activate();
                Log("Settings window opened");
            }
            catch (Exception ex)
            {
                Log($"Error opening settings: {ex.Message}");
            }
        }
    }
}
