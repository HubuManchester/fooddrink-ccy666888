using TasteHub.ViewModels;

namespace TasteHub.Views
{
    /// <summary>
    /// Settings page for managing theme, font size and viewing
    /// accessibility information
    /// </summary>
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}