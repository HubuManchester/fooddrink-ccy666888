using TasteHub.ViewModels;

namespace TasteHub.Views
{
    /// <summary>
    /// Code-behind for the Add/Edit Recipe page.
    /// Handles page initialisation and dependency injection of the view model.
    /// All business logic, validation and camera handling is delegated to
    /// AddEditViewModel following the MVVM pattern for clean separation of concerns.
    /// </summary>
    public partial class AddEditPage : ContentPage
    {
        /// <summary>
        /// Initialises the AddEditPage and binds the provided view model.
        /// The same page is reused for both creating new recipes and editing
        /// existing ones, demonstrating code reusability.
        /// </summary>
        /// <param name="viewModel">The view model providing data binding and commands</param>
        public AddEditPage(AddEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}