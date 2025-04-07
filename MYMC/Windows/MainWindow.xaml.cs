using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using MYMC.Models;
using MYMC.Services.Interface;
using MYMC.ViewModels;
using Serilog;

namespace MYMC.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : IDisposable
{
    public MainWindow(IPlayerCommandBus commandBus, ILogger logger)
    {
        InitializeComponent();
        _scriptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
        _commandBus = commandBus;
        _logger = logger;
        commandBus.PlayerCommandReceived += HandlePlayerCommand;
    }

    private readonly IPlayerCommandBus _commandBus;
    private readonly ILogger _logger;

    // ReSharper disable once AsyncVoidMethod - Event handler
    private async void HandlePlayerCommand(object? sender, PlayerCommandMessage e)
    {
        switch (e.CommandType)
        {
            case PlayerCommandType.TogglePlayback:
                await TogglePlayback();
                break;
            case PlayerCommandType.SetVolume:
                await SetVolume(e.Parameter);
                break;
            case PlayerCommandType.Next:
                await Next();
                break;
            case PlayerCommandType.Previous:
                await Previous();
                break;
            case PlayerCommandType.Seek:
                await SeekToTime(e.Parameter);
                break;
            case PlayerCommandType.ToggleRepeatMode:
                await ToggleRepeatMode();
                break;
            case PlayerCommandType.ToggleShuffle:
                await ToggleShuffle();
                break;
            case PlayerCommandType.SetCompactMode:
                SetCompactMode(e.Parameter);
                break;
            case PlayerCommandType.ToggleLike:
                await ToggleLike();
                break;
            case PlayerCommandType.Dislike:
                await Dislike();
                break;
            case PlayerCommandType.GetShareableLink:
                await GetShareableLink();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(e), actualValue: e.CommandType, "Unknown command type");
        }
    }

    private async Task GetShareableLink()
    {
        await ExecuteScriptAsync("getShareUrl();");
    }

    private async Task ToggleLike()
    {
        await ExecuteScriptAsync("document.querySelector('button[aria-label=\"Like\"]')?.click();");
    }
    
    private async Task Dislike()
    {
        await ExecuteScriptAsync("document.querySelector('button[aria-label=\"Dislike\"]')?.click();");
    }

    private async Task SeekToTime(object? e)
    {
        if (double.TryParse(e?.ToString(), CultureInfo.InvariantCulture, out var time))
        {
            await ExecuteScriptAsync($"seekTo({time});");
        }
    }
    
    private async Task SetVolume(object? e)
    {
        if (double.TryParse(e?.ToString(), CultureInfo.InvariantCulture, out var volume))
        {
            await ExecuteScriptAsync($"setVolume({volume});");
        }
    }

    private async Task Next()
    {
        await ExecuteScriptAsync("nextTrack();");
    }
    
    private async Task ToggleRepeatMode()
    {
        await ExecuteScriptAsync("toggleRepeat();");
    }

    private async Task ToggleShuffle()
    {
        await ExecuteScriptAsync("toggleShuffle();");
    }
    
    private async Task Previous()
    {
        await ExecuteScriptAsync("previousTrack();");
    }

    private async Task TogglePlayback()
    {
        await ExecuteScriptAsync("togglePlayback();");
    }

    private void SetCompactMode(object? e)
    {
        if (e is not bool isCompact) return;
        
        if (isCompact)
        {
            WindowState = WindowState.Normal;
            Height = 132;
            Width = 594;
        }
        else
        {
            WindowState = WindowState.Maximized;
        }
    }
    
    private async Task ExecuteScriptAsync(string script)
    {
        try
        {
            await WebView.CoreWebView2.ExecuteScriptAsync(script);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to execute script: {Script}", script);
        }
    }

    // ReSharper disable once AsyncVoidMethod - Event handler
    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel = (MainViewModel)DataContext;
        await InitializeWebView();
    }

    private async Task InitializeWebView()
    {
        var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MYMC", "cache", "MainWindow");
        var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
        await WebView.EnsureCoreWebView2Async(environment);
        WebView.CoreWebView2.WebMessageReceived += HandleWebMessage;        
        WebView.Source = new Uri("https://music.youtube.com/");

#if DEBUG
        WebView.CoreWebView2.OpenDevToolsWindow();
#endif
    }

    // ReSharper disable once AsyncVoidMethod - Event handler
    private async void WebView_NavigationCompleted(object _, CoreWebView2NavigationCompletedEventArgs __)
    {
        await InjectScriptFile("player-state.js");
    }

    private void HandleWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var message = YoutubeMusicMessage.FromJson(e.WebMessageAsJson);
            switch (message)
            {
                case PlayStateMessage playState:
                    HandlePlayStateChange(playState);
                    break;
                case TrackInfoMessage trackInfo:
                    HandleTrackInfoChanged(trackInfo);
                    break;
                case TimeInfoMessage timeInfo:
                    HandleTimeInfoChanged(timeInfo);
                    break;
                case RepeatModeMessage repeatMode:
                    HandleRepeatModeChanged(repeatMode);
                    break;
                case ShuffleStateMessage shuffleState:
                    HandleShuffleStateChanged(shuffleState);
                    break;
                case VolumeInfoMessage volumeInfo:
                    HandleVolumeInfoChanged(volumeInfo);
                    break;
                case LikeStateMessage likeState:
                    HandleLikeStateChanged(likeState);
                    break;
                case ShareUrlResultMessage shareUrlResult:
                    HandleShareUrlResult(shareUrlResult);
                    break;
                default:
                    _logger.Error("Unknown message type: {MessageType}", message.MessageType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to handle web message, the content was {e.WebMessageAsJson}");
        }
    }

    private void HandleLikeStateChanged(LikeStateMessage likeState)
    {
        _viewModel?.LikeStateChanged(likeState);
    }

    private void HandleTimeInfoChanged(TimeInfoMessage timeInfo)
    {
        _viewModel?.TimeInfoChanged(timeInfo);
    }
    
    private void HandleShuffleStateChanged(ShuffleStateMessage shuffleState)
    {
        _viewModel?.ShuffleStateChanged(shuffleState);
    }
    
    private void HandleRepeatModeChanged(RepeatModeMessage repeatMode)
    {
        _viewModel?.RepeatModeChanged(repeatMode);
    }
    
    private void HandleVolumeInfoChanged(VolumeInfoMessage volumeInfo)
    {
        _viewModel?.VolumeInfoChanged(volumeInfo);
    }

    private void HandleTrackInfoChanged(TrackInfoMessage trackInfo)
    {
        _viewModel?.TrackInfoChanged(trackInfo);
    }

    private void HandlePlayStateChange(PlayStateMessage message)
    {
        _viewModel?.PlaybackStateChanged(message);
    }
    
    private void HandleShareUrlResult(ShareUrlResultMessage message)
    {
        _viewModel?.ShareUrlResult(message);
    }


    private async Task InjectScriptFile(string fileName)
    {
            var scriptPath = Path.Combine(_scriptsPath, fileName);
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Script file not found: {scriptPath}");
            }
            var scriptContent = await File.ReadAllTextAsync(scriptPath);
            await WebView.CoreWebView2.ExecuteScriptAsync(scriptContent);
    }

    private readonly string _scriptsPath;
    private MainViewModel? _viewModel;

    private bool _disposed;
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects)
                WebView.CoreWebView2.WebMessageReceived -= HandleWebMessage;
                _commandBus.PlayerCommandReceived -= HandlePlayerCommand;
            }
            // Free unmanaged resources (unmanaged objects) and override finalizer

            _disposed = true;
        }
    }
    
    public void Dispose()
    {
        Dispose(disposing: true);
        // GC.SuppressFinalize(this); // Uncomment if finalizer is overridden
    }
    
    // Uncomment if finalizer is overridden
    // ~MainWindow()
    // {
    //     Dispose(disposing: false);
    // }
}