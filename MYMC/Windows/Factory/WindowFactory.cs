using System.Windows;
using MYMC.ViewModels;
using Serilog;

namespace MYMC.Windows.Factory;

public class WindowFactory(IDictionary<Type, Type> viewModelMapping, IViewModelFactory viewModelFactory, ILogger logger) : IWindowFactory
{
    public Window CreateWindowForViewModel<TViewModel>() where TViewModel : IViewModel
    {
        var viewModelType = typeof(TViewModel);
        if (!viewModelMapping.TryGetValue(viewModelType, out var windowType))
        {
            logger.Error("No window type found for view model type {ViewModelType}", viewModelType);
            throw new InvalidOperationException($"No window type found for view model type {viewModelType}");
        }

        if (!typeof(Window).IsAssignableFrom(windowType))
        {
            logger.Error("Window type {WindowType} does not inherit from {BaseType}", windowType, typeof(Window));
            throw new InvalidOperationException($"Window type {windowType} does not inherit from {typeof(Window)}");
        }

        if (Activator.CreateInstance(windowType) is Window window)
        {
            var viewModel = viewModelFactory.Create<TViewModel>();
            window.DataContext = viewModel;
            return window;
        }
        
        logger.Error("Failed to create window of type {WindowType}", windowType);
        throw new InvalidOperationException($"Failed to create window of type {windowType}");

    }
}