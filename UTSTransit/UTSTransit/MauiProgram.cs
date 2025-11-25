using CommunityToolkit.Maui;
using Microsoft.Maui.Controls.Hosting;
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
            .UseMauiMaps() // <--- 重要：启用地图功能
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 注册核心服务 (单例模式：整个 App 共享这一个实例)
        builder.Services.AddSingleton<TransitService>();

        // 注册页面和 ViewModel (后面创建页面时用到)
        // builder.Services.AddTransient<LoginPage>();
        // builder.Services.AddTransient<MapPage>();

        return builder.Build();
    }
}
