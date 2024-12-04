using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using MYMC.Models;
using MYMC.ViewModels;

namespace MYMC.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        _scriptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
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

}