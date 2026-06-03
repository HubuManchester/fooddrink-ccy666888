using TasteHub.ViewModels;

namespace TasteHub.Views
{
    /// <summary>
    /// Detail page showing full recipe information with TTS, 
    /// double-tap zoom gesture and zoom buttons.
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

        // ==================== Image Zoom ====================

        /// <summary>
        /// Handle double-tap gesture on the recipe image.
        /// Toggles between 1x (original) and 2.5x (zoomed) scale.
        /// Provides a quick gesture-based zoom alternative to buttons.
        /// </summary>
        private void OnImageDoubleTapped(object sender, TappedEventArgs e)
        {
            try
            {
                if (_currentScale > 1.0)
                {
                    _currentScale = 1.0;
                }
                else
                {
                    _currentScale = 2.5;
                }
                RecipeImage.Scale = _currentScale;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DoubleTap zoom error: {ex.Message}");
            }
        }

        /// <summary>
        /// Zoom in the recipe image by 0.5x, maximum 4x
        /// </summary>
        private void OnZoomInClicked(object sender, EventArgs e)
        {
            try
            {
                _currentScale = Math.Min(_currentScale + 0.5, 4.0);
                RecipeImage.Scale = _currentScale;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ZoomIn error: {ex.Message}");
            }
        }

        /// <summary>
        /// Zoom out the recipe image by 0.5x, minimum 1x
        /// </summary>
        private void OnZoomOutClicked(object sender, EventArgs e)
        {
            try
            {
                _currentScale = Math.Max(_currentScale - 0.5, 1.0);
                RecipeImage.Scale = _currentScale;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ZoomOut error: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset the recipe image zoom to original 1x scale
        /// </summary>
        private void OnZoomResetClicked(object sender, EventArgs e)
        {
            try
            {
                _currentScale = 1.0;
                RecipeImage.Scale = _currentScale;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ZoomReset error: {ex.Message}");
            }
        }

        // ==================== Text-to-Speech ====================

        /// <summary>
        /// Handle TTS button click.
        /// If not speaking: start reading all steps.
        /// If speaking: stop immediately.
        /// </summary>
        private async void OnTtsButtonClicked(object sender, EventArgs e)
        {
            if (_isSpeaking)
            {
                StopSpeaking();
                return;
            }
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
            try { _tts?.Stop(); } catch { }
#endif
        }

        /// <summary>
        /// Read all steps using Android native TTS with polling for stop support
        /// </summary>
        private async Task StartReadingSteps()
        {
            var steps = _viewModel.StepsList;
            if (steps == null || steps.Count == 0) return;

#if ANDROID
            if (!_ttsReady || _tts == null)
            {
                await DisplayAlert("Error", "Text-to-speech is not ready.", "OK");
                return;
            }
#endif

            _isSpeaking = true;
            _viewModel.IsReading = true;
            TtsButton.Text = "Stop";

            try
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    if (!_isSpeaking) break;
                    _viewModel.CurrentReadingStep = i;
                    string text = $"Step {i + 1}: {steps[i]}";

#if ANDROID
                    _tts.Speak(text, Android.Speech.Tts.QueueMode.Flush, null, $"step_{i}");
                    await Task.Delay(200);
                    while (_tts.IsSpeaking)
                    {
                        if (!_isSpeaking) { _tts.Stop(); break; }
                        await Task.Delay(50);
                    }
#else
                    await TextToSpeech.Default.SpeakAsync(text);
#endif

                    if (!_isSpeaking) break;
                    await Task.Delay(300);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Text-to-speech failed.", "OK");
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

        // ==================== Navigation ====================

        /// <summary>
        /// Navigate to interactive cooking page
        /// </summary>
        private async void OnCookClicked(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel.Recipe == null) return;
                StopSpeaking();
                await Shell.Current.GoToAsync(
                    $"InteractivePage?name={Uri.EscapeDataString(_viewModel.Recipe.Name)}&category={_viewModel.Recipe.Category}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to open interactive cooking mode.", "OK");
                System.Diagnostics.Debug.WriteLine($"OnCookClicked error: {ex.Message}");
            }
        }
    }
}