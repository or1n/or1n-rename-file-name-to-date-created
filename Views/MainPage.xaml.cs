using WinUIEx;
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
            this.Loaded += MainPage_Loaded;
            SetWindowSize(900, 420); // Set compact initial window size
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Use WinUIEx to set minimum window size and margin
            var window = App.WindowInstance;
            if (window is not null)
            {
                var manager = WinUIEx.WindowManager.Get(window);
                manager.MinWidth = 480;
                manager.MinHeight = 320;
                window.CenterOnScreen();
            }

        }

        // Set the window size using AppWindow API for WinUI 3
        private void SetWindowSize(int width, int height)
        {
            try
            {
                var window = App.WindowInstance;
                if (window is null) return;
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                if (appWindow != null)
                {
                    appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
                    var presenter = appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
                    if (presenter != null)
                    {
                        presenter.IsMaximizable = false;
                        presenter.IsResizable = true;
                    }
                }
            }
            catch { /* Ignore if fails */ }
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
