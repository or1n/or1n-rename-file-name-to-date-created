using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Dispatching;
using Or1nRenameFileNameToDateCreated.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;

namespace Or1nRenameFileNameToDateCreated.Views
{
    public sealed partial class SettingsWindow : Window
    {
        private const int MIN_WIDTH = 640;
        private const int MIN_HEIGHT = 700;
        private bool _isInitializing = true;
        private bool _restoredState = false;
        private DispatcherQueueTimer? _themeToastTimer;

        public SettingsWindow()
        {
#pragma warning disable CS0103
            this.InitializeComponent();
#pragma warning restore CS0103

            Title = "Settings";

            var appWindow = AppWindow;
            appWindow.Resize(new SizeInt32(820, 900));

            appWindow.Changed += AppWindow_Changed;
            Closed += SettingsWindow_Closed;
            Activated += SettingsWindow_Activated;

            WindowHelper.ActiveWindows.Add(this);
            Closed += (s, e) => WindowHelper.ActiveWindows.Remove(this);


            if (Content is FrameworkElement root)
            {
                root.ActualThemeChanged += Root_ActualThemeChanged;
                UpdateTitleBarTheme(root.ActualTheme);
            }

            ApplyBackdrop(SettingsService.Instance.BackdropMaterial);
            InitializeBindings();
            WireUpEventHandlers();
            ApplyAllSettings();
        }

        private async void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (_isInitializing)
            {
                return;
            }

            if (args.DidSizeChange)
            {
                EnforceMinimumSize(sender);
            }

            if (args.DidSizeChange || args.DidPositionChange)
            {
                await SaveWindowStateAsync();
            }
        }

        private async void SettingsWindow_Closed(object sender, WindowEventArgs args)
        {
            await SaveWindowStateAsync();
        }

        private void SettingsWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (_restoredState)
            {
                return;
            }

            _restoredState = true;
            _ = RestoreWindowStateAsync();
        }

        private async Task RestoreWindowStateAsync()
        {
            _isInitializing = true;

            try
            {
                var appWindow = AppWindow;
                var savedState = await SettingsWindowSettings.LoadAsync();

                if (savedState != null && savedState.Width > 0 && savedState.Height > 0)
                {
                    appWindow.Resize(new SizeInt32
                    {
                        Width = Math.Max(MIN_WIDTH, savedState.Width),
                        Height = Math.Max(MIN_HEIGHT, savedState.Height)
                    });

                    if (SettingsWindowSettings.IsValidPosition(savedState, appWindow.Size))
                    {
                        appWindow.Move(new PointInt32(savedState.X, savedState.Y));
                    }
                    else
                    {
                        CenterWindow(appWindow);
                    }
                }
                else
                {
                    CenterWindow(appWindow);
                }
            }
            catch
            {
                CenterWindow(AppWindow);
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private async Task SaveWindowStateAsync()
        {
            var appWindow = AppWindow;
            if (appWindow == null)
            {
                return;
            }

            var position = appWindow.Position;
            var size = appWindow.Size;
            await SettingsWindowSettings.SaveAsync(position.X, position.Y, size.Width, size.Height);
        }

        private static void EnforceMinimumSize(AppWindow appWindow)
        {
            var size = appWindow.Size;
            var width = Math.Max(MIN_WIDTH, size.Width);
            var height = Math.Max(MIN_HEIGHT, size.Height);

            if (width == size.Width && height == size.Height)
            {
                return;
            }

            appWindow.Resize(new SizeInt32(width, height));
        }

        private static void CenterWindow(AppWindow appWindow)
        {
            var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            var centered = SettingsWindowSettings.GetCenteredPosition(displayArea, appWindow.Size);
            appWindow.Move(centered);
        }

        private void Root_ActualThemeChanged(FrameworkElement sender, object args)
        {
            UpdateTitleBarTheme(sender.ActualTheme);
            ApplyBackdropOpacity(SettingsService.Instance.BackdropOpacity);
        }

        public void UpdateTitleBarTheme(ElementTheme theme)
        {
            var appWindow = AppWindow;
            if (appWindow?.TitleBar == null)
            {
                return;
            }

            var titleBar = appWindow.TitleBar;
            bool isDark = theme == ElementTheme.Dark;
            
            if (isDark)
            {
                titleBar.BackgroundColor = Color.FromArgb(255, 32, 32, 32);
                titleBar.ForegroundColor = Color.FromArgb(255, 255, 255, 255);
                titleBar.InactiveBackgroundColor = Color.FromArgb(255, 32, 32, 32);
                titleBar.InactiveForegroundColor = Color.FromArgb(255, 138, 138, 138);
                
                titleBar.ButtonBackgroundColor = Color.FromArgb(255, 32, 32, 32);
                titleBar.ButtonForegroundColor = Color.FromArgb(255, 255, 255, 255);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 60, 60, 60);
                titleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 255, 255, 255);
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 80, 80, 80);
                titleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 255, 255, 255);
                
                titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 32, 32, 32);
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 138, 138, 138);
            }
            else
            {
                titleBar.BackgroundColor = Color.FromArgb(255, 243, 243, 243);
                titleBar.ForegroundColor = Color.FromArgb(255, 0, 0, 0);
                titleBar.InactiveBackgroundColor = Color.FromArgb(255, 243, 243, 243);
                titleBar.InactiveForegroundColor = Color.FromArgb(255, 115, 115, 115);
                
                titleBar.ButtonBackgroundColor = Color.FromArgb(255, 243, 243, 243);
                titleBar.ButtonForegroundColor = Color.FromArgb(255, 0, 0, 0);
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 230, 230, 230);
                titleBar.ButtonHoverForegroundColor = Color.FromArgb(255, 0, 0, 0);
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 210, 210, 210);
                titleBar.ButtonPressedForegroundColor = Color.FromArgb(255, 0, 0, 0);
                
                titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 243, 243, 243);
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 115, 115, 115);
            }
        }

        private void ApplyAllSettings()
        {
            var settings = SettingsService.Instance;

            ApplyThemeToWindows(settings.Theme);
            ApplyBackdrop(settings.BackdropMaterial);
            ApplyCornerRadius(settings.CornerRadius);
            ApplyFontToWindows(settings.FontFamily);
            ApplyFontSizeToWindows(settings.FontSize);
            ApplyAccentColorToApp(settings.AccentColor);
            ApplyAlwaysOnTopToMainWindow(settings.AlwaysOnTop);
            ApplyLogColorSchemeToApp(settings.LogColorScheme);
            ApplyBackdropOpacity(settings.BackdropOpacity);
        }

        private void InitializeBindings()
        {
            var settings = SettingsService.Instance;

            ThemeComboBox.SelectedIndex = settings.Theme == "Dark" ? 1 : 0;

            BackdropMaterialComboBox.SelectedIndex = settings.BackdropMaterial switch
            {
                "None" => 0,
                "DesktopAcrylic" => 1,
                "Mica" => 2,
                "MicaAlt" => 3,
                _ => 1
            };

            CornerRadiusComboBox.SelectedIndex = settings.CornerRadius switch
            {
                "Rounded" => 1,
                "VeryRounded" => 2,
                _ => 0
            };

            FontFamilyComboBox.SelectedIndex = settings.FontFamily switch
            {
                "Consolas" => 1,
                "Cascadia Code" => 2,
                "Courier New" => 3,
                "Georgia" => 4,
                _ => 0
            };

            FontSizeSlider.Value = settings.FontSize;
            UpdateFontSizeDisplay();

            BackdropOpacitySlider.Value = settings.BackdropOpacity * 100;
            UpdateOpacityDisplay();

            AnimationSpeedSlider.Value = settings.AnimationSpeedMultiplier;
            UpdateAnimationSpeedDisplay();

            AccentColorComboBox.SelectedIndex = settings.AccentColor switch
            {
                "Blue" => 1,
                "Purple" => 2,
                "Pink" => 3,
                "Orange" => 4,
                "Green" => 5,
                _ => 0
            };

            EnableAnimationsToggle.IsOn = settings.EnableAnimations;
            SmoothScrollingToggle.IsOn = settings.SmoothScrolling;
            AutoScrollLogToggle.IsOn = settings.AutoScrollLog;

            LogTimestampFormatComboBox.SelectedIndex = settings.LogTimestampFormat switch
            {
                "None" => 0,
                "24Hour" => 1,
                "12Hour" => 2,
                _ => 3
            };

            DefaultFolderTextBox.Text = string.IsNullOrWhiteSpace(settings.DefaultFolderPath)
                ? "No default folder set"
                : settings.DefaultFolderPath;

            AlwaysOnTopToggle.IsOn = settings.AlwaysOnTop;
            AutoClearLogToggle.IsOn = settings.AutoClearLog;
            DebugModeToggle.IsOn = settings.DebugMode;

            LogColorSchemeComboBox.SelectedIndex = settings.LogColorScheme switch
            {
                "HighContrast" => 1,
                _ => 0
            };
        }

        private void WireUpEventHandlers()
        {
            var settings = SettingsService.Instance;

            ThemeComboBox.SelectionChanged += (s, e) =>
            {
                if (_isInitializing) { return; }
                
                var newTheme = ThemeComboBox.SelectedIndex == 1 ? "Dark" : "Light";
                settings.Theme = newTheme;
                
                // Show theme feedback toast
                ShowThemeFeedback(newTheme);
            };

            BackdropMaterialComboBox.SelectionChanged += (s, e) =>
            {
                settings.BackdropMaterial = BackdropMaterialComboBox.SelectedIndex switch
                {
                    0 => "None",
                    1 => "DesktopAcrylic",
                    2 => "Mica",
                    3 => "MicaAlt",
                    _ => "DesktopAcrylic"
                };
                ApplyBackdrop(settings.BackdropMaterial);
            };

            CornerRadiusComboBox.SelectionChanged += (s, e) =>
            {
                settings.CornerRadius = CornerRadiusComboBox.SelectedIndex switch
                {
                    1 => "Rounded",
                    2 => "VeryRounded",
                    _ => "Sharp"
                };
                ApplyCornerRadius(settings.CornerRadius);
            };

            FontFamilyComboBox.SelectionChanged += (s, e) =>
            {
                var fonts = new[] { "Segoe UI", "Consolas", "Cascadia Code", "Courier New", "Georgia" };
                if (FontFamilyComboBox.SelectedIndex >= 0 && FontFamilyComboBox.SelectedIndex < fonts.Length)
                {
                    settings.FontFamily = fonts[FontFamilyComboBox.SelectedIndex];
                    ApplyFontToWindows(settings.FontFamily);
                }
            };

            FontSizeSlider.ValueChanged += (s, e) =>
            {
                settings.FontSize = FontSizeSlider.Value;
                UpdateFontSizeDisplay();
                ApplyFontSizeToWindows(settings.FontSize);
            };

            BackdropOpacitySlider.ValueChanged += (s, e) =>
            {
                settings.BackdropOpacity = BackdropOpacitySlider.Value / 100.0;
                UpdateOpacityDisplay();
                ApplyBackdropOpacity(settings.BackdropOpacity);
            };

            AnimationSpeedSlider.ValueChanged += (s, e) =>
            {
                settings.AnimationSpeedMultiplier = AnimationSpeedSlider.Value;
                UpdateAnimationSpeedDisplay();
            };

            AccentColorComboBox.SelectionChanged += (s, e) =>
            {
                var colors = new[] { "SystemPrimary", "Blue", "Purple", "Pink", "Orange", "Green" };
                if (AccentColorComboBox.SelectedIndex >= 0 && AccentColorComboBox.SelectedIndex < colors.Length)
                {
                    settings.AccentColor = colors[AccentColorComboBox.SelectedIndex];
                    ApplyAccentColorToApp(settings.AccentColor);
                }
            };

            EnableAnimationsToggle.Toggled += (s, e) => settings.EnableAnimations = EnableAnimationsToggle.IsOn;
            SmoothScrollingToggle.Toggled += (s, e) => settings.SmoothScrolling = SmoothScrollingToggle.IsOn;
            AutoScrollLogToggle.Toggled += (s, e) => settings.AutoScrollLog = AutoScrollLogToggle.IsOn;

            LogTimestampFormatComboBox.SelectionChanged += (s, e) =>
            {
                settings.LogTimestampFormat = LogTimestampFormatComboBox.SelectedIndex switch
                {
                    0 => "None",
                    1 => "24Hour",
                    2 => "12Hour",
                    _ => "ISO"
                };
            };

            BrowseFolderButton.Click += async (s, e) => await BrowseFolderAsync();

            AlwaysOnTopToggle.Toggled += (s, e) =>
            {
                settings.AlwaysOnTop = AlwaysOnTopToggle.IsOn;
                ApplyAlwaysOnTopToMainWindow(settings.AlwaysOnTop);
            };

            AutoClearLogToggle.Toggled += (s, e) => settings.AutoClearLog = AutoClearLogToggle.IsOn;
            DebugModeToggle.Toggled += (s, e) => settings.DebugMode = DebugModeToggle.IsOn;

            LogColorSchemeComboBox.SelectionChanged += (s, e) =>
            {
                settings.LogColorScheme = LogColorSchemeComboBox.SelectedIndex == 1 ? "HighContrast" : "Default";
                ApplyLogColorSchemeToApp(settings.LogColorScheme);
            };

            ClearLogButton.Click += (s, e) => ClearLogInMainPage();
            ExportSettingsButton.Click += async (s, e) => await ExportSettingsAsync();
            ImportSettingsButton.Click += async (s, e) => await ImportSettingsAsync();
            ResetDefaultsButton.Click += async (s, e) => await ResetToDefaultsAsync();
        }

        private void UpdateFontSizeDisplay()
        {
            FontSizeDisplay.Text = $"{FontSizeSlider.Value:F0}px";
        }

        private void UpdateOpacityDisplay()
        {
            OpacityDisplay.Text = $"{BackdropOpacitySlider.Value:F0}%";
        }

        private void UpdateAnimationSpeedDisplay()
        {
            AnimationSpeedDisplay.Text = $"{AnimationSpeedSlider.Value:F1}x";
        }

        private void ApplyThemeToWindows(string theme)
        {
            ThemeManager.ApplyThemeToAllWindows(theme);
        }

        private void ApplyBackdrop(string material)
        {
            foreach (var window in WindowHelper.ActiveWindows)
            {
                ApplyBackdropToWindow(window, material);
            }
        }

        private void ApplyBackdropToWindow(Window window, string material)
        {
            try
            {
                if (material == "None")
                {
                    window.SystemBackdrop = null;
                    return;
                }

                if (material == "DesktopAcrylic")
                {
                    window.SystemBackdrop = new DesktopAcrylicBackdrop();
                    return;
                }

                if (MicaController.IsSupported())
                {
                    window.SystemBackdrop = new MicaBackdrop
                    {
                        Kind = material == "MicaAlt" ? MicaKind.BaseAlt : MicaKind.Base
                    };
                    return;
                }

                window.SystemBackdrop = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Backdrop error: {ex.Message}");
            }
        }

        private void ApplyCornerRadius(string radiusSetting)
        {
            var radiusValue = radiusSetting switch
            {
                "Rounded" => 4,
                "VeryRounded" => 8,
                _ => 0
            };

            var cornerRadius = new CornerRadius(radiusValue);
            var resources = Application.Current.Resources;
            resources["ControlCornerRadius"] = cornerRadius;
            resources["OverlayCornerRadius"] = cornerRadius;
            resources["ContentDialogCornerRadius"] = cornerRadius;
        }

        private void ApplyFontToWindows(string fontFamily)
        {
            var resources = Application.Current.Resources;
            var family = new FontFamily(fontFamily);
            resources["ContentControlThemeFontFamily"] = family;
            resources["TextBlockThemeFontFamily"] = family;

            foreach (var window in WindowHelper.ActiveWindows)
            {
                if (window.Content is FrameworkElement root)
                {
                    ApplyFontFamilyToTree(root, family);
                }
            }
        }

        private void ApplyFontSizeToWindows(double fontSize)
        {
            var resources = Application.Current.Resources;
            resources["ContentControlThemeFontSize"] = fontSize;
            resources["TextBlockThemeFontSize"] = fontSize;

            foreach (var window in WindowHelper.ActiveWindows)
            {
                if (window.Content is FrameworkElement root)
                {
                    ApplyFontSizeToTree(root, fontSize);
                }
            }
        }

        private static void ApplyFontFamilyToTree(DependencyObject root, FontFamily fontFamily)
        {
            if (root is Control control && control.ReadLocalValue(Control.FontFamilyProperty) == DependencyProperty.UnsetValue)
            {
                control.FontFamily = fontFamily;
            }

            if (root is TextBlock textBlock && textBlock.ReadLocalValue(TextBlock.FontFamilyProperty) == DependencyProperty.UnsetValue)
            {
                textBlock.FontFamily = fontFamily;
            }

            var childrenCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childrenCount; i++)
            {
                ApplyFontFamilyToTree(VisualTreeHelper.GetChild(root, i), fontFamily);
            }
        }

        private static void ApplyFontSizeToTree(DependencyObject root, double fontSize)
        {
            if (root is Control control && control.ReadLocalValue(Control.FontSizeProperty) == DependencyProperty.UnsetValue)
            {
                control.FontSize = fontSize;
            }

            if (root is TextBlock textBlock && textBlock.ReadLocalValue(TextBlock.FontSizeProperty) == DependencyProperty.UnsetValue)
            {
                textBlock.FontSize = fontSize;
            }

            var childrenCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childrenCount; i++)
            {
                ApplyFontSizeToTree(VisualTreeHelper.GetChild(root, i), fontSize);
            }
        }

        private void ApplyAccentColorToApp(string accent)
        {
            var color = accent switch
            {
                "Blue" => Colors.DeepSkyBlue,
                "Purple" => Colors.MediumPurple,
                "Pink" => Colors.HotPink,
                "Orange" => Colors.DarkOrange,
                "Green" => Colors.SeaGreen,
                _ => TryGetResourceColor("AccentColor", Colors.DeepSkyBlue)
            };

            if (Application.Current.Resources["AccentBrush"] is SolidColorBrush accentBrush)
            {
                accentBrush.Color = color;
            }

            Application.Current.Resources["AccentColor"] = color;
        }

        private void ApplyBackdropOpacity(double opacity)
        {
            var clamped = Math.Max(0, Math.Min(1, opacity));
            UpdateBrushOpacity("PageBackgroundBrush", clamped);
            UpdateBrushOpacity("SurfaceBrush", clamped);
            UpdateBrushOpacity("Surface2Brush", clamped);
            UpdateBrushOpacity("Surface3Brush", clamped);
        }

        private void ApplyAlwaysOnTopToMainWindow(bool isAlwaysOnTop)
        {
            var mainWindow = WindowHelper.ActiveWindows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow?.AppWindow?.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsAlwaysOnTop = isAlwaysOnTop;
            }
        }

        private void ApplyLogColorSchemeToApp(string scheme)
        {
            if (scheme == "HighContrast")
            {
                Application.Current.Resources["LogInfoBrush"] = new SolidColorBrush(Colors.White);
                Application.Current.Resources["LogWarningBrush"] = new SolidColorBrush(Colors.Yellow);
                Application.Current.Resources["LogErrorBrush"] = new SolidColorBrush(Colors.Red);
                Application.Current.Resources["LogSuccessBrush"] = new SolidColorBrush(Colors.Lime);
                Application.Current.Resources["LogDebugBrush"] = new SolidColorBrush(Colors.White);
                return;
            }

            ClearLogOverrides();
        }

        private static void ClearLogOverrides()
        {
            Application.Current.Resources.Remove("LogInfoBrush");
            Application.Current.Resources.Remove("LogWarningBrush");
            Application.Current.Resources.Remove("LogErrorBrush");
            Application.Current.Resources.Remove("LogSuccessBrush");
            Application.Current.Resources.Remove("LogDebugBrush");
        }

        private void ClearLogInMainPage()
        {
            var mainWindow = WindowHelper.ActiveWindows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow?.Content is Frame frame && frame.Content is MainPage mainPage)
            {
                mainPage.ClearLog();
            }
        }

        private async Task BrowseFolderAsync()
        {
            try
            {
                var picker = new FolderPicker();
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeFilter.Add("*");

                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);

                var folder = await picker.PickSingleFolderAsync();
                if (folder != null)
                {
                    SettingsService.Instance.DefaultFolderPath = folder.Path;
                    DefaultFolderTextBox.Text = folder.Path;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Folder picker error: {ex.Message}");
            }
        }

        private async Task ExportSettingsAsync()
        {
            try
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("JSON Settings", new[] { ".json" });
                savePicker.SuggestedFileName = $"or1n-settings-{DateTime.Now:yyyyMMdd-HHmmss}.json";

                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    var settings = SettingsService.Instance;
                    var settingsJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        settings.Theme,
                        settings.BackdropMaterial,
                        settings.EnableAnimations,
                        settings.CornerRadius,
                        settings.FontFamily,
                        settings.FontSize,
                        settings.BackdropOpacity,
                        settings.SmoothScrolling,
                        settings.AutoScrollLog,
                        settings.LogTimestampFormat,
                        settings.DebugMode,
                        settings.LogColorScheme,
                        settings.AccentColor,
                        settings.DefaultFolderPath,
                        settings.AnimationSpeedMultiplier,
                        settings.AlwaysOnTop,
                        settings.AutoClearLog,
                        ExportDate = DateTime.Now
                    }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                    await FileIO.WriteTextAsync(file, settingsJson);

                    var dialog = new ContentDialog
                    {
                        Title = "Export Successful",
                        Content = $"Settings exported to:\n{file.Name}",
                        CloseButtonText = "OK",
                        XamlRoot = Content.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting settings: {ex.Message}");
            }
        }

        private async Task ImportSettingsAsync()
        {
            try
            {
                var openPicker = new FileOpenPicker();
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                openPicker.FileTypeFilter.Add(".json");

                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

                var file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    var json = await FileIO.ReadTextAsync(file);
                    var imported = System.Text.Json.JsonSerializer.Deserialize<SettingsImportModel>(json);
                    if (imported != null)
                    {
                        var settings = SettingsService.Instance;
                        settings.Theme = imported.Theme;
                        settings.BackdropMaterial = imported.BackdropMaterial;
                        settings.EnableAnimations = imported.EnableAnimations;
                        settings.CornerRadius = imported.CornerRadius;
                        settings.FontFamily = imported.FontFamily;
                        settings.FontSize = imported.FontSize;
                        settings.BackdropOpacity = imported.BackdropOpacity;
                        settings.SmoothScrolling = imported.SmoothScrolling;
                        settings.AutoScrollLog = imported.AutoScrollLog;
                        settings.LogTimestampFormat = imported.LogTimestampFormat;
                        settings.DebugMode = imported.DebugMode;
                        settings.LogColorScheme = imported.LogColorScheme;
                        settings.AccentColor = imported.AccentColor;
                        settings.DefaultFolderPath = imported.DefaultFolderPath;
                        settings.AnimationSpeedMultiplier = imported.AnimationSpeedMultiplier;
                        settings.AlwaysOnTop = imported.AlwaysOnTop;
                        settings.AutoClearLog = imported.AutoClearLog;

                        InitializeBindings();
                        ApplyAllSettings();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing settings: {ex.Message}");
            }
        }

        private async Task ResetToDefaultsAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "Reset All Settings?",
                Content = "This action cannot be undone. All settings will be restored to their default values.",
                PrimaryButtonText = "Reset",
                CloseButtonText = "Cancel",
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SettingsService.Instance.ResetToDefaults();
                InitializeBindings();
                ApplyAllSettings();
            }
        }

        private static Color TryGetResourceColor(string key, Color fallback)
        {
            return Application.Current.Resources.TryGetValue(key, out var value) && value is Color color
                ? color
                : fallback;
        }


        private static void UpdateBrushOpacity(string key, double opacity)
        {
            UpdateBrushOpacityInDictionary(Application.Current.Resources, key, opacity);

            if (Application.Current.Resources.ThemeDictionaries.TryGetValue("Light", out var lightDictObj) &&
                lightDictObj is ResourceDictionary lightDict)
            {
                UpdateBrushOpacityInDictionary(lightDict, key, opacity);
            }

            if (Application.Current.Resources.ThemeDictionaries.TryGetValue("Dark", out var darkDictObj) &&
                darkDictObj is ResourceDictionary darkDict)
            {
                UpdateBrushOpacityInDictionary(darkDict, key, opacity);
            }
        }

        /// <summary>
        /// Shows a temporary feedback toast when theme is changed.
        /// Auto-hides after 2.5 seconds with smooth animation.
        /// </summary>
        private void ShowThemeFeedback(string themeName)
        {
            if (ThemeFeedbackInfoBar == null) { return; }

            // Cancel any existing timer
            if (_themeToastTimer != null)
            {
                _themeToastTimer.Stop();
                _themeToastTimer = null;
            }

            // Update message and show
            ThemeFeedbackInfoBar.Message = $"{themeName} theme applied successfully";
            ThemeFeedbackInfoBar.IsOpen = true;

            // Create auto-hide timer (2.5 seconds)
            _themeToastTimer = DispatcherQueue.CreateTimer();
            _themeToastTimer.Interval = TimeSpan.FromMilliseconds(2500);
            _themeToastTimer.Tick += (s, e) =>
            {
                ThemeFeedbackInfoBar.IsOpen = false;
                _themeToastTimer?.Stop();
                _themeToastTimer = null;
            };
            _themeToastTimer.Start();
        }

        private static void UpdateBrushOpacityInDictionary(ResourceDictionary dictionary, string key, double opacity)
        {
            if (dictionary.TryGetValue(key, out var value) && value is SolidColorBrush brush)
            {
                brush.Opacity = opacity;
            }
        }

        private sealed class SettingsImportModel
        {
            public string Theme { get; set; } = "System";
            public string BackdropMaterial { get; set; } = "DesktopAcrylic";
            public bool EnableAnimations { get; set; }
            public string CornerRadius { get; set; } = "Rounded";
            public string FontFamily { get; set; } = "Segoe UI";
            public double FontSize { get; set; } = 13;
            public double BackdropOpacity { get; set; } = 1.0;
            public bool SmoothScrolling { get; set; }
            public bool AutoScrollLog { get; set; }
            public string LogTimestampFormat { get; set; } = "24Hour";
            public bool DebugMode { get; set; }
            public string LogColorScheme { get; set; } = "Default";
            public string AccentColor { get; set; } = "SystemPrimary";
            public string DefaultFolderPath { get; set; } = string.Empty;
            public double AnimationSpeedMultiplier { get; set; } = 1.0;
            public bool AlwaysOnTop { get; set; }
            public bool AutoClearLog { get; set; }
        }


    }
}
