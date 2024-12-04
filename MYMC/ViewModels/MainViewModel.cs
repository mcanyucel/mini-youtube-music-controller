using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MYMC.Models;
using Serilog;

namespace MYMC.ViewModels;

public partial class MainViewModel(ILogger logger) : ObservableObject, IViewModel
{
    [ObservableProperty]
    private string _statusText = "Loading client...";

    [ObservableProperty] private bool _isBusy = true;
    
    [ObservableProperty] private bool _isPlaying;
    
    public void PlaybackStateChanged(PlayStateMessage message)
    {
        IsPlaying = message.IsPlaying;
    }

    private void TogglePlayback()
    {
        if (IsPlaying)
        {
            Pause();
        }
        else
        {
            Play();
        }
    }
    
    private void Play()
    {
        logger.Information("Playing.");
    }
    
    private void Pause()
    {
        logger.Information("Pausing.");
    }
    
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