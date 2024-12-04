using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;
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
    }

    // ReSharper disable once AsyncVoidMethod - Event handler
    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MYMC", "cache", "MainWindow");
        var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
        await WebView.EnsureCoreWebView2Async(environment);
        WebView.Source = new Uri("https://music.youtube.com/");
    }
    
    
}