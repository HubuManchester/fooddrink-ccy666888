using System;
using System.Threading.Tasks;

namespace TasteHub.Services
{
    /// <summary>
    /// Static service that bridges the native Android camera intent result
    /// back to the MAUI ViewModel layer using a TaskCompletionSource pattern.
    /// 
    /// This approach is required on HarmonyOS (HUAWEI) devices because
    /// MediaPicker.CapturePhotoAsync() is not supported. Instead, the app
    /// launches a native Android ACTION_IMAGE_CAPTURE intent and waits for
    /// the result via this service.
    /// 
    /// Flow: ViewModel calls WaitForPhotoAsync() -> launches camera intent
    ///       -> MainActivity.OnActivityResult() calls OnPhotoReceived()
    ///       -> awaited Task completes with the saved file path.
    /// </summary>
    public static class CameraService
    {
        /// <summary>TaskCompletionSource used to bridge the async gap between the camera intent callback and the awaiting ViewModel</summary>
        private static TaskCompletionSource<string> _tcs;

        /// <summary>Most recently captured or picked photo file path</summary>
        private static string _photoPath;

        /// <summary>
        /// Store the file path of a captured or picked photo for retrieval
        /// </summary>
        /// <param name="path">Full local file path of the saved photo</param>
        public static void SetPhotoPath(string path)
        {
            _photoPath = path;
        }

        /// <summary>
        /// Retrieve the most recently stored photo file path
        /// </summary>
        /// <returns>Full local file path of the saved photo</returns>
        public static string GetPhotoPath()
        {
            return _photoPath;
        }

        /// <summary>
        /// Create a new TaskCompletionSource and return its Task.
        /// The ViewModel awaits this Task; it completes when OnPhotoReceived,
        /// OnPhotoCancelled or OnPhotoError is called by the Android activity result handler.
        /// </summary>
        /// <returns>Task that resolves to the photo file path, or null if cancelled</returns>
        public static Task<string> WaitForPhotoAsync()
        {
            _tcs = new TaskCompletionSource<string>();
            return _tcs.Task;
        }

        /// <summary>
        /// Called by MainActivity.OnActivityResult when the camera intent returns successfully.
        /// Resolves the waiting Task with the saved photo file path.
        /// </summary>
        /// <param name="filePath">Full local path of the captured photo</param>
        public static void OnPhotoReceived(string filePath)
        {
            _tcs?.TrySetResult(filePath);
        }

        /// <summary>
        /// Called when the user cancels the camera intent without taking a photo.
        /// Resolves the waiting Task with null so the caller can handle gracefully.
        /// </summary>
        public static void OnPhotoCancelled()
        {
            _tcs?.TrySetResult(null);
        }

        /// <summary>
        /// Called when the camera intent encounters an error.
        /// Faults the waiting Task with an exception containing the error message.
        /// </summary>
        /// <param name="error">Error description from the Android activity result</param>
        public static void OnPhotoError(string error)
        {
            _tcs?.TrySetException(new Exception(error));
        }
    }
}