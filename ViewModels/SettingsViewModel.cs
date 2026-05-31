using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace TasteHub.ViewModels
{
    /// <summary>
    /// View model for the Settings page, managing theme and font size preferences
    /// </summary>
    public partial class SettingsViewModel : BaseViewModel
    {
        /// <summary>Whether dark mode is currently enabled</summary>
        [ObservableProperty]
        private bool _isDarkMode;

        /// <summary>Currently selected font size label</summary>
        [ObservableProperty]
        private string _selectedFontSize = "Medium";

        /// <summary>Current font size value in pixels</summary>
        [ObservableProperty]
        private double _fontSizeValue = 16;

        /// <summary>
        /// Constructor that loads saved preferences
        /// </summary>
        public SettingsViewModel()
        {
            Title = "Settings";
            LoadPreferences();
        }

        /// <summary>
        /// Load saved theme and font size preferences from app storage
        /// </summary>
        private void LoadPreferences()
        {
            try
            {
                IsDarkMode = Preferences.Get("IsDarkMode", false);
                SelectedFontSize = Preferences.Get("FontSize", "Medium");
                ApplyFontSize(SelectedFontSize);
                ApplyTheme(IsDarkMode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadPreferences error: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggle between dark and light theme
        /// </summary>
        partial void OnIsDarkModeChanged(bool value)
        {
            try
            {
                Preferences.Set("IsDarkMode", value);
                ApplyTheme(value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Theme change error: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply the selected theme to the application
        /// </summary>
        private void ApplyTheme(bool isDark)
        {
            try
            {
                if (Application.Current != null)
                {
                    Application.Current.UserAppTheme = isDark
                        ? AppTheme.Dark
                        : AppTheme.Light;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyTheme error: {ex.Message}");
            }
        }

        /// <summary>
        /// Set font size to Small (13px)
        /// </summary>
        [RelayCommand]
        public void SetFontSmall()
        {
            try
            {
                SelectedFontSize = "Small";
                ApplyFontSize("Small");
                Preferences.Set("FontSize", "Small");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetFontSmall error: {ex.Message}");
            }
        }

        /// <summary>
        /// Set font size to Medium (16px)
        /// </summary>
        [RelayCommand]
        public void SetFontMedium()
        {
            try
            {
                SelectedFontSize = "Medium";
                ApplyFontSize("Medium");
                Preferences.Set("FontSize", "Medium");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetFontMedium error: {ex.Message}");
            }
        }

        /// <summary>
        /// Set font size to Large (20px)
        /// </summary>
        [RelayCommand]
        public void SetFontLarge()
        {
            try
            {
                SelectedFontSize = "Large";
                ApplyFontSize("Large");
                Preferences.Set("FontSize", "Large");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetFontLarge error: {ex.Message}");
            }
        }

        /// <summary>
        /// Set font size to Extra Large (24px)
        /// </summary>
        [RelayCommand]
        public void SetFontExtraLarge()
        {
            try
            {
                SelectedFontSize = "Extra Large";
                ApplyFontSize("Extra Large");
                Preferences.Set("FontSize", "Extra Large");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetFontExtraLarge error: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply the selected font size to the application resources
        /// </summary>
        private void ApplyFontSize(string size)
        {
            try
            {
                FontSizeValue = size switch
                {
                    "Small" => 13,
                    "Medium" => 16,
                    "Large" => 20,
                    "Extra Large" => 24,
                    _ => 16
                };

                if (Application.Current != null)
                {
                    Application.Current.Resources["AppFontSize"] = FontSizeValue;
                    Application.Current.Resources["AppFontSizeSmall"] = FontSizeValue - 2;
                    Application.Current.Resources["AppFontSizeLarge"] = FontSizeValue + 4;
                    Application.Current.Resources["AppFontSizeTitle"] = FontSizeValue + 8;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyFontSize error: {ex.Message}");
            }
        }
    }
}