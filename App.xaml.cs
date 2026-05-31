namespace TasteHub
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            LoadThemePreference();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            return new Window(new AppShell());
        }

        private void LoadThemePreference()
        {
            try
            {
                bool isDarkMode = Preferences.Get("IsDarkMode", false);
                UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;

                string fontSize = Preferences.Get("FontSize", "Medium");
                double fontValue = fontSize switch
                {
                    "Small" => 13,
                    "Medium" => 16,
                    "Large" => 20,
                    "Extra Large" => 24,
                    _ => 16
                };

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