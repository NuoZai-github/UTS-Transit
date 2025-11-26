using Microsoft.Extensions.Logging;
using UTSTransit.Services;
using UTSTransit.ViewModels;
using UTSTransit.Views;

namespace UTSTransit;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps() // <--- 关键：初始化地图组件
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. 注册核心服务 (单例模式)
        builder.Services.AddSingleton<TransitService>();

        // 2. 注册页面 (Transient)
        builder.Services.AddTransient<DriverPage>();
        builder.Services.AddTransient<MapPage>();

        // 3. 注册 ViewModels
        builder.Services.AddTransient<DriverViewModel>();
        builder.Services.AddTransient<MapViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}