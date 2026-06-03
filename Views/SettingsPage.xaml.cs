using TasteHub.ViewModels;

namespace TasteHub.Views
{
    /// <summary>
    /// Represents the settings view within the application, responsible for allowing 
    /// users to manage themes, typography scales, and view compliance information.
    /// </summary>
    public partial class SettingsPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// </summary>
        /// <param name="viewModel">The view model component that supplies the underlying business logic and bindings.</param>
        public SettingsPage(SettingsViewModel viewModel)
        {
            // Initializes all visual components declared in the accompanying XAML file.
            InitializeComponent();

            // Establishes the data context for MVVM binding architecture.
            BindingContext = viewModel;
        }
    }
}