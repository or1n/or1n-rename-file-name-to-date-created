using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.DynamicDependency;
using Or1nRenameFileNameToDateCreated.Helpers;
using System;
using System.Threading;

namespace Or1nRenameFileNameToDateCreated
{
    /// <summary>
    /// Provides the application entry point for the WinUI 3 desktop app.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Runs the application with WinRT and WinUI bootstrap initialization.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the process.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            Bootstrap.TryInitialize(0x00010008, out var _);

            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                var app = new App();

                var window = new MainWindow();
                // Note: Window sizing is handled by MainWindow's RestoreOrCenterWindow()
                // which restores the last saved size or centers at default size
                window.Activate();

                _ = SettingsService.Instance.InitializeAsync();
            });
        }
    }

}
