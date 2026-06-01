using System;
using System.Threading.Tasks;

namespace TasteHub.Services
{
    /// <summary>
    /// Cross-platform text-to-speech service with stop capability.
    /// Uses Android native TextToSpeech API for reliable stop functionality.
    /// </summary>
    public class TtsService
    {
#if ANDROID
        private Android.Speech.Tts.TextToSpeech _tts;
        private TaskCompletionSource<bool> _initTcs;
        private TaskCompletionSource<bool> _speakTcs;
        private bool _isInitialized = false;

        /// <summary>
        /// Initialise the Android native TTS engine
        /// </summary>
        public Task InitAsync()
        {
            _initTcs = new TaskCompletionSource<bool>();

            _tts = new Android.Speech.Tts.TextToSpeech(
                Android.App.Application.Context,
                new TtsInitListener(_initTcs));

            return _initTcs.Task;
        }

        /// <summary>
        /// Speak the given text. Returns when speech is complete or stopped.
        /// </summary>
        public async Task SpeakAsync(string text)
        {
            if (_tts == null || !_isInitialized)
            {
                await InitAsync();
                _isInitialized = true;
            }

            _speakTcs = new TaskCompletionSource<bool>();

            _tts.SetOnUtteranceProgressListener(new TtsProgressListener(_speakTcs));

            var parameters = new Android.OS.Bundle();
            _tts.Speak(text, Android.Speech.Tts.QueueMode.Flush, parameters, "tastehub_utterance");

            await _speakTcs.Task;
        }

        /// <summary>
        /// Immediately stop any ongoing speech
        /// </summary>
        public void Stop()
        {
            try
            {
                _tts?.Stop();
                _speakTcs?.TrySetResult(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TTS Stop error: {ex.Message}");
            }
        }

        /// <summary>
        /// Release TTS resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                _tts?.Stop();
                _tts?.Shutdown();
                _tts = null;
                _isInitialized = false;
            }
            catch { }
        }

        /// <summary>
        /// Listener for TTS engine initialisation
        /// </summary>
        private class TtsInitListener : Java.Lang.Object, Android.Speech.Tts.TextToSpeech.IOnInitListener
        {
            private readonly TaskCompletionSource<bool> _tcs;

            public TtsInitListener(TaskCompletionSource<bool> tcs)
            {
                _tcs = tcs;
            }

            public void OnInit(Android.Speech.Tts.OperationResult status)
            {
                _tcs.TrySetResult(status == Android.Speech.Tts.OperationResult.Success);
            }
        }

        /// <summary>
        /// Listener for TTS utterance progress (completion/error)
        /// </summary>
        private class TtsProgressListener : Android.Speech.Tts.UtteranceProgressListener
        {
            private readonly TaskCompletionSource<bool> _tcs;

            public TtsProgressListener(TaskCompletionSource<bool> tcs)
            {
                _tcs = tcs;
            }

            public override void OnDone(string utteranceId)
            {
                _tcs.TrySetResult(true);
            }

            public override void OnError(string utteranceId)
            {
                _tcs.TrySetResult(false);
            }

            public override void OnStart(string utteranceId) { }
        }
#else
        public Task InitAsync() => Task.CompletedTask;

        public async Task SpeakAsync(string text)
        {
            await TextToSpeech.Default.SpeakAsync(text);
        }

        public void Stop() { }
        public void Dispose() { }
#endif
    }
}