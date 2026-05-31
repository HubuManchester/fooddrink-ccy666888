using TasteHub.ViewModels;

namespace TasteHub.Views
{
    /// <summary>
    /// Home page displaying recipe list with search, filtering,
    /// barometer recommendation and shake-to-earn coupon
    /// </summary>
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _viewModel;

        // Shake detection variables
        private DateTime _lastShakeTime = DateTime.MinValue;
        private double _lastX, _lastY, _lastZ;
        private bool _shakeInitialized = false;

        public HomePage(HomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        /// <summary>
        /// Load recipes and start hardware sensors when page appears
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadRecipesAsync();
            StartBarometer();
            StartShakeDetection();
        }

        /// <summary>
        /// Stop hardware sensors when page disappears
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopBarometer();
            StopShakeDetection();
        }

        /// <summary>Set category filter to All and reload</summary>
        private async void OnAllCategoryClicked(object sender, EventArgs e)
        {
            _viewModel.SelectedCategory = "All";
            await _viewModel.LoadRecipesAsync();
        }

        /// <summary>Set category filter to Food and reload</summary>
        private async void OnFoodCategoryClicked(object sender, EventArgs e)
        {
            _viewModel.SelectedCategory = "Food";
            await _viewModel.LoadRecipesAsync();
        }

        /// <summary>Set category filter to Drink and reload</summary>
        private async void OnDrinkCategoryClicked(object sender, EventArgs e)
        {
            _viewModel.SelectedCategory = "Drink";
            await _viewModel.LoadRecipesAsync();
        }

        // ==================== Barometer Hardware ====================

        /// <summary>
        /// Start monitoring the barometer sensor for weather-based recommendations
        /// </summary>
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

        /// <summary>
        /// Stop the barometer sensor
        /// </summary>
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

        /// <summary>
        /// Handle barometer reading changes and update recommendation
        /// </summary>
        private async void OnBarometerReadingChanged(object sender, BarometerChangedEventArgs e)
        {
            await _viewModel.UpdateBarometerRecommendationAsync(e.Reading.PressureInHectopascals);
        }

        // ==================== Shake Detection (Manual via Accelerometer) ====================

        /// <summary>
        /// Start monitoring accelerometer for manual shake detection
        /// </summary>
        private void StartShakeDetection()
        {
            try
            {
                if (Accelerometer.Default.IsSupported && !Accelerometer.Default.IsMonitoring)
                {
                    Accelerometer.Default.ReadingChanged += OnAccelerometerForShake;
                    Accelerometer.Default.Start(SensorSpeed.Game);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shake start error: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop the accelerometer sensor
        /// </summary>
        private void StopShakeDetection()
        {
            try
            {
                if (Accelerometer.Default.IsSupported && Accelerometer.Default.IsMonitoring)
                {
                    Accelerometer.Default.ReadingChanged -= OnAccelerometerForShake;
                    Accelerometer.Default.Stop();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shake stop error: {ex.Message}");
            }
        }

        /// <summary>
        /// Detect shake by measuring acceleration force change.
        /// Triggers coupon generation when force exceeds threshold.
        /// Cooldown of 3 seconds between shakes to prevent duplicates.
        /// </summary>
        private void OnAccelerometerForShake(object sender, AccelerometerChangedEventArgs e)
        {
            double x = e.Reading.Acceleration.X;
            double y = e.Reading.Acceleration.Y;
            double z = e.Reading.Acceleration.Z;

            if (_shakeInitialized)
            {
                double force = Math.Abs(x + y + z - _lastX - _lastY - _lastZ);

                if (force > 2.5 && (DateTime.Now - _lastShakeTime).TotalSeconds > 3)
                {
                    _lastShakeTime = DateTime.Now;
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await _viewModel.ShakeDetectedAsync();
                    });
                }
            }

            _lastX = x;
            _lastY = y;
            _lastZ = z;
            _shakeInitialized = true;
        }
    }
}