using TasteHub.ViewModels;

namespace TasteHub.Views
{
    public partial class InteractivePage : ContentPage
    {
        private readonly InteractiveViewModel _viewModel;

        public InteractivePage(InteractiveViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartAccelerometer();
            StartGyroscope();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopAccelerometer();
            StopGyroscope();
        }

        private void StartAccelerometer()
        {
            try
            {
                if (Accelerometer.Default.IsSupported && !Accelerometer.Default.IsMonitoring)
                {
                    Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
                    Accelerometer.Default.ShakeDetected += OnShakeDetected;
                    Accelerometer.Default.Start(SensorSpeed.Game);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Accelerometer start error: {ex.Message}");
            }
        }

        private void StopAccelerometer()
        {
            try
            {
                if (Accelerometer.Default.IsSupported && Accelerometer.Default.IsMonitoring)
                {
                    Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
                    Accelerometer.Default.ShakeDetected -= OnShakeDetected;
                    Accelerometer.Default.Stop();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Accelerometer stop error: {ex.Message}");
            }
        }

        private void OnAccelerometerReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _viewModel.UpdateAccelerometerCommand.Execute(e.Reading.Acceleration.Z);
            });
        }

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

        private void OnGyroscopeReadingChanged(object sender, GyroscopeChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _viewModel.UpdateGyroscopeCommand.Execute(e.Reading.AngularVelocity.Z);
            });
        }

        private void OnShakeDetected(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _viewModel.ShakeDetectedCommand.Execute(null);
            });
        }
    }
}