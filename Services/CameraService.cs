using System;
using System.IO;
using System.Threading.Tasks;

namespace TasteHub.Services
{
    /// <summary>
    /// Service to handle camera photo capture results across Android activities.
    /// Uses a static TaskCompletionSource to bridge the Android activity result
    /// back to the awaiting MAUI code.
    /// </summary>
    public static class CameraService
    {
        private static TaskCompletionSource<string> _tcs;

        /// <summary>
        /// Start waiting for a camera result
        /// </summary>
        public static Task<string> WaitForPhotoAsync()
        {
            _tcs = new TaskCompletionSource<string>();
            return _tcs.Task;
        }

        /// <summary>
        /// Called from MainActivity when camera returns a photo.
        /// Saves the bitmap to app storage and returns the file path.
        /// </summary>
        public static void OnPhotoReceived(string filePath)
        {
            _tcs?.TrySetResult(filePath);
        }

        /// <summary>
        /// Called when camera is cancelled by the user
        /// </summary>
        public static void OnPhotoCancelled()
        {
            _tcs?.TrySetResult(null);
        }

        /// <summary>
        /// Called when camera encounters an error
        /// </summary>
        public static void OnPhotoError(string error)
        {
            _tcs?.TrySetException(new Exception(error));
        }
    }
}