using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TasteHub.Services;
using TasteHub.ViewModels;
using TasteHub.Views;

namespace TasteHub
{
    /// <summary>
    /// Main entry point for the MAUI application,
    /// configuring services, view models and pages via dependency injection
    /// </summary>
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Register Services
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

            // Register ViewModels
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<DetailViewModel>();
            builder.Services.AddTransient<AddEditViewModel>();
            builder.Services.AddTransient<InteractiveViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

            // Register Pages
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<DetailPage>();
            builder.Services.AddTransient<AddEditPage>();
            builder.Services.AddTransient<InteractivePage>();
            builder.Services.AddTransient<SettingsPage>();

            return builder.Build();
        }
    }
}