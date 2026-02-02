using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Or1nRenameFileNameToDateCreated;
using Or1nRenameFileNameToDateCreated.Helpers;
using Windows.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;

namespace Or1nRenameFileNameToDateCreated.Views
{
    public sealed partial class FolderBrowserDialog : Window
    {
        public string SelectedPath { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public ObservableCollection<string> FolderItems { get; private set; }
        public bool DialogResult { get; private set; }

        private const int MIN_WIDTH = 500;
        private const int MIN_HEIGHT = 400;
        private string currentPath = "";
        private ListView folderListView = null!;
        private TextBlock currentPathText = null!;
        private TextBlock infoText = null!;
        private TextBox pathEditBox = null!;
        private TaskCompletionSource<bool> _taskCompletionSource = null!;
        private bool _isInitializing = true;
        private bool _isLoaded = false;

        public FolderBrowserDialog()
        {
            FolderItems = new ObservableCollection<string>();
            
            // Initialize with default path - will be overridden in ShowAsync if saved path exists
            currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            
            BuildUI();
            LoadFolders();
            
            this.Title = "Select Folder";
            
            // Match main window's backdrop style (DesktopAcrylic like main window)
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();

            WindowHelper.ActiveWindows.Add(this);
            this.Closed += (s, e) =>
            {
                WindowHelper.ActiveWindows.Remove(this);
                SettingsService.Instance.PropertyChanged -= SettingsService_PropertyChanged;
            };

            SettingsService.Instance.PropertyChanged += SettingsService_PropertyChanged;
            
            // Match the main window's theme
            var mainWindow = WindowHelper.ActiveWindows
                .Find(w => w is MainWindow) as MainWindow;
            if (mainWindow != null && this.Content is FrameworkElement content)
            {
                // Get the RootFrame from MainWindow to match its theme
                if (mainWindow.Content is FrameworkElement mainContent)
                {
                    content.RequestedTheme = mainContent.ActualTheme;
                }
            }

            ApplyThemeFromSettings();
            
            var appWindow = this.AppWindow;
            appWindow.Resize(new SizeInt32 { Width = 800, Height = 600 });
            
            // Setup title bar styling
            SetupTitleBar();
            
            // Listen for theme changes
            if (this.Content is FrameworkElement element)
            {
                element.ActualThemeChanged += Content_ActualThemeChanged;
            }
            
            // Track position/size changes
            appWindow.Changed += AppWindow_Changed;
            
            // Save settings when window closes
            this.Closed += Window_Closed;
            
            _isInitializing = false;
            _isLoaded = true;
        }

        private void SettingsService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!_isLoaded)
            {
                return;
            }

            if (e.PropertyName == nameof(SettingsService.Theme))
            {
                ApplyThemeFromSettings();
            }
        }

        private void ApplyThemeFromSettings()
        {
            var theme = SettingsService.Instance.Theme;
            var elementTheme = ThemeManager.ParseTheme(theme);

            if (Content is FrameworkElement root)
            {
                root.RequestedTheme = elementTheme;
            }

            UpdateTitleBarTheme(elementTheme);
        }

        

        public async Task<bool> ShowAsync()
        {
            _taskCompletionSource = new TaskCompletionSource<bool>();
            
            try
            {
                // Load saved path if it exists
                try
                {
                    var defaultPath = SettingsService.Instance.DefaultFolderPath;
                    if (!string.IsNullOrWhiteSpace(defaultPath) && Directory.Exists(defaultPath))
                    {
                        currentPath = defaultPath;
                        LoadFolders();
                        if (currentPathText != null)
                        {
                            currentPathText.Text = currentPath;
                        }
                    }
                    else
                    {
                    var savedState = await FolderBrowserSettings.LoadAsync();
                    if (savedState?.LastPath != null && Directory.Exists(savedState.LastPath))
                    {
                        currentPath = savedState.LastPath;
                        LoadFolders();
                        if (currentPathText != null)
                            currentPathText.Text = currentPath;
                    }
                    }
                }
                catch (Exception ex)
                {
                    // Could not load saved path - continue with default
                    System.Diagnostics.Debug.WriteLine($"[FOLDER PICKER] Warning: Could not load saved path: {ex.Message}");
                    // Continue with default path
                }
                
                var appWindow = AppWindow;
                
                // Restore saved position and size
                try
                {
                    var savedState = await FolderBrowserSettings.LoadAsync();
                    
                    if (savedState != null && savedState.Width > 0 && savedState.Height > 0)
                    {
                        // Restore size first
                        appWindow.Resize(new SizeInt32 
                        { 
                            Width = Math.Max(MIN_WIDTH, savedState.Width), 
                            Height = Math.Max(MIN_HEIGHT, savedState.Height) 
                        });
                        
                        // Then restore position if valid
                        if (FolderBrowserSettings.IsValidPosition(savedState, appWindow.Size))
                        {
                            appWindow.Move(new PointInt32(savedState.X, savedState.Y));
                        }
                        else
                        {
                            // Position invalid - center with restored size
                            CenterWindow();
                        }
                    }
                    else
                    {
                        // No saved state - use default size and center on first open
                        appWindow.Resize(new SizeInt32 { Width = 800, Height = 600 });
                        CenterWindow();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[FOLDER PICKER] Warning: Could not restore position: {ex.Message}");
                    // Fall back to default
                    appWindow.Resize(new SizeInt32 { Width = 800, Height = 600 });
                    CenterWindow();
                }
                
                this.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FOLDER PICKER] Error in ShowAsync: {ex.Message}");
            }
            
            return await _taskCompletionSource.Task;
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            // Skip saving during initialization
            if (_isInitializing) return;
            
            if (args.DidSizeChange)
            {
                EnforceMinimumSize(sender);
            }
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Save window state when closing
            SaveWindowState();
        }

        private void SaveWindowState()
        {
            var appWindow = AppWindow;
            if (appWindow == null) return;

            var position = appWindow.Position;
            var size = appWindow.Size;

            _ = FolderBrowserSettings.SaveAsync(position.X, position.Y, size.Width, size.Height, currentPath);
        }

        private void EnforceMinimumSize(AppWindow appWindow)
        {
            var size = appWindow.Size;
            var width = Math.Max(MIN_WIDTH, size.Width);
            var height = Math.Max(MIN_HEIGHT, size.Height);

            if (width == size.Width && height == size.Height) return;

            appWindow.Resize(new SizeInt32 { Width = width, Height = height });
        }

        private void CenterWindow()
        {
            var appWindow = AppWindow;
            if (appWindow == null) return;

            var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            if (displayArea == null) return;

            var workArea = displayArea.WorkArea;
            var size = appWindow.Size;
            
            int x = workArea.X + (workArea.Width - size.Width) / 2;
            int y = workArea.Y + (workArea.Height - size.Height) / 2;

            appWindow.Move(new PointInt32(x, y));
        }

        private void SetupTitleBar()
        {
            var appWindow = AppWindow;
            if (appWindow?.TitleBar != null)
            {
                // Get theme from content
                ElementTheme currentTheme = ElementTheme.Default;
                if (this.Content is FrameworkElement element)
                {
                    currentTheme = element.ActualTheme;
                }
                UpdateTitleBarTheme(currentTheme);
            }
        }

        private void Content_ActualThemeChanged(FrameworkElement sender, object args)
        {
            UpdateTitleBarTheme(sender.ActualTheme);
        }

        public void UpdateTitleBarTheme(ElementTheme theme)
        {
            var appWindow = AppWindow;
            if (appWindow?.TitleBar == null) return;

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

        private void CloseDialog(bool result)
        {
            DialogResult = result;
            if (result)
            {
                SelectedPath = currentPath;
            }
            _taskCompletionSource?.SetResult(result);
            this.Close();
        }

        private void BuildUI()
        {

            var rootGrid = new Grid { Padding = new Thickness(20), RowSpacing = 12 };
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Quick access
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Navigation
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Folder list
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Info text
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Action buttons

            // Quick access buttons - icon-only with consistent width (Drives on left, then Home, Desktop, Documents, Downloads, Pictures)
            var quickAccessPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 0, 0, 8) };
            
            var drivesBtn = new Button { Content = new FontIcon { Glyph = "\uE8EC", FontSize = 18 }, Width = 48, Height = 42, Padding = new Thickness(4) };
            ToolTipService.SetToolTip(drivesBtn, "Drives");
            drivesBtn.Click += DrivesButton_Click;
            
            var homeBtn = CreateQuickAccessButton("\uE80F", "Home", Environment.SpecialFolder.UserProfile); // Home icon
            var desktopBtn = CreateQuickAccessButton("\uE8FC", "Desktop", Environment.SpecialFolder.Desktop); // Desktop icon
            var documentsBtn = CreateQuickAccessButton("\uE8A5", "Documents", Environment.SpecialFolder.MyDocuments); // Document icon
            var downloadsBtn = CreateQuickAccessButton("\uE896", "Downloads", Environment.SpecialFolder.UserProfile); // Download icon
            var picturesBtn = CreateQuickAccessButton("\uEB9F", "Pictures", Environment.SpecialFolder.MyPictures); // Picture icon
            
            quickAccessPanel.Children.Add(drivesBtn);
            quickAccessPanel.Children.Add(homeBtn);
            quickAccessPanel.Children.Add(desktopBtn);
            quickAccessPanel.Children.Add(documentsBtn);
            quickAccessPanel.Children.Add(downloadsBtn);
            quickAccessPanel.Children.Add(picturesBtn);
            
            Grid.SetRow(quickAccessPanel, 0);
            rootGrid.Children.Add(quickAccessPanel);

            // Navigation buttons - consistent width (Up button on left, then Path display/edit)
            var navGrid = new Grid { ColumnSpacing = 8 };
            navGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Up
            navGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Path

            var upButton = new Button { Content = new FontIcon { Glyph = "\uE74A", FontSize = 18 }, Width = 48, Height = 42, Padding = new Thickness(4) };
            ToolTipService.SetToolTip(upButton, "Up");
            upButton.Click += UpButton_Click;
            Grid.SetColumn(upButton, 0);
            navGrid.Children.Add(upButton);

            // Path display/edit container - overlapping TextBlock and TextBox in same location
            var pathContainer = new Grid { Margin = new Thickness(8, 0, 8, 0) };
            
            currentPathText = new TextBlock 
            { 
                Text = currentPath, 
                VerticalAlignment = VerticalAlignment.Center, 
                TextTrimming = TextTrimming.CharacterEllipsis, 
                FontSize = 13
            };
            currentPathText.PointerPressed += CurrentPathText_PointerPressed;
            currentPathText.PointerEntered += CurrentPathText_PointerEntered;
            currentPathText.PointerExited += CurrentPathText_PointerExited;
            ToolTipService.SetToolTip(currentPathText, "Click to edit path");
            pathContainer.Children.Add(currentPathText);

            // Path edit textBox - overlays the TextBlock when editing
            pathEditBox = new TextBox 
            { 
                Text = currentPath, 
                PlaceholderText = "Enter folder path (e.g., C:\\Users)...", 
                Visibility = Visibility.Collapsed,
                VerticalAlignment = VerticalAlignment.Center
            };
            pathEditBox.KeyDown += PathEditBox_KeyDown;
            pathContainer.Children.Add(pathEditBox);
            
            Grid.SetColumn(pathContainer, 1);
            navGrid.Children.Add(pathContainer);

            Grid.SetRow(navGrid, 1);
            rootGrid.Children.Add(navGrid);

            var border = new Border 
            { 
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(0),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 8, 0, 8)
            };
            folderListView = new ListView 
            { 
                ItemsSource = FolderItems, 
                SelectionMode = ListViewSelectionMode.Single
            };
            folderListView.DoubleTapped += FolderListView_DoubleTapped;
            border.Child = folderListView;
            Grid.SetRow(border, 2);
            rootGrid.Children.Add(border);

            // Info text
            infoText = new TextBlock { Text = "Ready to select folder", FontSize = 12, Opacity = 0.7, Margin = new Thickness(0, 0, 0, 8) };
            Grid.SetRow(infoText, 3);
            rootGrid.Children.Add(infoText);

            // Action buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8 };
            
            var selectButton = new Button { Content = "Select", Style = Application.Current.Resources["AccentButtonStyle"] as Style, MinWidth = 100 };
            selectButton.Click += (s, e) => CloseDialog(true);
            
            var cancelButton = new Button { Content = "Cancel", MinWidth = 100 };
            cancelButton.Click += (s, e) => CloseDialog(false);
            
            buttonPanel.Children.Add(selectButton);
            buttonPanel.Children.Add(cancelButton);
            
            Grid.SetRow(buttonPanel, 4);
            rootGrid.Children.Add(buttonPanel);

            this.Content = rootGrid;
        }

        private Button CreateQuickAccessButton(string iconGlyph, string tooltip, Environment.SpecialFolder folder)
        {
            var btn = new Button 
            { 
                Content = new FontIcon { Glyph = iconGlyph, FontSize = 18 },
                Width = 48,
                Height = 42,
                Padding = new Thickness(4)
            };
            ToolTipService.SetToolTip(btn, tooltip);
            btn.Click += (s, e) =>
            {
                try
                {
                    string path = Environment.GetFolderPath(folder);
                    if (tooltip == "Downloads")
                    {
                        path = Path.Combine(path, "Downloads");
                    }
                    if (Directory.Exists(path))
                    {
                        currentPath = path;
                        LoadFolders();
                    }
                }
                catch { }
            };
            return btn;
        }

        private void LoadFolders()
        {
            try
            {
                FolderItems.Clear();

                try
                {
                    var directories = Directory.GetDirectories(currentPath)
                        .Select(d => new DirectoryInfo(d).Name)
                        .OrderBy(d => d)
                        .ToList();

                    foreach (var dir in directories)
                    {
                        FolderItems.Add(dir);
                    }

                    currentPathText.Text = currentPath;
                    pathEditBox.Text = currentPath;
                    infoText.Text = $"{directories.Count} folders";

                    if (directories.Count == 0)
                    {
                        infoText.Text = "No subfolders in this directory";
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    infoText.Text = "Access denied to this folder";
                    FolderItems.Clear();
                }
            }
            catch (Exception ex)
            {
                infoText.Text = $"Error: {ex.Message}";
            }
        }

        private void FolderListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (folderListView.SelectedItem is string selectedFolder)
            {
                try
                {
                    string newPath = Path.Combine(currentPath, selectedFolder);
                    if (Directory.Exists(newPath))
                    {
                        currentPath = newPath;
                        LoadFolders();
                    }
                }
                catch (Exception ex)
                {
                    infoText.Text = $"Error navigating: {ex.Message}";
                }
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var parent = Directory.GetParent(currentPath);
                if (parent != null)
                {
                    currentPath = parent.FullName;
                    LoadFolders();
                }
            }
            catch (Exception ex)
            {
                infoText.Text = $"Error: {ex.Message}";
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            LoadFolders();
        }

        private void EditPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (pathEditBox.Visibility == Visibility.Collapsed)
            {
                pathEditBox.Visibility = Visibility.Visible;
                pathEditBox.Focus(FocusState.Programmatic);
                pathEditBox.SelectAll();
            }
            else
            {
                pathEditBox.Visibility = Visibility.Collapsed;
            }
        }

        private void CurrentPathText_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Show edit box when path is clicked
            if (pathEditBox.Visibility == Visibility.Collapsed)
            {
                currentPathText.Visibility = Visibility.Collapsed;
                pathEditBox.Visibility = Visibility.Visible;
                pathEditBox.Focus(FocusState.Programmatic);
                pathEditBox.SelectAll();
                e.Handled = true;
            }
        }

        private void CurrentPathText_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // Add hover feedback - make text slightly transparent to indicate it's interactive
            if (currentPathText.Visibility == Visibility.Visible)
            {
                currentPathText.Opacity = 0.7;
            }
        }

        private void CurrentPathText_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Remove hover feedback
            currentPathText.Opacity = 1.0;
        }

        private async void DrivesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get all available drives
                var drives = DriveInfo.GetDrives();
                
                if (drives.Length == 0)
                {
                    infoText.Text = "No drives found";
                    return;
                }

                // Create a simple dialog with drive options
                var drivesPanel = new StackPanel { Spacing = 8 };
                
                // ContentDialog for drive selection (create early so we can close it from button handler)
                var dialog = new ContentDialog 
                { 
                    Title = "Select Drive",
                    XamlRoot = this.Content.XamlRoot
                };

                // Apply theme to dialog matching the parent window
                if (this.Content is FrameworkElement element)
                {
                    dialog.RequestedTheme = element.ActualTheme;
                }

                foreach (var drive in drives.OrderBy(d => d.Name))
                {
                    try
                    {
                        // Check if drive is ready before accessing properties
                        if (!drive.IsReady)
                        {
                            infoText.Text = $"(Skipped {drive.Name}: drive not ready)";
                            continue;
                        }

                        var driveButton = new Button 
                        { 
                            Content = $"{drive.Name} ({drive.VolumeLabel})",
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Padding = new Thickness(12, 8, 12, 8)
                        };
                        
                        var drivePath = drive.Name.TrimEnd('\\');
                        driveButton.Click += (s, args) =>
                        {
                            currentPath = drivePath;
                            LoadFolders();
                            dialog.Hide(); // Close dialog after selection
                        };
                        
                        drivesPanel.Children.Add(driveButton);
                    }
                    catch (Exception driveEx)
                    {
                        // Log error but continue with other drives
                        infoText.Text = $"(Error accessing {drive.Name}: {driveEx.Message})";
                    }
                }

                if (drivesPanel.Children.Count == 0)
                {
                    infoText.Text = "No accessible drives found";
                    return;
                }

                // Scroll viewer for drive list
                var scrollViewer = new ScrollViewer 
                { 
                    Content = drivesPanel, 
                    MaxHeight = 300,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };

                // Create a container with centered content
                var container = new Grid();
                var closeButton = new Button 
                { 
                    Content = "Cancel",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    MinWidth = 100
                };

                var contentPanel = new StackPanel { Spacing = 12 };
                contentPanel.Children.Add(scrollViewer);
                contentPanel.Children.Add(closeButton);

                // Set dialog content
                dialog.Content = contentPanel;

                // Close dialog when cancel button is clicked
                closeButton.Click += (s, args) => dialog.Hide();

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                infoText.Text = $"Error loading drives: {ex.Message}";
            }
        }

        private void PathEditBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string enteredPath = pathEditBox.Text.Trim();
                if (Directory.Exists(enteredPath))
                {
                    currentPath = enteredPath;
                    pathEditBox.Visibility = Visibility.Collapsed;
                    currentPathText.Visibility = Visibility.Visible;
                    LoadFolders();
                    e.Handled = true;
                }
                else
                {
                    infoText.Text = "Path does not exist";
                }
            }
            else if (e.Key == Windows.System.VirtualKey.Escape)
            {
                pathEditBox.Visibility = Visibility.Collapsed;
                currentPathText.Visibility = Visibility.Visible;
                e.Handled = true;
            }
        }
    }
}

