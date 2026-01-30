using Microsoft.UI.Xaml.Navigation;

namespace or1n_rename_file_name_to_date_created
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window window = Window.Current;
        public static Window? WindowInstance { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            // this.InitializeComponent(); // Not needed in WinUI 3
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try
                {
                    var ex = e.ExceptionObject as Exception;
                    var msg = $"[UNHANDLED] {DateTime.Now}: {ex?.ToString() ?? e.ExceptionObject.ToString()}\n";
                    System.IO.File.AppendAllText("app_crash.log", msg);
                }
                catch { }
            };
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                try
                {
                    var msg = $"[UNOBSERVED TASK] {DateTime.Now}: {e.Exception.ToString()}\n";
                    System.IO.File.AppendAllText("app_crash.log", msg);
                }
                catch { }
            };
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                window ??= new Window();
                WindowInstance = window;

                if (window.Content is not Frame rootFrame)
                {
                    rootFrame = new Frame();
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    window.Content = rootFrame;
                }

                _ = rootFrame.Navigate(typeof(MainPage), e.Arguments);
                window.Activate();

                // Bring window to foreground
                try
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    if (hwnd != IntPtr.Zero)
                    {
                        Win32.BringWindowToFront(hwnd);
                    }
                }
                catch { /* Ignore if fails */ }
            }
            catch (Exception ex)
            {
                try
                {
                    var msg = $"[ONLAUNCHED] {DateTime.Now}: {ex.ToString()}\n";
                    System.IO.File.AppendAllText("app_crash.log", msg);
                }
                catch { }
                throw;
            }
        }

        private static class Win32
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            public static void BringWindowToFront(IntPtr hwnd)
            {
                ShowWindow(hwnd, 5); // SW_SHOW
                SetForegroundWindow(hwnd);
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
