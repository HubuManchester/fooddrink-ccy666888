using TasteHub.ViewModels;

namespace TasteHub.Views
{
    /// <summary>
    /// Page for adding a new recipe or editing an existing one,
    /// with camera integration for cover photos
    /// </summary>
    public partial class AddEditPage : ContentPage
    {
        public AddEditPage(AddEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}