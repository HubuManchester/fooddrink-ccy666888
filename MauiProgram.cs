using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TasteHub.Services;
using TasteHub.ViewModels;
using TasteHub.Views;

namespace TasteHub
{
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

            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
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

            Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

            Microsoft.Maui.Handlers.SwitchHandler.Mapper.AppendToMapping("GreenSwitch", (handler, view) =>
            {
#if ANDROID
                var states = new int[][] {
                    new int[] { Android.Resource.Attribute.StateChecked },
                    new int[] { -Android.Resource.Attribute.StateChecked }
                };
                var thumbColors = new int[] {
                    new Android.Graphics.Color(0x7B, 0xA3, 0x8E).ToArgb(),
                    new Android.Graphics.Color(0xB0, 0xB0, 0xB0).ToArgb()
                };
                var trackColors = new int[] {
                    new Android.Graphics.Color(0xA3, 0xC9, 0xB3).ToArgb(),
                    new Android.Graphics.Color(0x60, 0x60, 0x60).ToArgb()
                };
                handler.PlatformView.ThumbTintList = new Android.Content.Res.ColorStateList(states, thumbColors);
                handler.PlatformView.TrackTintList = new Android.Content.Res.ColorStateList(states, trackColors);
#endif
            });

            return builder.Build();
        }
    }
}