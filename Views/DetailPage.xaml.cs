using TasteHub.ViewModels;

namespace TasteHub.Views
{
    /// <summary>
    /// Detail page showing full recipe information with TTS and pinch-to-zoom
    /// </summary>
    public partial class DetailPage : ContentPage
    {
        private readonly DetailViewModel _viewModel;

        public DetailPage(DetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        /// <summary>
        /// Handle pinch gesture to zoom the recipe cover image
        /// </summary>
        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Running:
                    double newScale = _viewModel.ImageScale * e.Scale;
                    _viewModel.ImageScale = Math.Clamp(newScale, 1.0, 4.0);
                    break;
                case GestureStatus.Completed:
                    // Keep the current scale
                    break;
            }
        }

        /// <summary>
        /// Navigate to interactive cooking page with recipe info
        /// </summary>
        private async void OnCookClicked(object sender, EventArgs e)
        {
            if (_viewModel.Recipe == null) return;

            await Shell.Current.GoToAsync(
                $"InteractivePage?name={Uri.EscapeDataString(_viewModel.Recipe.Name)}&category={_viewModel.Recipe.Category}");
        }
    }
}