using System.Windows;
using MYMC.ViewModels;

namespace MYMC.Windows.Factory;

public interface IWindowFactory
{
    Window CreateWindowForViewModel<TViewModel>() where TViewModel: IViewModel;
}