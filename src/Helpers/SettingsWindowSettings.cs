using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using Windows.Storage;

namespace Or1nRenameFileNameToDateCreated.Helpers
{
    /// <summary>
    /// Persists Settings window size and position between sessions.
    /// </summary>
    public static class SettingsWindowSettings
    {
        private const string SETTINGS_FILE = "settings-window-state.json";
        private static WindowState? _cachedSettings;
        private static string? _settingsFolderPath;

        public sealed class WindowState
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        private static string GetSettingsFolderPath()
        {
            if (_settingsFolderPath != null)
            {
                return _settingsFolderPath;
            }

            try
            {
                _settingsFolderPath = ApplicationData.Current.LocalFolder.Path;
                return _settingsFolderPath;
            }
            catch
            {
                var folderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Or1nRenameFileNameToDate"
                );
                Directory.CreateDirectory(folderPath);
                _settingsFolderPath = folderPath;
                return folderPath;
            }
        }

        public static async Task<WindowState?> LoadAsync()
        {
            try
            {
                if (_cachedSettings != null)
                {
                    return _cachedSettings;
                }

                var folderPath = GetSettingsFolderPath();
                var settingsPath = Path.Combine(folderPath, SETTINGS_FILE);
                if (!File.Exists(settingsPath))
                {
                    return null;
                }

                var content = await File.ReadAllTextAsync(settingsPath);
                _cachedSettings = JsonSerializer.Deserialize<WindowState>(content);
                return _cachedSettings;
            }
            catch
            {
                return null;
            }
        }

        public static async Task SaveAsync(int x, int y, int width, int height)
        {
            try
            {
                var state = new WindowState { X = x, Y = y, Width = width, Height = height };
                _cachedSettings = state;

                var folderPath = GetSettingsFolderPath();
                var settingsPath = Path.Combine(folderPath, SETTINGS_FILE);
                var json = JsonSerializer.Serialize(state);
                await File.WriteAllTextAsync(settingsPath, json);
            }
            catch
            {
                // Intentionally ignore save failures.
            }
        }

        public static bool IsValidPosition(WindowState state, SizeInt32 windowSize)
        {
            try
            {
                var windowCenter = new PointInt32(state.X + windowSize.Width / 2, state.Y + windowSize.Height / 2);
                var displayArea = DisplayArea.GetFromPoint(windowCenter, DisplayAreaFallback.Primary);
                var workArea = displayArea.WorkArea;

                return state.X >= workArea.X &&
                       state.Y >= workArea.Y &&
                       state.X <= workArea.X + workArea.Width &&
                       state.Y <= workArea.Y + workArea.Height;
            }
            catch
            {
                return false;
            }
        }

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
    }
}
