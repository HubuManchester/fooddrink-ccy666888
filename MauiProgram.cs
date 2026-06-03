using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TasteHub.Services;
using TasteHub.ViewModels;
using TasteHub.Views;

namespace TasteHub
{
    /// <summary>
    /// The main entry point and configuration class for the TasteHub MAUI application.
    /// Handles the registration of services, ViewModels, Views, and platform-specific UI configurations.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Configures and builds the .NET MAUI application instance.
        /// </summary>
        /// <returns>A fully configured <see cref="MauiApp"/> instance.</returns>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            // 1. Core Application & Toolkit Configuration
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            // Enable debug logging only during development for performance optimization
            builder.Logging.AddDebug();
#endif

            // 2. Dependency Injection (DI) Container Registration
            // Registering services as Singleton ensures a single, globally reusable instance (e.g., shared database connection)
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

            // Registering ViewModels and Pages as Transient ensures a fresh instance per request, preventing unintended state leakage
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<DetailViewModel>();
            builder.Services.AddTransient<AddEditViewModel>();
            builder.Services.AddTransient<InteractiveViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<DetailPage>();
            builder.Services.AddTransient<AddEditPage>();
            builder.Services.AddTransient<InteractivePage>();
            builder.Services.AddTransient<SettingsPage>();

            // 3. Platform-Specific UI Customizations (Global Handlers)
            // Applying global mappers to enhance UI consistency and reusability across the entire application

            // Remove the default underline from SearchBar on Android for a cleaner UI
            Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            // Remove the default underline from Entry (Text Input) on Android
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            // Remove the default underline from Editor (Multiline Text Input) on Android
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            // Remove the default underline from Picker on Android
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            // Apply custom branding colors (Green/Gray) to the Switch control on Android
            Microsoft.Maui.Handlers.SwitchHandler.Mapper.AppendToMapping("GreenSwitch", (handler, view) =>
            {
#if ANDROID
                // Define state arrays for Checked and Unchecked configurations
                var states = new int[][] {
                    new int[] { Android.Resource.Attribute.StateChecked },
                    new int[] { -Android.Resource.Attribute.StateChecked }
                };

                // Set custom Thumb (the interactive circle) colors based on state
                var thumbColors = new int[] {
                    new Android.Graphics.Color(0x7B, 0xA3, 0x8E).ToArgb(), // Checked state: Custom Green
                    new Android.Graphics.Color(0xB0, 0xB0, 0xB0).ToArgb()  // Unchecked state: Gray
                };

                // Set custom Track (the background) colors based on state
                var trackColors = new int[] {
                    new Android.Graphics.Color(0xA3, 0xC9, 0xB3).ToArgb(), // Checked state: Light Green
                    new Android.Graphics.Color(0x60, 0x60, 0x60).ToArgb()  // Unchecked state: Dark Gray
                };

                // Apply the custom color state lists to the native Android view
                handler.PlatformView.ThumbTintList = new Android.Content.Res.ColorStateList(states, thumbColors);
                handler.PlatformView.TrackTintList = new Android.Content.Res.ColorStateList(states, trackColors);
#endif
            });

            return builder.Build();
        }
    }
}