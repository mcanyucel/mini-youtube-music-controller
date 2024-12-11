using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MYMC.Models;
using MYMC.Services.Interface;

namespace MYMC.ViewModels;

public partial class LyricsViewModel(ILyricsService lyricsService, Dictionary<string, object> parameters) : ObservableObject, IViewModel
{
    private readonly string _songName = parameters[IViewModel.SongNameParameter] as string ?? throw new ArgumentException("Song name is required");
    private readonly string _artist = parameters[IViewModel.ArtistParameter] as string ?? throw new ArgumentException("Artist is required");

    [ObservableProperty] private LyricsResult? _lyricsResult;
    [ObservableProperty] private bool _isBusy;
    
    [RelayCommand]
    private async Task GetLyrics()
    {
        IsBusy = true;
        LyricsResult = await lyricsService.GetLyrics(_songName, _artist);
        IsBusy = false;
    }
    
    [RelayCommand]
    private void OpenInGoogleSearch()
    {
        try
        {
            var searchQuery = $"{_songName} {_artist} lyrics";
            Process process = new();
            process.StartInfo.FileName = "https://www.google.com/search?q=" + searchQuery;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
        catch
        {
            // ignored
        }
    }
    
    [RelayCommand]
    private void OpenInGenius()
    {
        try
        {
            if (string.IsNullOrEmpty(LyricsResult?.GeniusUrl)) return;

            Process process = new();
            process.StartInfo.FileName = LyricsResult.GeniusUrl;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
        catch
        {
            // ignored
        }
    }
}