using TasteHub.ViewModels;

namespace TasteHub.Views
{
    /// <summary>
    /// Detail page showing full recipe information with TTS and pinch-to-zoom.
    /// TTS uses Android native engine directly for reliable stop support.
    /// </summary>
    public partial class DetailPage : ContentPage
    {
        private readonly DetailViewModel _viewModel;
        private double _currentScale = 1;
        private double _startScale = 1;
        private bool _isSpeaking = false;

#if ANDROID
        private Android.Speech.Tts.TextToSpeech _tts;
        private bool _ttsReady = false;
#endif

        public DetailPage(DetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;

#if ANDROID
            // Initialise Android native TTS engine
            _tts = new Android.Speech.Tts.TextToSpeech(
                Android.App.Application.Context,
                new TtsInitListener(this));
#endif
        }

#if ANDROID
        /// <summary>
        /// Called when Android TTS engine is ready
        /// </summary>
        public void SetTtsReady(bool ready)
        {
            _ttsReady = ready;
        }

        /// <summary>
        /// Android TTS initialisation listener
        /// </summary>
        private class TtsInitListener : Java.Lang.Object,
            Android.Speech.Tts.TextToSpeech.IOnInitListener
        {
            private readonly DetailPage _page;

            public TtsInitListener(DetailPage page)
            {
                _page = page;
            }

            public void OnInit(Android.Speech.Tts.OperationResult status)
            {
                _page.SetTtsReady(
                    status == Android.Speech.Tts.OperationResult.Success);
            }
        }
#endif

        /// <summary>
        /// Reload recipe data every time the page appears
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
        /// Stop TTS when leaving the page
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopSpeaking();
        }

        /// <summary>
        /// Handle TTS button click.
        /// If not speaking: start reading all steps one by one.
        /// If speaking: stop immediately.
        /// </summary>
        private async void OnTtsButtonClicked(object sender, EventArgs e)
        {
            if (_isSpeaking)
            {
                // STOP immediately
                StopSpeaking();
                return;
            }

            // START reading steps
            await StartReadingSteps();
        }

        /// <summary>
        /// Stop TTS immediately and reset button text
        /// </summary>
        private void StopSpeaking()
        {
            _isSpeaking = false;
            _viewModel.CurrentReadingStep = -1;
            _viewModel.IsReading = false;
            TtsButton.Text = "🔊 Read Aloud";

#if ANDROID
            try
            {
                if (_tts != null)
                {
                    _tts.Stop();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TTS stop error: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// Read all steps sequentially using Android native TTS.
        /// Polls IsSpeaking to wait for each step to finish.
        /// Checks _isSpeaking flag to allow immediate cancellation.
        /// </summary>
        private async Task StartReadingSteps()
        {
            var steps = _viewModel.StepsList;
            if (steps == null || steps.Count == 0) return;

#if ANDROID
            if (!_ttsReady || _tts == null)
            {
                await DisplayAlert("Error",
                    "Text-to-speech is not ready. Please try again.", "OK");
                return;
            }
#endif

            _isSpeaking = true;
            _viewModel.IsReading = true;
            TtsButton.Text = "⏹ Stop";

            try
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    // Check if user pressed Stop
                    if (!_isSpeaking) break;

                    _viewModel.CurrentReadingStep = i;
                    string text = $"Step {i + 1}: {steps[i]}";

#if ANDROID
                    // Speak using native Android TTS (non-blocking)
                    _tts.Speak(text,
                        Android.Speech.Tts.QueueMode.Flush,
                        null, $"step_{i}");

                    // Wait a moment for speech to start
                    await Task.Delay(200);

                    // Poll until this step finishes speaking or user stops
                    while (_tts.IsSpeaking)
                    {
                        if (!_isSpeaking)
                        {
                            _tts.Stop();
                            break;
                        }
                        await Task.Delay(50);
                    }
#else
                    await TextToSpeech.Default.SpeakAsync(text);
#endif

                    // Check again after speech completes
                    if (!_isSpeaking) break;

                    // Small pause between steps
                    await Task.Delay(300);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error",
                    "Text-to-speech failed. Please try again.", "OK");
                System.Diagnostics.Debug.WriteLine($"TTS error: {ex.Message}");
            }
            finally
            {
                _isSpeaking = false;
                _viewModel.IsReading = false;
                _viewModel.CurrentReadingStep = -1;
                TtsButton.Text = "🔊 Read Aloud";
            }
        }

        /// <summary>
        /// Handle pinch gesture to zoom the recipe cover image
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

                // Stop TTS before navigating away
                StopSpeaking();

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