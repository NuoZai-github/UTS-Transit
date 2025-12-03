using Microsoft.Extensions.Logging;
using UTSTransit.Services;
using UTSTransit.ViewModels;
using UTSTransit.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace UTSTransit;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp() // Register SkiaSharp handlers
            //.UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Remove Entry Underline on Android
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (h, v) =>
        {
#if ANDROID
            h.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
        });

        // 1. 注册核心服务 (单例模式)
        builder.Services.AddSingleton<TransitService>();
        builder.Services.AddSingleton<RouteService>();

        // 2. 注册页面 (Transient)
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<SignUpPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<DriverPage>();
        builder.Services.AddTransient<MapPage>();
        builder.Services.AddTransient<TimetablePage>();
        builder.Services.AddTransient<AnnouncementsPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<AdminDashboardPage>();

        // 3. 注册 ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<SignUpViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<DriverViewModel>();
        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<TimetableViewModel>();
        builder.Services.AddTransient<AnnouncementsViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<HomePageViewModel>();
        builder.Services.AddTransient<AdminDashboardViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}