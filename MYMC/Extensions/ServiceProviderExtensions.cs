using System.IO;
using Microsoft.Extensions.DependencyInjection;
using MYMC.AutoUpdate;
using MYMC.Services.Implementation;
using MYMC.Services.Interface;
using MYMC.ViewModels;
using MYMC.Windows;
using MYMC.Windows.Factory;
using Serilog;

namespace MYMC.Extensions;

public static class ServiceProviderExtensions
{
    public static ServiceCollection ConfigureViewModels(this ServiceCollection services)
    {
        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
        return services;
    }
    
    public static ServiceCollection ConfigureWindows(this ServiceCollection services)
    {
        services.AddSingleton<IWindowFactory, WindowFactory>();
        return services;
    }
    
    public static ServiceCollection ConfigureViewModelWindowMapping(this ServiceCollection services)
    {
        var viewModelMapping = new Dictionary<Type, Type>
        {
            { typeof(MainViewModel), typeof(MainWindow) },
            { typeof(LyricsViewModel), typeof(LyricsWindow) }
        };
        services.AddSingleton<IDictionary<Type, Type>>(viewModelMapping);
        return services;
    }

    public static ServiceCollection ConfigureLyricsServices(this ServiceCollection services)
    {
        services.AddSingleton<ILyricsService, GeniusService>();
        return services;
    }
    
    public static ServiceCollection ConfigureHttpClientFactory(this ServiceCollection services)
    {
        services.AddHttpClient();
        return services;
    }
    
    public static ServiceCollection ConfigureLogger(this ServiceCollection services)
    {
        var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MiniYoutubeMusicController",
            "logs");
        
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(logDirectory, "log-.txt"), rollingInterval: RollingInterval.Day);
        
#if DEBUG
        loggerConfiguration.MinimumLevel.Debug();
#else
        loggerConfiguration.MinimumLevel.Error();
#endif
        Log.Logger = loggerConfiguration.CreateLogger();
        Log.Information("Application started.");
        services.AddSingleton(Log.Logger);
        return services;
    }
    
    public static ServiceCollection ConfigureCoreServices(this ServiceCollection services)
    {
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<IPlayerCommandBus, PlayerCommandBus>();
        return services;
    }
    
    public static ServiceCollection AddUpdateServices(this ServiceCollection services)
    {
        services.AddSingleton<UpdateEngine>();
        return services;
    }
}