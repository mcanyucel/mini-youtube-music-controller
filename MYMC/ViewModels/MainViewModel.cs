using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MYMC.AutoUpdate;
using MYMC.Models;
using MYMC.Services.Interface;
using MYMC.Settings;
using Serilog;
using Timer = System.Timers.Timer;

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
    
    [ObservableProperty] private GetShareLinkStatus _shareLinkStatus = GetShareLinkStatus.Ready;
    
    [ObservableProperty] private double _updateProgress;
    [ObservableProperty] private List<string> _supportedAccents;
    [ObservableProperty] private double _startLeft;
    [ObservableProperty] private double _startTop;
    
    
    
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

    private string _theme = IThemeService.LightThemeName;
    public string Theme
    {
        get => _theme;
        private set
        {
            if (!SetProperty(ref _theme, value)) return;
            
            _userSettings.Theme = value;
            _userSettings.Save();
            _themeService.SetThemeAndAccent(value, Accent);
        }
    }
    
    private string _accent = IThemeService.DefaultAccentName;
    public string Accent
    {
        get => _accent;
        private set
        {
            if (!SetProperty(ref _accent, value)) return;
            
            _userSettings.Accent = value;
            _userSettings.Save();
            _themeService.SetThemeAndAccent(Theme, value);
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
    private readonly IThemeService _themeService;
    private readonly Timer _shareLinkTimer = new(5000);
    
    public MainViewModel(ILogger logger, IPlayerCommandBus commandBus, IWindowService windowService, UpdateEngine updateEngine, 
        IDialogService dialogService, ISystemService systemService, IThemeService themeService)
    {
        _logger = logger;
        _commandBus = commandBus;
        _windowService = windowService;
        _updateEngine = updateEngine;
        _dialogService = dialogService;
        _systemService = systemService;
        _themeService = themeService;

        _userSettings = UserSettings.Load();
        _updateEngine.UpdateProgress += UpdateEngine_UpdateProgress;
        SupportedAccents = _themeService.SupportedAccents;
        ApplyUserSettings();
        
        _shareLinkTimer.Elapsed += ShareLinkTimer_Elapsed;
    }

    private void ShareLinkTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        _shareLinkTimer.Stop();
        ShareLinkStatus = GetShareLinkStatus.Ready;
    }

    private void UpdateEngine_UpdateProgress(object? sender, UpdateProgressEventArgs e)
    {
        UpdateProgress = e.ProgressPercentage;
    }

    private void ApplyUserSettings()
    {
        TopMost = _userSettings.IsTopMost;
        IsCompact = _userSettings.IsCompactMode;
        Theme = _userSettings.Theme;
        Accent = _userSettings.Accent;
        StartLeft = _userSettings.Left;
        StartTop = _userSettings.Top;
        _themeService.SetThemeAndAccent(_userSettings.Theme, _userSettings.Accent);
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
    
    public void ShareUrlResult(ShareUrlResultMessage message)
    {
        if (message is { IsSuccess: true, Url: not null })
        {
            _systemService.CopyToClipboard(message.Url);
            ShareLinkStatus = GetShareLinkStatus.Success;
        }
        else
        {
            ShareLinkStatus = GetShareLinkStatus.Error;
        }
        
        _shareLinkTimer.Start();
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

    
    public void SaveWindowLocation(double left, double top)
    {
        _userSettings.Left = left;
        _userSettings.Top = top;
        _userSettings.Save();
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
        ShareLinkStatus = GetShareLinkStatus.Loading;
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
    
    [RelayCommand]
    private void ToggleTheme()
    {
        Theme = Theme == IThemeService.LightThemeName ? IThemeService.DarkThemeName : IThemeService.LightThemeName;
    }

    [RelayCommand]
    private void ToggleAccent()
    {
        var currentAccentIndex = SupportedAccents.IndexOf(Accent);
        var nextAccentIndex = (currentAccentIndex + 1) % SupportedAccents.Count;
        Accent = SupportedAccents[nextAccentIndex];
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