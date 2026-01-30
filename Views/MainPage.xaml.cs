using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace or1n_rename_file_name_to_date_created.Views
{
    public sealed partial class MainPage : Page
    {
        private const int MaxLogLines = 100;
        private readonly Queue<string> _logLines = new();

        public MainPage()
        {
            this.InitializeComponent();
            Log("App started.");
        }

        private void Log(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            _logLines.Enqueue($"[{timestamp}] {message}");
            while (_logLines.Count > MaxLogLines)
                _logLines.Dequeue();
            InfoTextBlock.Text = string.Join("\n", _logLines.Skip(Math.Max(0, _logLines.Count - 10)));
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Log("Open Folder button pressed.");
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                SelectedFolderText.Text = folder.Path;
                Log($"Selected folder: {folder.Path}");
            }
            else
            {
                Log("No folder selected.");
            }
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            Log("Scan button pressed.");
            var folderPath = SelectedFolderText.Text;
            if (string.IsNullOrWhiteSpace(folderPath) || folderPath == "No folder selected")
            {
                Log("Please select a folder first.");
                return;
            }
            try
            {
                Log($"Scanning folder: {folderPath}");
                var dir = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
                var files = await dir.GetFilesAsync();
                var groups = files.GroupBy(f => f.FileType.ToLowerInvariant())
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count);
                foreach (var group in groups)
                {
                    Log($"{group.Type}: {group.Count}");
                }
                Log($"Scan complete. {files.Count} files found.");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }
    }
}
