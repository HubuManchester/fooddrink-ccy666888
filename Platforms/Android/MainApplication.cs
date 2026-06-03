using Android.App;
using Android.Runtime;

namespace TasteHub
{
    /// <summary>
    /// Android application entry point for the TasteHub MAUI app.
    /// Inherits from MauiApplication to integrate the .NET MAUI framework
    /// with the native Android application lifecycle.
    /// </summary>
    [Application]
    public class MainApplication : MauiApplication
    {
        /// <summary>
        /// Constructor called by the Android runtime when the application process starts.
        /// </summary>
        /// <param name="handle">Native Android handle for the Java object</param>
        /// <param name="ownership">JNI handle ownership semantics</param>
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        /// <summary>
        /// Creates and returns the MAUI application instance.
        /// Called by the MauiApplication base class during app initialisation.
        /// Delegates to MauiProgram.CreateMauiApp() where all services,
        /// view models and custom handlers are registered.
        /// </summary>
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}