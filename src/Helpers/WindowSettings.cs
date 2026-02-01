using System;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Graphics;
using Microsoft.UI.Windowing;
using System.IO;

namespace Or1nRenameFileNameToDateCreated.Helpers
{
    /// <summary>
    /// Manages window position and size persistence using local app storage.
    /// </summary>
    public static class WindowSettings
    {
        private const string SETTINGS_FILE = "window-settings.json";
        private const string DEBUG_LOG = "or1n-window-debug.log";
        private static WindowState? _cachedSettings;
        private static string? _settingsFolderPath;

        /// <summary>
        /// Gets the local settings folder path, creating it if necessary.
        /// Uses environment folder as fallback for debug mode where ApplicationData might not be available.
        /// </summary>
        private static string GetSettingsFolderPath()
        {
            if (_settingsFolderPath != null)
                return _settingsFolderPath;

            try
            {
                // Try to use ApplicationData first (for packaged apps)
                var folderPath = ApplicationData.Current.LocalFolder.Path;
                _settingsFolderPath = folderPath;
                return folderPath;
            }
            catch
            {
                // Fallback to user's LocalAppData folder (works in debug mode)
                var folderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Or1nRenameFileNameToDate"
                );
                
                // Ensure folder exists
                Directory.CreateDirectory(folderPath);
                _settingsFolderPath = folderPath;
                return folderPath;
            }
        }

        private static void DebugLog(string message)
        {
            try
            {
                var folderPath = GetSettingsFolderPath();
                var logPath = Path.Combine(folderPath, DEBUG_LOG);
                File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
            catch { }
        }

        public static void ClearDebugLog()
        {
            try
            {
                var folderPath = GetSettingsFolderPath();
                var logPath = Path.Combine(folderPath, DEBUG_LOG);
                if (File.Exists(logPath))
                    File.Delete(logPath);
            }
            catch { }
        }
        public class WindowState
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string? Theme { get; set; } // "Light", "Dark", or null for system default
        }

        /// <summary>
        /// Loads saved window settings from local storage.
        /// Returns null if no saved settings exist.
        /// </summary>
        public static async Task<WindowState?> LoadAsync()
        {
            try
            {
                if (_cachedSettings != null)
                {
                    DebugLog("LoadAsync: Returning cached settings");
                    return _cachedSettings;
                }

                var folderPath = GetSettingsFolderPath();
                var settingsPath = Path.Combine(folderPath, SETTINGS_FILE);

                if (!File.Exists(settingsPath))
                {
                    DebugLog("LoadAsync: No settings file found");
                    return null;
                }

                var content = await File.ReadAllTextAsync(settingsPath);
                _cachedSettings = JsonSerializer.Deserialize<WindowState>(content);
                DebugLog($"LoadAsync: Loaded settings X={_cachedSettings?.X}, Y={_cachedSettings?.Y}, W={_cachedSettings?.Width}, H={_cachedSettings?.Height}, Theme={_cachedSettings?.Theme}");
                return _cachedSettings;
            }
            catch (Exception ex)
            {
                DebugLog($"LoadAsync: Error - {ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Saves theme preference to settings.
        /// </summary>
        public static async Task SaveThemeAsync(string theme)
        {
            try
            {
                DebugLog($"SaveThemeAsync: Saving theme={theme}");
                
                // Load existing settings or create new
                var settings = _cachedSettings ?? await LoadAsync() ?? new WindowState();
                settings.Theme = theme;
                _cachedSettings = settings;

                var folderPath = GetSettingsFolderPath();
                var settingsPath = Path.Combine(folderPath, SETTINGS_FILE);

                var json = JsonSerializer.Serialize(settings);
                await File.WriteAllTextAsync(settingsPath, json);
                
                DebugLog($"SaveThemeAsync: Successfully saved theme");
            }
            catch (Exception ex)
            {
                DebugLog($"SaveThemeAsync: Error - {ex.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves current window position and size to local storage.
        /// </summary>
        public static async Task SaveAsync(int x, int y, int width, int height)
        {
            try
            {
                DebugLog($"SaveAsync: Saving X={x}, Y={y}, W={width}, H={height}");
                
                // Preserve existing theme when saving position/size
                var existingTheme = _cachedSettings?.Theme;
                var windowState = new WindowState { X = x, Y = y, Width = width, Height = height, Theme = existingTheme };
                _cachedSettings = windowState;

                var folderPath = GetSettingsFolderPath();
                var settingsPath = Path.Combine(folderPath, SETTINGS_FILE);

                var json = JsonSerializer.Serialize(windowState);
                await File.WriteAllTextAsync(settingsPath, json);
                
                DebugLog($"SaveAsync: Successfully saved settings to {settingsPath}");
            }
            catch (Exception ex)
            {
                DebugLog($"SaveAsync: Error - {ex.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Centers a window on the current display.
        /// </summary>
        public static PointInt32 GetCenteredPosition(DisplayArea displayArea, SizeInt32 windowSize)
        {
            var workArea = displayArea.WorkArea;
            var x = workArea.X + (workArea.Width - windowSize.Width) / 2;
            var y = workArea.Y + (workArea.Height - windowSize.Height) / 2;

            return new PointInt32
            {
                X = Math.Max(workArea.X, x),
                Y = Math.Max(workArea.Y, y)
            };
        }

        /// <summary>
        /// Validates that a saved position is still valid for current display configuration.
        /// Returns true if the position intersects with any active display.
        /// </summary>
        public static bool IsValidPosition(WindowState state, SizeInt32 windowSize)
        {
            if (state == null)
                return false;

            try
            {
                // Check if the window center is on any valid display
                var centerX = state.X + state.Width / 2;
                var centerY = state.Y + state.Height / 2;

                var displayArea = DisplayArea.GetFromPoint(new Windows.Graphics.PointInt32(centerX, centerY), DisplayAreaFallback.Primary);
                return displayArea != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
