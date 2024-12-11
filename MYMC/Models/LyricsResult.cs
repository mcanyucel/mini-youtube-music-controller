namespace MYMC.Models;

public sealed record LyricsResult
{
    public LyricsResultType ResultType { get; init; }
    public string? Title { get; init; }
    public string? ArtistName { get; init; }
    public string? Lyrics { get; init; }
    public string? ErrorMessage { get; init; }
    public string? GeniusUrl { get; init; }
    
    private LyricsResult(LyricsResultType resultType, string? title = null, string? artistName = null, string? lyrics = null, string? url = null, string? errorMessage = null)
    {
        Title = title;
        ResultType = resultType;
        ArtistName = artistName;
        Lyrics = lyrics;
        ErrorMessage = errorMessage;
        GeniusUrl = url;
    }
    
    public static LyricsResult CreateError(string? errorMessage) => new(LyricsResultType.Error, errorMessage: errorMessage);
    public static LyricsResult CreateNoResult() => new(LyricsResultType.NoResult);
    public static LyricsResult CreateMatch(LyricsResultType lyricsResultType, string title, string artistName, string lyrics, string url) =>
        new(lyricsResultType, title, artistName, lyrics, url);
}