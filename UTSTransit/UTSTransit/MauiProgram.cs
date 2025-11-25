using Microsoft.Extensions.Logging;
using UTSTransit.Services;
using UTSTransit.ViewModels;
using UTSTransit.Views; // 现在这个引用应该正常了

namespace UTSTransit;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps() // 启用地图
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. 注册服务 (Singleton)
        builder.Services.AddSingleton<TransitService>();

        // 2. 注册页面 (Transient - 每次打开都是新的)
        builder.Services.AddTransient<DriverPage>();
        builder.Services.AddTransient<MapPage>();

        // 3. 注册 ViewModel (如果使用依赖注入)
        builder.Services.AddTransient<DriverViewModel>();
        builder.Services.AddTransient<MapViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}