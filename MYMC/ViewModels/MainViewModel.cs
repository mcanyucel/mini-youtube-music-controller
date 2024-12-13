using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MYMC.AutoUpdate;
using MYMC.Models;
using MYMC.Services.Interface;
using MYMC.Settings;
using Serilog;

namespace MYMC.ViewModels;

public sealed partial class MainViewModel : ObservableObject, IViewModel, IDisposable
{
    [ObservableProperty]
    private string _statusText = "Loading client...";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TogglePlaybackCommand))]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleRepeatModeCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleShuffleCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleLikeCommand))]
    [NotifyCanExecuteChangedFor(nameof(DislikeCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowLyricsWindowCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyShareUrlCommand))]
    private bool _isBusy = true;
    
    [ObservableProperty] private bool _isPlaying;
    
    [ObservableProperty] private bool _isShuffled;

    [ObservableProperty] private bool _isLiked;

    [ObservableProperty] private bool _isDownloadingUpdate;
    
    [ObservableProperty] private double _updateProgress;

    private bool _topMost;
    public bool TopMost
    {
        get => _topMost;
        set
        {
            if (!SetProperty(ref _topMost, value)) return;
            
            _userSettings.IsTopMost = value;
            _userSettings.Save();
        }
    }

    private bool _isCompact;

    public bool IsCompact
    {
        get => _isCompact;
        set
        {
            if (!SetProperty(ref _isCompact, value)) return;
            
            _userSettings.IsCompactMode = value;
            _userSettings.Save();
            ApplyCompactModeChange(value);
        }
    }

    private void ApplyCompactModeChange(bool value)
    {
        _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.SetCompactMode, value));
    }

    [ObservableProperty] private TrackInfo? _trackInfo;
    [ObservableProperty] private bool _isSeeking;
    [ObservableProperty] private bool _isAdjustingVolume;
    [ObservableProperty] private TimeInfo? _timeInfo;
    [ObservableProperty] private RepeatMode _repeatMode = RepeatMode.Off;
    [ObservableProperty] private int _volume = 100;

    private int _previousVolume;
    private readonly ILogger _logger;
    private readonly IPlayerCommandBus _commandBus;
    private readonly UserSettings _userSettings;
    private readonly IWindowService _windowService;
    private readonly UpdateEngine _updateEngine;
    private readonly IDialogService _dialogService;
    private readonly ISystemService _systemService;
    
    public MainViewModel(ILogger logger, IPlayerCommandBus commandBus, IWindowService windowService, UpdateEngine updateEngine, IDialogService dialogService, ISystemService systemService)
    {
        _logger = logger;
        _commandBus = commandBus;
        _windowService = windowService;
        _updateEngine = updateEngine;
        _dialogService = dialogService;
        _systemService = systemService;

        _userSettings = UserSettings.Load();
        _updateEngine.UpdateProgress += UpdateEngine_UpdateProgress;
        ApplyUserSettings();
    }

    private void UpdateEngine_UpdateProgress(object? sender, UpdateProgressEventArgs e)
    {
        UpdateProgress = e.ProgressPercentage;
    }

    private void ApplyUserSettings()
    {
        TopMost = _userSettings.IsTopMost;
        IsCompact = _userSettings.IsCompactMode;
    }

    public void PlaybackStateChanged(PlayStateMessage message)
    {
        IsPlaying = message.IsPlaying;
    }
    
    public void LikeStateChanged(LikeStateMessage message)
    {
        IsLiked = message.IsLiked;
    }
    
    public void ShuffleStateChanged(ShuffleStateMessage message)
    {
        IsShuffled = message.IsShuffled;
    }
    
    public void VolumeInfoChanged(VolumeInfoMessage message)
    {
        if (!IsAdjustingVolume)
        {
            Volume = message.Volume;
        }
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
        _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.Seek, value));
    }
    
    [RelayCommand]
    private void VolumeAdjustStart()
    {
        IsAdjustingVolume = true;
    }
    
    [RelayCommand]
    private void VolumeAdjustEnd(double value)
    {
        IsAdjustingVolume = false;
        _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.SetVolume, value));
    }

    [RelayCommand]
    private void ToggleMute()
    {
        if (Volume > 0)
        {
            _previousVolume = Volume;
            _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.SetVolume, 0));
        }
        else
        {
            _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.SetVolume, _previousVolume));
        }
    }

    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void ToggleLike()
    {
        _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.ToggleLike));
    }

    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void Dislike()
    {
        // unlike 'like', 'dislike' is not a toggle - it immediately dislikes the track and skips to the next one
        _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.Dislike));
    }

    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void ShowLyricsWindow()
    {
        if (TrackInfo?.Title == null || TrackInfo?.Artist == null) return;
        var parameters = new Dictionary<string, object>
        {
            {IViewModel.SongNameParameter, TrackInfo?.Title!},
            {IViewModel.ArtistParameter, TrackInfo?.Artist!}
        };
        _windowService.ShowWindow<LyricsViewModel>(parameters);
    }
    
    private bool IsBusyCanExecute() => !IsBusy;

    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void TogglePlayback()
    {
        _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.TogglePlayback));
    }

    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void ToggleRepeatMode()
    {
        _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.ToggleRepeatMode));
    }

    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void ToggleShuffle()
    {
        _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.ToggleShuffle));
    }
    
    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void Previous()
    {
        _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.Previous));
    }
    
    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void Next()
    {
        _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.Next));
    }

    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private void CopyShareUrl()
    {
        _commandBus.SendPlayerCommand(new PlayerCommandMessage(PlayerCommandType.GetShareableLink));
    }

    [RelayCommand]
    private async Task NavigationCompleted()
    {
        if (IsDevelopmentEnvironment())
        {
            _logger.Information("Skipping auto-update check in development environment.");
        }
        else
        {
            // check for updates only after the UI is fully loaded and the navigation is completed
            StatusText = "Checking for updates...";
            var hasUpdates = await _updateEngine.CheckForUpdates();
            if (hasUpdates)
            {
                var shouldUpdate = _dialogService.ShowYesNoMessageBox("An update is available. Do you want to download it now?", "Update available");
                if (shouldUpdate)
                {
                    StatusText = "Downloading update...";
                    IsDownloadingUpdate = true;
                    await _updateEngine.DownloadAndInstallUpdate(); // this will close the app
                    IsDownloadingUpdate = false;
                }
            }
        }

        IsBusy = false;
        StatusText = "Ready";
    }
    
    [RelayCommand]
    private async Task NavigationStarting()
    {
        await Task.Delay(100);
        _logger.Information("Navigation starting.");
    }

    private static bool IsDevelopmentEnvironment()
    {
        // ReSharper disable once RedundantAssignment
        var isDevelopment = System.Diagnostics.Debugger.IsAttached;
        
#if DEBUG
        isDevelopment = true;
#endif

        return isDevelopment;
    }
    
    private bool _disposed;
    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            // Dispose managed resources
            _updateEngine.UpdateProgress -= UpdateEngine_UpdateProgress;
        }
        // Dispose unmanaged resources
        
        _disposed = true;
    }
    
    public void Dispose()
    {
        Dispose(true);
        // GC.SuppressFinalize(this); // Uncomment if disposing unmanaged resources
    }
    
    // Uncomment if disposing unmanaged resources
    // ~MainViewModel()
    // {
    //     Dispose(false);
    // }
}