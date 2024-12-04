using MYMC.ViewModels;

namespace MYMC.Services.Interface;

public interface IWindowService
{
    public void ShowWindow<TViewModel>(IDictionary<string, object>? parameters = null) where TViewModel : IViewModel;
}