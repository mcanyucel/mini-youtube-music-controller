using Microsoft.Extensions.DependencyInjection;

namespace MYMC.ViewModels;

public class ViewModelFactory(IServiceProvider serviceProvider) : IViewModelFactory
{
    public TViewModel Create<TViewModel>(IDictionary<string, object>? parameters = null) where TViewModel : IViewModel
    {
        return parameters is null
            ? ActivatorUtilities.CreateInstance<TViewModel>(serviceProvider)
            : ActivatorUtilities.CreateInstance<TViewModel>(serviceProvider, parameters);
    }
}