using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MYMC.Extensions;
using MYMC.Services.Interface;
using MYMC.ViewModels;
using MYMC.Windows;
using MYMC.Windows.Factory;
using Serilog;

namespace MYMC;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IServiceProvider ServiceProvider { get; }

    public App()
    {
        ServiceProvider = ConfigureServices();
    }

    private static void ConfigureLogger()
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
    }

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .ConfigureLogger()
            .ConfigureCoreServices()
            .ConfigureViewModels()
            .ConfigureWindows()
            .ConfigureViewModelWindowMapping()
            .BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var windowService = ServiceProvider.GetRequiredService<IWindowService>();
        windowService.ShowWindow<MainViewModel>();
    }
}