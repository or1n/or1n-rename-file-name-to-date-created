using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace or1n_rename_file_name_to_date_created.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.WindowInstance);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                SelectedFolderText.Text = folder.Path;
                InfoTextBlock.Text = $"Selected folder: {folder.Path}";
            }
            else
            {
                InfoTextBlock.Text = "No folder selected.";
            }
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            InfoTextBlock.Text = "Scanning folder...";
            ResultsListView.Items.Clear();
            var folderPath = SelectedFolderText.Text;
            if (string.IsNullOrWhiteSpace(folderPath) || folderPath == "No folder selected")
            {
                InfoTextBlock.Text = "Please select a folder first.";
                return;
            }
            try
            {
                var dir = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
                var files = await dir.GetFilesAsync();
                var groups = files.GroupBy(f => f.FileType.ToLowerInvariant())
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count);
                foreach (var group in groups)
                {
                    ResultsListView.Items.Add($"{group.Type}: {group.Count}");
                }
                InfoTextBlock.Text = $"Scan complete. {files.Count} files found.";
            }
            catch (Exception ex)
            {
                InfoTextBlock.Text = $"Error: {ex.Message}";
            }
        }
    }
}
