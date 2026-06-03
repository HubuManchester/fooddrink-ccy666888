namespace TasteHub
{
    /// <summary>
    /// Main application class responsible for initialising global resources,
    /// creating the application window, and restoring user preferences
    /// (theme and font size) from persistent storage on startup.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initialises the application, loads XAML resources and restores
        /// the user's saved theme and font size preferences.
        /// </summary>
        public App()
        {
            InitializeComponent();
            LoadThemePreference();
        }

        /// <summary>
        /// Creates the main application window with AppShell as the root page.
        /// Called by the MAUI framework during the application lifecycle.
        /// </summary>
        /// <param name="activationState">Platform-specific activation state</param>
        /// <returns>A new Window containing the AppShell navigation structure</returns>
        protected override Window CreateWindow(IActivationState activationState)
        {
            return new Window(new AppShell());
        }

        /// <summary>
        /// Loads and applies the user's saved theme (dark/light) and font size
        /// preferences from the device's persistent Preferences storage.
        /// This ensures settings persist across app restarts (WCAG 1.4.4 Resize Text).
        /// </summary>
        private void LoadThemePreference()
        {
            try
            {
                // Restore dark/light theme preference
                bool isDarkMode = Preferences.Get("IsDarkMode", false);
                UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;

                // Restore font size preference with 4 levels (Small/Medium/Large/Extra Large)
                string fontSize = Preferences.Get("FontSize", "Medium");
                double fontValue = fontSize switch
                {
                    "Small" => 13,
                    "Medium" => 16,
                    "Large" => 20,
                    "Extra Large" => 24,
                    _ => 16
                };

                // Update all DynamicResource font sizes so every page reflects the change
                Resources["AppFontSize"] = fontValue;
                Resources["AppFontSizeSmall"] = fontValue - 2;
                Resources["AppFontSizeLarge"] = fontValue + 4;
                Resources["AppFontSizeTitle"] = fontValue + 8;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadThemePreference error: {ex.Message}");
            }
        }
    }
}