using TasteHub.ViewModels;

namespace TasteHub.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _viewModel;

        public HomePage(HomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadRecipesAsync();
            StartBarometer();
            StartShakeDetection();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopBarometer();
            StopShakeDetection();
        }

        private void OnAllCategoryClicked(object sender, EventArgs e)
        {
            _viewModel.SelectedCategory = "All";
        }

        private void OnFoodCategoryClicked(object sender, EventArgs e)
        {
            _viewModel.SelectedCategory = "Food";
        }

        private void OnDrinkCategoryClicked(object sender, EventArgs e)
        {
            _viewModel.SelectedCategory = "Drink";
        }

        private void StartBarometer()
        {
            try
            {
                if (Barometer.Default.IsSupported)
                {
                    Barometer.Default.ReadingChanged += OnBarometerReadingChanged;
                    Barometer.Default.Start(SensorSpeed.UI);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Barometer start error: {ex.Message}");
            }
        }

        private void StopBarometer()
        {
            try
            {
                if (Barometer.Default.IsSupported && Barometer.Default.IsMonitoring)
                {
                    Barometer.Default.ReadingChanged -= OnBarometerReadingChanged;
                    Barometer.Default.Stop();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Barometer stop error: {ex.Message}");
            }
        }

        private async void OnBarometerReadingChanged(object sender, BarometerChangedEventArgs e)
        {
            await _viewModel.UpdateBarometerRecommendationAsync(e.Reading.PressureInHectopascals);
        }

        private void StartShakeDetection()
        {
            try
            {
                if (Accelerometer.Default.IsSupported)
                {
                    Accelerometer.Default.ShakeDetected += OnShakeDetected;
                    Accelerometer.Default.Start(SensorSpeed.Game);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shake detection start error: {ex.Message}");
            }
        }

        private void StopShakeDetection()
        {
            try
            {
                if (Accelerometer.Default.IsSupported && Accelerometer.Default.IsMonitoring)
                {
                    Accelerometer.Default.ShakeDetected -= OnShakeDetected;
                    Accelerometer.Default.Stop();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shake detection stop error: {ex.Message}");
            }
        }

        private async void OnShakeDetected(object sender, EventArgs e)
        {
            await _viewModel.ShakeDetectedAsync();
        }
    }
}