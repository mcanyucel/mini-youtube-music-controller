using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MYMC.Extensions;
using MYMC.Services.Interface;
using MYMC.ViewModels;

namespace MYMC;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private IServiceProvider ServiceProvider { get; } = ConfigureServices();

    public const string UpdateHttpClientName = "UpdateHttpClient";

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .ConfigureHttpClientFactory()
            .ConfigureLogger()
            .ConfigureCoreServices()
            .ConfigureLyricsServices()
            .ConfigureViewModels()
            .ConfigureWindows()
            .ConfigureViewModelWindowMapping()
            .AddUpdateServices()
            .ConfigureThemeServices()
            .BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var windowService = ServiceProvider.GetRequiredService<IWindowService>();
        windowService.ShowWindow<MainViewModel>();
    }
}