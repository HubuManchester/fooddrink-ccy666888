using TasteHub.ViewModels;

namespace TasteHub.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _viewModel;
        private DateTime _lastShakeTime = DateTime.MinValue;
        private double _lastX, _lastY, _lastZ;
        private bool _shakeInitialized = false;
        private bool _barometerInitialized = false;

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
            StartCompass();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopBarometer();
            StopShakeDetection();
            StopCompass();
        }

        private async void OnAllCategoryClicked(object sender, EventArgs e)
        {
            _viewModel.SelectedCategory = "All";
            await _viewModel.LoadRecipesAsync();
        }

        private async void OnFoodCategoryClicked(object sender, EventArgs e)
        {
            _viewModel.SelectedCategory = "Food";
            await _viewModel.LoadRecipesAsync();
        }

        private async void OnDrinkCategoryClicked(object sender, EventArgs e)
        {
            _viewModel.SelectedCategory = "Drink";
            await _viewModel.LoadRecipesAsync();
        }

        private async void OnHelpClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SettingsPage");
        }

        // ==================== Barometer ====================

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
            if (_barometerInitialized) return;
            _barometerInitialized = true;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _viewModel.UpdateBarometerRecommendationAsync(e.Reading.PressureInHectopascals);
            });
        }

        // ==================== Shake Detection ====================

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

        private void OnAccelerometerForShake(object sender, AccelerometerChangedEventArgs e)
        {
            try
            {
                double x = e.Reading.Acceleration.X;
                double y = e.Reading.Acceleration.Y;
                double z = e.Reading.Acceleration.Z;

                if (_shakeInitialized)
                {
                    double force = Math.Abs(x + y + z - _lastX - _lastY - _lastZ);

                    if (force > 1.8 && (DateTime.Now - _lastShakeTime).TotalSeconds > 2)
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Shake detection error: {ex.Message}");
            }
        }

        // ==================== Compass ====================

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