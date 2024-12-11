using System.Windows;
using MYMC.ViewModels;

namespace MYMC.Windows.Factory;

public interface IWindowFactory
{
    Window CreateWindowForViewModel<TViewModel>(IDictionary<string, object>? parameters = null) where TViewModel: IViewModel;
}