using TasteHub.ViewModels;

namespace TasteHub.Views
{
    /// <summary>
    /// Detail page showing full recipe information with TTS and pinch-to-zoom
    /// </summary>
    public partial class DetailPage : ContentPage
    {
        private readonly DetailViewModel _viewModel;
        private double _currentScale = 1;
        private double _startScale = 1;

        public DetailPage(DetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        /// <summary>
        /// Reload recipe data every time the page appears (e.g. after editing)
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                if (_viewModel.RecipeId > 0)
                {
                    await _viewModel.LoadRecipeAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DetailPage OnAppearing error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle pinch gesture to zoom the recipe cover image.
        /// Scale is clamped between 1x and 4x.
        /// </summary>
        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            try
            {
                switch (e.Status)
                {
                    case GestureStatus.Started:
                        _startScale = _currentScale;
                        break;
                    case GestureStatus.Running:
                        _currentScale = Math.Clamp(_startScale * e.Scale, 1.0, 4.0);
                        if (sender is Image image)
                        {
                            image.Scale = _currentScale;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PinchGesture error: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigate to interactive cooking page with recipe info
        /// </summary>
        private async void OnCookClicked(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel.Recipe == null) return;

                await Shell.Current.GoToAsync(
                    $"InteractivePage?name={Uri.EscapeDataString(_viewModel.Recipe.Name)}&category={_viewModel.Recipe.Category}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error",
                    "Failed to open interactive cooking mode. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"OnCookClicked error: {ex.Message}");
            }
        }
    }
}