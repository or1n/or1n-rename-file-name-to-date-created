using Microsoft.UI.Xaml;
using System;
using Windows.Graphics;
using Or1nRenameFileNameToDateCreated.Helpers;

namespace Or1nRenameFileNameToDateCreated
{
    /// <summary>
    /// Defines application-level behavior for the WinUI 3 app.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the application and loads XAML resources.
        /// </summary>
        public App()
        {
#pragma warning disable CS1061
            this.InitializeComponent();
#pragma warning restore CS1061

            // Initialize settings service
            InitializeSettingsAsync();
        }

        private async void InitializeSettingsAsync()
        {
            try
            {
                await SettingsService.Instance.InitializeAsync();
                System.Diagnostics.Debug.WriteLine("[App] SettingsService initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Error initializing SettingsService: {ex.Message}");
            }
        }
    }
}
