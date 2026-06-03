using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;

namespace TasteHub
{
    /// <summary>
    /// Main Android activity for TasteHub, configured as the app launcher.
    /// Handles the Android activity lifecycle and processes results from
    /// camera intents launched by the AddEditPage for recipe cover photos.
    /// 
    /// ConfigurationChanges are declared to prevent activity restarts on
    /// screen rotation, orientation change and UI mode change, ensuring
    /// a smooth user experience on foldable devices (HUAWEI Mate X5).
    /// </summary>
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
        ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        /// <summary>
        /// Handle results from camera and other activity intents.
        /// When the camera returns a photo (requestCode 1002), saves the bitmap
        /// to local app storage as a JPEG and notifies CameraService.
        /// 
        /// This override is required because HarmonyOS does not support
        /// the MAUI MediaPicker.CapturePhotoAsync API, so a native Android
        /// ACTION_IMAGE_CAPTURE intent is used instead via CameraService.
        /// </summary>
        /// <param name="requestCode">Identifies which intent returned (1002 = camera)</param>
        /// <param name="resultCode">Whether the activity completed successfully or was cancelled</param>
        /// <param name="data">Intent data containing the captured photo bitmap in extras</param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Handle camera capture result
            if (requestCode == 1002)
            {
                if (resultCode == Result.Ok && data != null && data.Extras != null)
                {
                    try
                    {
                        // Extract the thumbnail bitmap from the intent extras
                        var bitmap = (Bitmap)data.Extras.Get("data");
                        if (bitmap != null)
                        {
                            // Save bitmap to app data directory with timestamp filename
                            string fileName = $"recipe_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                            string filePath = System.IO.Path.Combine(
                                FileSystem.AppDataDirectory, fileName);

                            using (var stream = System.IO.File.Create(filePath))
                            {
                                // Compress to JPEG at 90% quality to balance size and clarity
                                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, stream);
                            }

                            // Free native bitmap memory immediately after saving
                            bitmap.Recycle();

                            // Notify CameraService so the awaiting ViewModel Task completes
                            Services.CameraService.OnPhotoReceived(filePath);
                        }
                        else
                        {
                            Services.CameraService.OnPhotoCancelled();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        // Pass error to CameraService so the awaiting Task faults gracefully
                        Services.CameraService.OnPhotoError(ex.Message);
                    }
                }
                else
                {
                    // User cancelled the camera without taking a photo
                    Services.CameraService.OnPhotoCancelled();
                }
            }
        }
    }
}