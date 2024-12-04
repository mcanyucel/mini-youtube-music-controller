using Microsoft.Extensions.DependencyInjection;
using MYMC.Services.Implementation;
using MYMC.Services.Interface;
using MYMC.ViewModels;
using MYMC.Windows;
using MYMC.Windows.Factory;
using Serilog;
using Serilog.Core;

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
            { typeof(MainViewModel), typeof(MainWindow) }
        };
        services.AddSingleton<IDictionary<Type, Type>>(viewModelMapping);
        return services;
    }
    
    public static ServiceCollection ConfigureLogger(this ServiceCollection services)
    {
        services.AddSingleton(Log.Logger);
        return services;
    }
    
    public static ServiceCollection ConfigureCoreServices(this ServiceCollection services)
    {
        services.AddSingleton<IWindowService, WindowService>();
        return services;
    }
}