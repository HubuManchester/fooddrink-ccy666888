using System;
using System.Threading.Tasks;

namespace TasteHub.Services
{
    public static class CameraService
    {
        private static TaskCompletionSource<string> _tcs;
        private static string _photoPath;

        public static void SetPhotoPath(string path)
        {
            _photoPath = path;
        }

        public static string GetPhotoPath()
        {
            return _photoPath;
        }

        public static Task<string> WaitForPhotoAsync()
        {
            _tcs = new TaskCompletionSource<string>();
            return _tcs.Task;
        }

        public static void OnPhotoReceived(string filePath)
        {
            _tcs?.TrySetResult(filePath);
        }

        public static void OnPhotoCancelled()
        {
            _tcs?.TrySetResult(null);
        }

        public static void OnPhotoError(string error)
        {
            _tcs?.TrySetException(new Exception(error));
        }
    }
}