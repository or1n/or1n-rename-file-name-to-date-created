using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Or1nRenameFileNameToDateCreated.Views
{
    public sealed partial class MainPage : Page
    {
        private List<string> _logLines = new();
        private bool _folderSelected = false;
        private string _selectedFolderPath = string.Empty;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[MainPage_Loaded] Page loaded");
        }

        private static void SetWindowSize(int width, int height)
        {
            System.Diagnostics.Debug.WriteLine($"[SetWindowSize] Setting window size to {width}x{height}");
        }

        private void Log(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            _logLines.Add($"[{timestamp}] {message}");
            while (_logLines.Count > 100)
                _logLines.RemoveAt(_logLines.Count - 1);
            if (InfoTextBlock != null)
            {
                InfoTextBlock.Text = string.Join("\n", _logLines.Skip(Math.Max(0, _logLines.Count - 10)));
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[ThemeComboBox_SelectionChanged] Selection changed");
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
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeComboBox_SelectionChanged] Exception: {ex}");
                throw;
            }
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                _folderSelected = true;
                _selectedFolderPath = folder.Path;
                Log($"Selected folder: {folder.Path}");
            }
            else
            {
                if (_folderSelected)
                {
                    Log("Folder selection cancelled.");
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
                Log($"Scan complete. {files.Count} files found.");
            }
            catch (ArgumentException ex)
            {
                Log($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
