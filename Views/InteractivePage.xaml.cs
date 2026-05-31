using TasteHub.ViewModels;

namespace TasteHub.Views
{
    /// <summary>
    /// Interactive cooking page using accelerometer, gyroscope and shake
    /// to simulate the cooking process
    /// </summary>
    public partial class InteractivePage : ContentPage
    {
        private readonly InteractiveViewModel _viewModel;

        // Manual shake detection variables
        private double _lastX, _lastY, _lastZ;
        private bool _shakeInitialized = false;
        private DateTime _lastShakeTime = DateTime.MinValue;

        public InteractivePage(InteractiveViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        /// <summary>
        /// Start hardware sensors when page appears
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartAccelerometer();
            StartGyroscope();
        }

        /// <summary>
        /// Stop all hardware sensors when page disappears
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopAccelerometer();
            StopGyroscope();
        }

        // ==================== Accelerometer ====================

        /// <summary>
        /// Start the accelerometer sensor for tilt-to-pour and shake detection
        /// </summary>
        private void StartAccelerometer()
        {
            try
            {
                if (Accelerometer.Default.IsSupported && !Accelerometer.Default.IsMonitoring)
                {
                    Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
                    Accelerometer.Default.Start(SensorSpeed.Game);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Accelerometer start error: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop the accelerometer sensor
        /// </summary>
        private void StopAccelerometer()
        {
            try
            {
                if (Accelerometer.Default.IsSupported && Accelerometer.Default.IsMonitoring)
                {
                    Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
                    Accelerometer.Default.Stop();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Accelerometer stop error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle accelerometer readings for both tilt-to-pour and manual shake detection
        /// </summary>
        private void OnAccelerometerReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            try
            {
                double x = e.Reading.Acceleration.X;
                double y = e.Reading.Acceleration.Y;
                double z = e.Reading.Acceleration.Z;

                // Update tilt-to-pour via ViewModel
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _viewModel.UpdateAccelerometerCommand.Execute(z);
                });

                // Manual shake detection
                if (_shakeInitialized)
                {
                    double force = Math.Abs(x + y + z - _lastX - _lastY - _lastZ);
                    if (force > 2.5 && (DateTime.Now - _lastShakeTime).TotalSeconds > 1)
                    {
                        _lastShakeTime = DateTime.Now;
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            _viewModel.ShakeDetectedCommand.Execute(null);
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
                System.Diagnostics.Debug.WriteLine($"Accelerometer reading error: {ex.Message}");
            }
        }

        // ==================== Gyroscope ====================

        /// <summary>
        /// Start the gyroscope sensor for rotate-to-stir feature
        /// </summary>
        private void StartGyroscope()
        {
            try
            {
                if (Gyroscope.Default.IsSupported && !Gyroscope.Default.IsMonitoring)
                {
                    Gyroscope.Default.ReadingChanged += OnGyroscopeReadingChanged;
                    Gyroscope.Default.Start(SensorSpeed.Game);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gyroscope start error: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop the gyroscope sensor
        /// </summary>
        private void StopGyroscope()
        {
            try
            {
                if (Gyroscope.Default.IsSupported && Gyroscope.Default.IsMonitoring)
                {
                    Gyroscope.Default.ReadingChanged -= OnGyroscopeReadingChanged;
                    Gyroscope.Default.Stop();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gyroscope stop error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle gyroscope readings and pass Z-axis angular velocity to view model
        /// </summary>
        private void OnGyroscopeReadingChanged(object sender, GyroscopeChangedEventArgs e)
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _viewModel.UpdateGyroscopeCommand.Execute(e.Reading.AngularVelocity.Z);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gyroscope reading error: {ex.Message}");
            }
        }
    }
}