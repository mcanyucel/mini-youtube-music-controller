namespace MYMC.ViewModels;

public interface IViewModelFactory
{
    TViewModel Create<TViewModel>(IDictionary<string, object>? parameters = null) where TViewModel : IViewModel;
}