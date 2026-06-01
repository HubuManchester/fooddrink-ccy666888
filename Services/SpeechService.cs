using System;
using System.Threading.Tasks;

namespace TasteHub.Services
{
    /// <summary>
    /// Service to handle speech recognition results.
    /// Uses TaskCompletionSource to bridge Android callback to async/await.
    /// </summary>
    public static class SpeechService
    {
        private static TaskCompletionSource<string> _tcs;

        /// <summary>
        /// Start waiting for speech recognition result
        /// </summary>
        public static Task<string> WaitForResultAsync()
        {
            _tcs = new TaskCompletionSource<string>();
            return _tcs.Task;
        }

        /// <summary>
        /// Called when speech is successfully recognised
        /// </summary>
        public static void OnResult(string text)
        {
            _tcs?.TrySetResult(text);
        }

        /// <summary>
        /// Called when speech recognition fails or is cancelled
        /// </summary>
        public static void OnError(string error)
        {
            _tcs?.TrySetResult(null);
        }
    }
}