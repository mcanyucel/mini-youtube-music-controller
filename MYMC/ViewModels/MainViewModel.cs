using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MYMC.Models;
using MYMC.Services.Interface;
using Serilog;

namespace MYMC.ViewModels;

public partial class MainViewModel(ILogger logger, IPlayerCommandBus commandBus) : ObservableObject, IViewModel
{
    [ObservableProperty]
    private string _statusText = "Loading client...";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TogglePlaybackCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleRepeatModeCommand))]
    private bool _isBusy = true;
    
    [ObservableProperty] private bool _isPlaying;

    [ObservableProperty]
    private TrackInfo? _trackInfo;

    [ObservableProperty]
    private bool _isSeeking;

    [ObservableProperty] private TimeInfo? _timeInfo;

    [ObservableProperty] private RepeatMode _repeatMode = RepeatMode.Off;
    
    public void PlaybackStateChanged(PlayStateMessage message)
    {
        IsPlaying = message.IsPlaying;
    }
    
    public void TrackInfoChanged(TrackInfoMessage message)
    {
        TrackInfo = message.TrackInfo;
    }
    
    public void TimeInfoChanged(TimeInfoMessage message)
    {
        if (!IsSeeking)
        {
            TimeInfo = message.TimeInfo;
        }
    }
    
    public void RepeatModeChanged(RepeatModeMessage message)
    {
        RepeatMode = message.RepeatMode;
    }

    [RelayCommand]
    private void SeekStart()
    {
        IsSeeking = true;
    }
    
    [RelayCommand]
    private void SeekEnd(double value)
    {
        IsSeeking = false;
        commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.Seek, value));
    }
    
    private bool IsBusyCanExecute() => !IsBusy;

    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void TogglePlayback()
    {
        commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.TogglePlayback));
    }

    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void ToggleRepeatMode()
    {
        commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.ToggleRepeatMode));
    }
    
    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void Previous()
    {
        commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.Previous));
    }
    
    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void Next()
    {
        commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.Next));
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