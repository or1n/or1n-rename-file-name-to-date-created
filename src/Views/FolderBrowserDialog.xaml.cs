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
        }

        public async Task<bool> ShowAsync()
        {
            _taskCompletionSource = new TaskCompletionSource<bool>();
            
            try
            {
                // Load saved path if it exists
                try
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

        private void UpdateTitleBarTheme(ElementTheme theme)
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
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Path edit
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Folder list
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Info text
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Action buttons

            // Quick access buttons - icon-only with consistent width (Home on left, then Desktop, Documents, Downloads, Pictures)
            var quickAccessPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 0, 0, 8) };
            
            var homeBtn = CreateQuickAccessButton("\uE80F", "Home", Environment.SpecialFolder.UserProfile); // Home icon
            var desktopBtn = CreateQuickAccessButton("\uE8FC", "Desktop", Environment.SpecialFolder.Desktop); // Desktop icon
            var documentsBtn = CreateQuickAccessButton("\uE8A5", "Documents", Environment.SpecialFolder.MyDocuments); // Document icon
            var downloadsBtn = CreateQuickAccessButton("\uE896", "Downloads", Environment.SpecialFolder.UserProfile); // Download icon
            var picturesBtn = CreateQuickAccessButton("\uEB9F", "Pictures", Environment.SpecialFolder.MyPictures); // Picture icon
            
            quickAccessPanel.Children.Add(homeBtn);
            quickAccessPanel.Children.Add(desktopBtn);
            quickAccessPanel.Children.Add(documentsBtn);
            quickAccessPanel.Children.Add(downloadsBtn);
            quickAccessPanel.Children.Add(picturesBtn);
            
            Grid.SetRow(quickAccessPanel, 0);
            rootGrid.Children.Add(quickAccessPanel);

            // Navigation buttons - consistent width (Up button on left, then Path display)
            var navGrid = new Grid { ColumnSpacing = 8 };
            navGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Up
            navGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Path

            var upButton = new Button { Content = new FontIcon { Glyph = "\uE74A", FontSize = 16 }, Width = 40, Height = 36 };
            ToolTipService.SetToolTip(upButton, "Up");
            upButton.Click += UpButton_Click;
            Grid.SetColumn(upButton, 0);
            navGrid.Children.Add(upButton);

            currentPathText = new TextBlock { Text = currentPath, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, FontSize = 13, Margin = new Thickness(8, 0, 8, 0) };
            currentPathText.PointerPressed += CurrentPathText_PointerPressed;
            ToolTipService.SetToolTip(currentPathText, "Click to edit path");
            Grid.SetColumn(currentPathText, 1);
            navGrid.Children.Add(currentPathText);

            Grid.SetRow(navGrid, 1);
            rootGrid.Children.Add(navGrid);

            // Path edit textBox
            pathEditBox = new TextBox { Text = currentPath, PlaceholderText = "Enter folder path (e.g., C:\\Users)...", Visibility = Visibility.Collapsed };
            pathEditBox.KeyDown += PathEditBox_KeyDown;
            Grid.SetRow(pathEditBox, 2);
            rootGrid.Children.Add(pathEditBox);
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
            Grid.SetRow(border, 3);
            rootGrid.Children.Add(border);

            // Info text
            infoText = new TextBlock { Text = "Ready to select folder", FontSize = 12, Opacity = 0.7, Margin = new Thickness(0, 0, 0, 8) };
            Grid.SetRow(infoText, 4);
            rootGrid.Children.Add(infoText);

            // Action buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8 };
            
            var selectButton = new Button { Content = "Select", Style = Application.Current.Resources["AccentButtonStyle"] as Style, MinWidth = 100 };
            selectButton.Click += (s, e) => CloseDialog(true);
            
            var cancelButton = new Button { Content = "Cancel", MinWidth = 100 };
            cancelButton.Click += (s, e) => CloseDialog(false);
            
            buttonPanel.Children.Add(selectButton);
            buttonPanel.Children.Add(cancelButton);
            
            Grid.SetRow(buttonPanel, 5);
            rootGrid.Children.Add(buttonPanel);

            this.Content = rootGrid;
        }

        private Button CreateQuickAccessButton(string iconGlyph, string tooltip, Environment.SpecialFolder folder)
        {
            var btn = new Button 
            { 
                Content = new FontIcon { Glyph = iconGlyph, FontSize = 16 },
                Width = 40,
                Height = 36
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
                pathEditBox.Visibility = Visibility.Visible;
                pathEditBox.Focus(FocusState.Programmatic);
                pathEditBox.SelectAll();
                e.Handled = true;
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
                e.Handled = true;
            }
        }
    }
}

