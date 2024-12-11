using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using MYMC.Models;
using MYMC.Services.Interface;
using MYMC.ViewModels;

namespace MYMC.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : IDisposable
{
    public MainWindow(IPlayerCommandBus commandBus)
    {
        InitializeComponent();
        _scriptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
        _commandBus = commandBus;
        commandBus.PlayerCommandReceived += HandlePlayerCommand;
    }

    private readonly IPlayerCommandBus _commandBus;

    // ReSharper disable once AsyncVoidMethod - Event handler
    private async void HandlePlayerCommand(object? sender, PlayerCommandMessage e)
    {
        switch (e.CommandType)
        {
            case PlayerCommandType.TogglePlayback:
                await TogglePlayback();
                break;
            case PlayerCommandType.SetVolume:
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
            default:
                throw new ArgumentOutOfRangeException(nameof(e), actualValue: e.CommandType, "Unknown command type");
        }
    }

    private async Task SeekToTime(object? e)
    {
        if (double.TryParse(e?.ToString(), CultureInfo.InvariantCulture, out var time))
        {
            await ExecuteScriptAsync($"seekTo({time});");
        }
    }

    private async Task Next()
    {
        await ExecuteScriptAsync("document.querySelector('ytmusic-player-bar tp-yt-paper-icon-button[aria-label=\"Next\"]').click();");
    }
    
    private async Task ToggleRepeatMode()
    {
        await ExecuteScriptAsync("document.querySelector('ytmusic-player-bar tp-yt-paper-icon-button[class=\"repeat style-scope ytmusic-player-bar\"').click();");
    }

    private async Task ToggleShuffle()
    {
        await ExecuteScriptAsync("document.querySelector('ytmusic-player-bar tp-yt-paper-icon-button[class=\"shuffle style-scope ytmusic-player-bar\"').click();");
    }
    
    private async Task Previous()
    {
        await ExecuteScriptAsync("document.querySelector('ytmusic-player-bar tp-yt-paper-icon-button[aria-label=\"Previous\"]').click();");
    }

    private async Task TogglePlayback()
    {
        await ExecuteScriptAsync("document.querySelector('ytmusic-player-bar tp-yt-paper-icon-button[aria-label=\"Play\"], ytmusic-player-bar tp-yt-paper-icon-button[aria-label=\"Pause\"]').click();");
    }
    
    private async Task ExecuteScriptAsync(string script)
    {
        try
        {
            await WebView.CoreWebView2.ExecuteScriptAsync(script);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Script execution failed: {ex.Message}");
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
        WebView.CoreWebView2.OpenDevToolsWindow();
        WebView.Source = new Uri("https://music.youtube.com/");
    }

    // ReSharper disable once AsyncVoidMethod - Event handler
    private async void WebView_NavigationCompleted(object _, CoreWebView2NavigationCompletedEventArgs __)
    {
        await InjectScriptFile("player-state.js");
    }

    private void HandleWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        Debug.WriteLine($"Received message: {e.WebMessageAsJson}");
        try
        {
            var message = YoutubeMusicMessage.FromJson(e.WebMessageAsJson);
            Debug.WriteLine($"Parsed message type: {message.MessageType}");
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
                default:
                    Debug.WriteLine($"Unknown message type: {message.MessageType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to handle message: {ex.Message}");
            Debug.WriteLine($"Message content was: {e.WebMessageAsJson}");
        }
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

    private void HandleTrackInfoChanged(TrackInfoMessage trackInfo)
    {
        _viewModel?.TrackInfoChanged(trackInfo);
    }

    private void HandlePlayStateChange(PlayStateMessage message)
    {
        _viewModel?.PlaybackStateChanged(message);
    }


    private async Task InjectScriptFile(string fileName)
    {
            var scriptPath = Path.Combine(_scriptsPath, fileName);
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Script file not found: {scriptPath}");
            }
            var scriptContent = await File.ReadAllTextAsync(scriptPath);
            Debug.WriteLine("Executing script");
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