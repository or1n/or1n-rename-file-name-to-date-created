using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;

namespace Or1nRenameFileNameToDateCreated.Helpers;

/// <summary>
/// Manages application settings with real-time change notifications and JSON persistence.
/// Settings are automatically saved to local app data folder and restored on app startup.
/// </summary>
public class SettingsService : INotifyPropertyChanged
{
    private static SettingsService? _instance;
    private static readonly object _lockObject = new object();
    private bool _isInitialized;
    private DispatcherQueueTimer? _saveDebounceTimer;
    private const string SETTINGS_FILE = "app-settings.json";
    private const int SAVE_DEBOUNCE_MS = 150;

    #region Settings Properties

    private string _theme = "Light";
    private string _backdropMaterial = "DesktopAcrylic";
    private bool _enableAnimations = true;
    private string _cornerRadius = "Rounded";
    private string _fontFamily = "Segoe UI";
    private double _fontSize = 13.0;
    private double _backdropOpacity = 1.0;
    private bool _smoothScrolling = true;
    private bool _autoScrollLog = true;
    private string _logTimestampFormat = "24Hour"; // None, 24Hour, 12Hour, ISO
    private bool _debugMode = false;
    private string _logColorScheme = "Default"; // Default, HighContrast, Custom
    private string _accentColor = "SystemPrimary";
    private string _defaultFolderPath = "";
    private double _animationSpeedMultiplier = 1.0;
    private bool _alwaysOnTop = false;
    private bool _autoClearLog = false;

    public string Theme
    {
        get => _theme;
        set
        {
            if (SetProperty(ref _theme, value))
            {
                DebounceSaveSettings();
                ThemeManager.ApplyThemeToAllWindows(_theme);
            }
        }
    }

    public string BackdropMaterial
    {
        get => _backdropMaterial;
        set
        {
            if (SetProperty(ref _backdropMaterial, value))
            {
                DebounceSaveSettings();
                ThemeManager.ApplyThemeToAllWindows(Theme);
            }
        }
    }

    public bool EnableAnimations
    {
        get => _enableAnimations;
        set { if (SetProperty(ref _enableAnimations, value)) DebounceSaveSettings(); }
    }

    public string CornerRadius
    {
        get => _cornerRadius;
        set { if (SetProperty(ref _cornerRadius, value)) DebounceSaveSettings(); }
    }

    public string FontFamily
    {
        get => _fontFamily;
        set { if (SetProperty(ref _fontFamily, value)) DebounceSaveSettings(); }
    }

    public double FontSize
    {
        get => _fontSize;
        set { if (SetProperty(ref _fontSize, value)) DebounceSaveSettings(); }
    }

    public double BackdropOpacity
    {
        get => _backdropOpacity;
        set { if (SetProperty(ref _backdropOpacity, value)) DebounceSaveSettings(); }
    }

    public bool SmoothScrolling
    {
        get => _smoothScrolling;
        set { if (SetProperty(ref _smoothScrolling, value)) DebounceSaveSettings(); }
    }

    public bool AutoScrollLog
    {
        get => _autoScrollLog;
        set { if (SetProperty(ref _autoScrollLog, value)) DebounceSaveSettings(); }
    }

    public string LogTimestampFormat
    {
        get => _logTimestampFormat;
        set { if (SetProperty(ref _logTimestampFormat, value)) DebounceSaveSettings(); }
    }

    public bool DebugMode
    {
        get => _debugMode;
        set { if (SetProperty(ref _debugMode, value)) DebounceSaveSettings(); }
    }

    public string LogColorScheme
    {
        get => _logColorScheme;
        set { if (SetProperty(ref _logColorScheme, value)) DebounceSaveSettings(); }
    }

    public string AccentColor
    {
        get => _accentColor;
        set { if (SetProperty(ref _accentColor, value)) DebounceSaveSettings(); }
    }

    public string DefaultFolderPath
    {
        get => _defaultFolderPath;
        set { if (SetProperty(ref _defaultFolderPath, value)) DebounceSaveSettings(); }
    }

    public double AnimationSpeedMultiplier
    {
        get => _animationSpeedMultiplier;
        set { if (SetProperty(ref _animationSpeedMultiplier, value)) DebounceSaveSettings(); }
    }

    public bool AlwaysOnTop
    {
        get => _alwaysOnTop;
        set { if (SetProperty(ref _alwaysOnTop, value)) DebounceSaveSettings(); }
    }

    public bool AutoClearLog
    {
        get => _autoClearLog;
        set { if (SetProperty(ref _autoClearLog, value)) DebounceSaveSettings(); }
    }

    #endregion

    /// <summary>
    /// Gets the singleton instance of SettingsService.
    /// </summary>
    public static SettingsService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lockObject)
                {
                    _instance ??= new SettingsService();
                }
            }
            return _instance;
        }
    }

    private SettingsService()
    {
        // Private constructor for singleton pattern
    }

    /// <summary>
    /// Debounces calls to save settings, using a timer to batch multiple rapid changes.
    /// If called multiple times within the debounce window, previous timers are cancelled
    /// and a new one starts, ensuring saves only happen after user stops interacting.
    /// </summary>
    private void DebounceSaveSettings()
    {
        // Stop any existing timer and reset
        if (_saveDebounceTimer != null)
        {
            _saveDebounceTimer.Stop();
            _saveDebounceTimer = null;
        }

        // Create a new timer if we don't have a dispatcher yet
        var dispatcher = DispatcherQueue.GetForCurrentThread();
        if (dispatcher == null)
        {
            // Fallback: save immediately if no dispatcher available
            SaveSettingsAsync();
            return;
        }

        _saveDebounceTimer = dispatcher.CreateTimer();
        _saveDebounceTimer.Interval = TimeSpan.FromMilliseconds(SAVE_DEBOUNCE_MS);
        _saveDebounceTimer.IsRepeating = false; // Single-shot timer
        _saveDebounceTimer.Tick += (sender, args) =>
        {
            SaveSettingsAsync();
            _saveDebounceTimer = null;
        };
        _saveDebounceTimer.Start();
    }

    /// <summary>
    /// Initializes the settings service by loading saved settings from disk.
    /// Should be called once at app startup before using any settings.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            if (_isInitialized)
            {
                return;
            }

            await LoadSettingsAsync();
            _isInitialized = true;
            ThemeManager.ApplyThemeToAllWindows(_theme);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            // Use defaults if loading fails
        }
    }

    /// <summary>
    /// Loads settings from the local settings.json file.
    /// </summary>
    private async Task LoadSettingsAsync()
    {
        try
        {
            var settingsPath = GetSettingsFilePath();

            if (!File.Exists(settingsPath))
            {
                // No saved settings, use system preference
                _theme = GetSystemPreferredTheme();
                return;
            }

            var json = await File.ReadAllTextAsync(settingsPath);
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;

                var storedTheme = root.TryGetProperty("Theme", out var themeProp) ? themeProp.GetString() : null;
                _theme = storedTheme == "Dark" ? "Dark" : storedTheme == "Light" ? "Light" : GetSystemPreferredTheme();

                _backdropMaterial = root.TryGetProperty("BackdropMaterial", out var backdropProp) ? backdropProp.GetString() ?? "DesktopAcrylic" : "DesktopAcrylic";
                _enableAnimations = root.TryGetProperty("EnableAnimations", out var animProp) &&
                    (animProp.ValueKind == JsonValueKind.True || animProp.ValueKind == JsonValueKind.False)
                    ? animProp.GetBoolean()
                    : true;
                _cornerRadius = root.TryGetProperty("CornerRadius", out var cornerProp) ? cornerProp.GetString() ?? "Rounded" : "Rounded";
                _fontFamily = root.TryGetProperty("FontFamily", out var fontProp) ? fontProp.GetString() ?? "Segoe UI" : "Segoe UI";
                _fontSize = root.TryGetProperty("FontSize", out var fontSizeProp) && fontSizeProp.ValueKind == JsonValueKind.Number ? fontSizeProp.GetDouble() : 13.0;
                _backdropOpacity = root.TryGetProperty("BackdropOpacity", out var opacityProp) && opacityProp.ValueKind == JsonValueKind.Number ? opacityProp.GetDouble() : 1.0;
                _smoothScrolling = root.TryGetProperty("SmoothScrolling", out var smoothProp) &&
                    (smoothProp.ValueKind == JsonValueKind.True || smoothProp.ValueKind == JsonValueKind.False)
                    ? smoothProp.GetBoolean()
                    : true;
                _autoScrollLog = root.TryGetProperty("AutoScrollLog", out var autoScrollProp) &&
                    (autoScrollProp.ValueKind == JsonValueKind.True || autoScrollProp.ValueKind == JsonValueKind.False)
                    ? autoScrollProp.GetBoolean()
                    : true;
                _logTimestampFormat = root.TryGetProperty("LogTimestampFormat", out var logFmtProp) ? logFmtProp.GetString() ?? "24Hour" : "24Hour";
                _debugMode = root.TryGetProperty("DebugMode", out var debugProp) &&
                    (debugProp.ValueKind == JsonValueKind.True || debugProp.ValueKind == JsonValueKind.False)
                    ? debugProp.GetBoolean()
                    : false;
                _logColorScheme = root.TryGetProperty("LogColorScheme", out var logSchemeProp) ? logSchemeProp.GetString() ?? "Default" : "Default";
                _accentColor = root.TryGetProperty("AccentColor", out var accentProp) ? accentProp.GetString() ?? "SystemPrimary" : "SystemPrimary";
                _defaultFolderPath = root.TryGetProperty("DefaultFolderPath", out var pathProp) ? pathProp.GetString() ?? "" : "";
                _animationSpeedMultiplier = root.TryGetProperty("AnimationSpeedMultiplier", out var speedProp) && speedProp.ValueKind == JsonValueKind.Number ? speedProp.GetDouble() : 1.0;
                _alwaysOnTop = root.TryGetProperty("AlwaysOnTop", out var alwaysProp) &&
                    (alwaysProp.ValueKind == JsonValueKind.True || alwaysProp.ValueKind == JsonValueKind.False)
                    ? alwaysProp.GetBoolean()
                    : false;
                _autoClearLog = root.TryGetProperty("AutoClearLog", out var clearProp) &&
                    (clearProp.ValueKind == JsonValueKind.True || clearProp.ValueKind == JsonValueKind.False)
                    ? clearProp.GetBoolean()
                    : false;

                OnPropertyChanged(nameof(Theme));
                OnPropertyChanged(nameof(BackdropMaterial));
                OnPropertyChanged(nameof(AccentColor));
                OnPropertyChanged(nameof(FontFamily));
                OnPropertyChanged(nameof(FontSize));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            // Use defaults on error
        }
    }

    /// <summary>
    /// Saves all current settings to the local settings.json file.
    /// </summary>
    private async void SaveSettingsAsync()
    {
        try
        {
            var settingsPath = GetSettingsFilePath();

            var settings = new
            {
                Theme = _theme,
                BackdropMaterial = _backdropMaterial,
                EnableAnimations = _enableAnimations,
                CornerRadius = _cornerRadius,
                FontFamily = _fontFamily,
                FontSize = _fontSize,
                BackdropOpacity = _backdropOpacity,
                SmoothScrolling = _smoothScrolling,
                AutoScrollLog = _autoScrollLog,
                LogTimestampFormat = _logTimestampFormat,
                DebugMode = _debugMode,
                LogColorScheme = _logColorScheme,
                AccentColor = _accentColor,
                DefaultFolderPath = _defaultFolderPath,
                AnimationSpeedMultiplier = _animationSpeedMultiplier,
                AlwaysOnTop = _alwaysOnTop,
                AutoClearLog = _autoClearLog
            };

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(settingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    private static string GetSettingsFilePath()
    {
        try
        {
            var folderPath = ApplicationData.Current.LocalFolder.Path;
            Directory.CreateDirectory(folderPath);
            return Path.Combine(folderPath, SETTINGS_FILE);
        }
        catch
        {
            var folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Or1nRenameFileNameToDate"
            );
            Directory.CreateDirectory(folderPath);
            return Path.Combine(folderPath, SETTINGS_FILE);
        }
    }

    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        Theme = GetSystemPreferredTheme();
        BackdropMaterial = "DesktopAcrylic";
        EnableAnimations = true;
        CornerRadius = "Rounded";
        FontFamily = "Segoe UI";
        FontSize = 13.0;
        BackdropOpacity = 1.0;
        SmoothScrolling = true;
        AutoScrollLog = true;
        LogTimestampFormat = "24Hour";
        DebugMode = false;
        LogColorScheme = "Default";
        AccentColor = "SystemPrimary";
        DefaultFolderPath = "";
        AnimationSpeedMultiplier = 1.0;
        AlwaysOnTop = false;
        AutoClearLog = false;
    }

    private static string GetSystemPreferredTheme()
    {
        return Application.Current.RequestedTheme == ApplicationTheme.Dark
            ? "Dark"
            : "Light";
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T backingField, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value))
            return false;

        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
