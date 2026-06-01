using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;

namespace TasteHub
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
        ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        /// <summary>
        /// Handle results from camera and other activity intents.
        /// When the camera returns a photo, save the bitmap to local storage
        /// and notify the CameraService.
        /// </summary>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 1002)
            {
                if (resultCode == Result.Ok && data != null && data.Extras != null)
                {
                    try
                    {
                        var bitmap = (Bitmap)data.Extras.Get("data");
                        if (bitmap != null)
                        {
                            string fileName = $"recipe_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                            string filePath = System.IO.Path.Combine(
                                FileSystem.AppDataDirectory, fileName);

                            using (var stream = System.IO.File.Create(filePath))
                            {
                                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, stream);
                            }

                            bitmap.Recycle();
                            Services.CameraService.OnPhotoReceived(filePath);
                        }
                        else
                        {
                            Services.CameraService.OnPhotoCancelled();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Services.CameraService.OnPhotoError(ex.Message);
                    }
                }
                else
                {
                    Services.CameraService.OnPhotoCancelled();
                }
            }
        }
    }
}