using Android.OS;
using Android.Speech;
using System.Collections.Generic;
using TasteHub.Services;

namespace TasteHub.Platforms.Android
{
    /// <summary>
    /// Android RecognitionListener implementation that receives speech recognition
    /// callbacks and forwards the result to SpeechService.
    /// </summary>
    public class SpeechRecognitionListener : Java.Lang.Object, IRecognitionListener
    {
        /// <summary>Called when speech recognition returns results</summary>
        public void OnResults(Bundle results)
        {
            var matches = results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches != null && matches.Count > 0)
            {
                SpeechService.OnResult(matches[0]);
            }
            else
            {
                SpeechService.OnError("No speech detected");
            }
        }

        /// <summary>Called when speech recognition encounters an error</summary>
        public void OnError(SpeechRecognizerError error)
        {
            SpeechService.OnError($"Speech recognition error: {error}");
        }

        /// <summary>Called when partial results are available</summary>
        public void OnPartialResults(Bundle partialResults) { }

        /// <summary>Called when the user starts speaking</summary>
        public void OnBeginningOfSpeech() { }

        /// <summary>Called when the speech input is complete</summary>
        public void OnEndOfSpeech() { }

        /// <summary>Called when the recogniser is ready</summary>
        public void OnReadyForSpeech(Bundle @params) { }

        /// <summary>Called when the sound level changes</summary>
        public void OnRmsChanged(float rmsdB) { }

        /// <summary>Called when buffered results are available</summary>
        public void OnBufferReceived(byte[] buffer) { }

        /// <summary>Called when a recognition event occurs</summary>
        public void OnEvent(int eventType, Bundle @params) { }
    }
}