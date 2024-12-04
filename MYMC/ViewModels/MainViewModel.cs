using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace MYMC.ViewModels;

public partial class MainViewModel(ILogger logger) : ObservableObject, IViewModel
{
    [ObservableProperty]
    private string _statusText = "Loading Youtube Music Client...";

    [ObservableProperty] private bool _isBusy = true;
    
    [RelayCommand]
    private void NavigationCompleted()
    {
        IsBusy = false;
        StatusText = "Ready";
    }
    
    [RelayCommand]
    private async Task NavigationStarting()
    {
        await Task.Delay(100);
        logger.Information("Navigation starting.");
    }
}