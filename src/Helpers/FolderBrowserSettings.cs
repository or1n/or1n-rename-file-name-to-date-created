using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Graphics;
using Microsoft.UI.Windowing;

namespace Or1nRenameFileNameToDateCreated.Helpers
{
    /// <summary>
    /// Manages folder browser window state (position, size, last path) persistence.
    /// </summary>
    public static class FolderBrowserSettings
    {
        private const string SETTINGS_FILE = "folder-browser-settings.json";
        private static FolderBrowserState? _cachedSettings;
        private static string? _settingsFolderPath;

        public class FolderBrowserState
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string? LastPath { get; set; }
        }

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

        /// <summary>
        /// Loads saved folder browser settings from local storage.
        /// Returns null if no saved settings exist.
        /// </summary>
        public static async Task<FolderBrowserState?> LoadAsync()
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
                _cachedSettings = JsonSerializer.Deserialize<FolderBrowserState>(content);
                return _cachedSettings;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Saves current folder browser position, size, and last path to local storage.
        /// </summary>
        public static async Task SaveAsync(int x, int y, int width, int height, string? lastPath)
        {
            try
            {
                var state = new FolderBrowserState 
                { 
                    X = x, 
                    Y = y, 
                    Width = width, 
                    Height = height,
                    LastPath = lastPath
                };
                _cachedSettings = state;

                var folderPath = GetSettingsFolderPath();
                var settingsPath = Path.Combine(folderPath, SETTINGS_FILE);

                var json = JsonSerializer.Serialize(state);
                await File.WriteAllTextAsync(settingsPath, json);
            }
            catch
            {
                // Silently fail if we can't save
            }
        }

        /// <summary>
        /// Validates that a saved position is still valid for current display configuration.
        /// </summary>
        public static bool IsValidPosition(FolderBrowserState state, SizeInt32 windowSize)
        {
            if (state == null)
                return false;

            try
            {
                // Check if the window center is on any valid display
                var centerX = state.X + state.Width / 2;
                var centerY = state.Y + state.Height / 2;

                var displayArea = DisplayArea.GetFromPoint(new PointInt32(centerX, centerY), DisplayAreaFallback.Primary);
                return displayArea != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
