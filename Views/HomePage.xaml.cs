using TasteHub.ViewModels;

namespace TasteHub.Views
{
    /// <summary>
    /// Code-behind for the Home page, managing three hardware sensors:
    /// Barometer (weather-based recipe recommendation),
    /// Accelerometer (manual shake detection for coupon generation),
    /// and Compass (Surprise Me spinning wheel).
    /// Hardware sensor lifecycle is tied to page visibility via OnAppearing/OnDisappearing.
    /// </summary>
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _viewModel;

        // Shake detection state variables
        private DateTime _lastShakeTime = DateTime.MinValue;  // Prevents duplicate shake events
        private double _lastX, _lastY, _lastZ;                // Previous accelerometer readings
        private bool _shakeInitialized = false;               // Guards against false positive on first reading

        // Barometer state: read once only to prevent recommendation jumping
        private bool _barometerInitialized = false;

        /// <summary>
        /// Initialises the HomePage with dependency-injected view model.
        /// </summary>
        /// <param name="viewModel">The view model providing recipe data and sensor commands</param>
        public HomePage(HomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        /// <summary>
        /// Called when the page becomes visible.
        /// Loads recipes and starts all three hardware sensors.
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadRecipesAsync();
            StartBarometer();
            StartShakeDetection();
            StartCompass();
        }

        /// <summary>
        /// Called when the page is hidden or navigated away from.
        /// Stops all sensors to conserve battery and prevent memory leaks.
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopBarometer();
            StopShakeDetection();
            StopCompass();
        }

        /// <summary>Set category filter to All and reload recipe list</summary>
        private async void OnAllCategoryClicked(object sender, EventArgs e)
        {
            _viewModel.SelectedCategory = "All";
            await _viewModel.LoadRecipesAsync();
        }

        /// <summary>Set category filter to Food and reload recipe list</summary>
        private async void OnFoodCategoryClicked(object sender, EventArgs e)
        {
            _viewModel.SelectedCategory = "Food";
            await _viewModel.LoadRecipesAsync();
        }

        /// <summary>Set category filter to Drink and reload recipe list</summary>
        private async void OnDrinkCategoryClicked(object sender, EventArgs e)
        {
            _viewModel.SelectedCategory = "Drink";
            await _viewModel.LoadRecipesAsync();
        }

        /// <summary>
        /// Navigate to the Settings page when the help (?) button is tapped.
        /// Provides quick access to usage instructions (WCAG 3.3.2).
        /// </summary>
        private async void OnHelpClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SettingsPage");
        }

        // ==================== Barometer Hardware ====================

        /// <summary>
        /// Start monitoring the barometer sensor.
        /// Reading is taken once only (_barometerInitialized guard) to produce
        /// a stable weather-based recipe recommendation without jumping.
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

        /// <summary>Stop the barometer sensor and unsubscribe from events</summary>
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
        /// Handle the first barometer reading and request a recipe recommendation.
        /// The _barometerInitialized flag ensures this fires only once per page visit,
        /// preventing the recommendation from changing while the user is browsing.
        /// </summary>
        private async void OnBarometerReadingChanged(object sender, BarometerChangedEventArgs e)
        {
            if (_barometerInitialized) return;
            _barometerInitialized = true;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _viewModel.UpdateBarometerRecommendationAsync(e.Reading.PressureInHectopascals);
            });
        }

        // ==================== Shake Detection (Manual via Accelerometer) ====================

        /// <summary>
        /// Start the accelerometer for manual shake detection.
        /// Ensures no duplicate subscriptions by stopping any existing monitoring first.
        /// </summary>
        private void StartShakeDetection()
        {
            try
            {
                if (Accelerometer.Default.IsSupported)
                {
                    if (Accelerometer.Default.IsMonitoring)
                    {
                        Accelerometer.Default.ReadingChanged -= OnAccelerometerForShake;
                        Accelerometer.Default.Stop();
                    }
                    Accelerometer.Default.ReadingChanged += OnAccelerometerForShake;
                    Accelerometer.Default.Start(SensorSpeed.Game);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shake start error: {ex.Message}");
            }
        }

        /// <summary>Stop the accelerometer and unsubscribe shake detection handler</summary>
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
        /// Detect shake by measuring the total acceleration force change between readings.
        /// Triggers coupon generation when force exceeds the threshold (1.8g),
        /// with a 2-second cooldown to prevent duplicate triggers.
        /// Uses manual calculation for reliable detection on HarmonyOS devices.
        /// </summary>
        private void OnAccelerometerForShake(object sender, AccelerometerChangedEventArgs e)
        {
            try
            {
                double x = e.Reading.Acceleration.X;
                double y = e.Reading.Acceleration.Y;
                double z = e.Reading.Acceleration.Z;

                if (_shakeInitialized)
                {
                    // Calculate combined force change across all axes
                    double force = Math.Abs(x + y + z - _lastX - _lastY - _lastZ);

                    // Trigger coupon if force threshold exceeded and cooldown has passed
                    if (force > 1.8 && (DateTime.Now - _lastShakeTime).TotalSeconds > 2)
                    {
                        _lastShakeTime = DateTime.Now;
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await _viewModel.ShakeDetectedAsync();
                        });
                    }
                }

                // Store current readings for next comparison
                _lastX = x;
                _lastY = y;
                _lastZ = z;
                _shakeInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shake detection error: {ex.Message}");
            }
        }

        // ==================== Compass Hardware ====================

        /// <summary>
        /// Start the compass sensor for the Surprise Me spinning wheel.
        /// Real magnetic heading data drives the wheel rotation animation in real-time.
        /// </summary>
        private void StartCompass()
        {
            try
            {
                if (Compass.Default.IsSupported && !Compass.Default.IsMonitoring)
                {
                    Compass.Default.ReadingChanged += OnCompassReadingChanged;
                    Compass.Default.Start(SensorSpeed.UI);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Compass start error: {ex.Message}");
            }
        }

        /// <summary>Stop the compass sensor and unsubscribe from events</summary>
        private void StopCompass()
        {
            try
            {
                if (Compass.Default.IsSupported && Compass.Default.IsMonitoring)
                {
                    Compass.Default.ReadingChanged -= OnCompassReadingChanged;
                    Compass.Default.Stop();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Compass stop error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update the compass heading in the view model on every sensor reading.
        /// The heading value is bound to the wheel rotation in XAML,
        /// creating a live sensor-driven animation.
        /// </summary>
        private void OnCompassReadingChanged(object sender, CompassChangedEventArgs e)
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _viewModel.CompassHeading = e.Reading.HeadingMagneticNorth;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Compass reading error: {ex.Message}");
            }
        }
    }
}