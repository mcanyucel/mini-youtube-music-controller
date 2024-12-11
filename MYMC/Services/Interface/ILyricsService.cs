using MYMC.Models;

namespace MYMC.Services.Interface;

public interface ILyricsService
{
    public Task<LyricsResult> GetLyrics(string songName, string artist);
}