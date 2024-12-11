using System.Net.Http;
using System.Text.Json;
using HtmlAgilityPack;
using MYMC.Models;
using MYMC.Models.Genius;
using MYMC.Services.Interface;
using SimMetrics.Net.Metric;

namespace MYMC.Services.Implementation;

public sealed partial class GeniusService : ILyricsService, IDisposable
{
    public async Task<LyricsResult> GetLyrics(string songName, string artist)
    {
        LyricsResult result;
        try
        {
            // replace spaces with %20 in the song name
            var searchQuery = songName.Replace(" ", "%20");
            var searchUrl = $"{SearchEndpoint}{searchQuery}";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, searchUrl);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            httpRequest.Headers.Add("User-Agent", ClientId);
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseRoot = await Task.Run(() => JsonSerializer.Deserialize<ReturnData>(responseContent, _jsonOptions));

            if (responseRoot == null)
            {
                result = LyricsResult.CreateError("Failed to deserialize Genius API response");
            }
            else
            {
                var hits = responseRoot.Response.Hits;
            var hitsList = hits.ToList();
            if (hitsList.Count != 0)
            {
                Hit finalMatch;
                LyricsResultType resultType;

                var nameMatches = hitsList.Where(hit => hit.Result.Title.Equals(songName, StringComparison.OrdinalIgnoreCase));
                var nameMatchesList = nameMatches.ToList();
                if (nameMatchesList.Count != 0)
                {
                    var artistMatches = nameMatchesList.Where(hit => hit.Result.PrimaryArtist.Name.Equals(artist, StringComparison.OrdinalIgnoreCase));

                    var artistMatchesList = artistMatches.ToList();
                    if (artistMatchesList.Count != 0)
                    {
                        // assume the first match is the correct one
                        finalMatch = artistMatchesList.First();
                        resultType = LyricsResultType.ExactMatch;
                    }
                    else
                    {
                        // name matches but artist does not - possibly a cover. Return the match with the highest page views
                        finalMatch = nameMatchesList.OrderByDescending(hit => hit.Result.Stats.Pageviews).First();
                        resultType = LyricsResultType.NameMatch;
                    }

                }
                else
                {
                    // name does not match - return the match whose name is closest to the search query name
                    SmithWaterman smithWaterman = new();
                    var bestMatch = hitsList.Select(hit => new { Hit = hit, Distance = smithWaterman.GetSimilarity(songName, hit.Result.Title) })
                                        .OrderByDescending(match => match.Distance)
                                        .First();

                    finalMatch = bestMatch.Hit;
                    resultType = LyricsResultType.SimilarMatch;
                }

                var lyricsUrl = finalMatch.Result.Url;
                var lyrics = await GetLyricsFromUrl(lyricsUrl);
                result = LyricsResult.CreateMatch(resultType, finalMatch.Result.Title, finalMatch.Result.PrimaryArtist.Name, lyrics, lyricsUrl);
            }
            else
                result = LyricsResult.CreateNoResult();
            }
        }
        catch (Exception e)
        {
            result = LyricsResult.CreateError(e.Message);
        }

        return result;
    }


    private async Task<string> GetLyricsFromUrl(string url)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        // lyrics are in a div where data-lyrics-container attribute is set to "true"
        var doc = new HtmlDocument();
        doc.LoadHtml(responseContent);
        var lyricsNode = doc.DocumentNode.SelectSingleNode("//div[@data-lyrics-container='true']");
        var innerHtml = lyricsNode.InnerHtml;
        // handle breaks
        innerHtml = innerHtml.Replace("<br>", "\n");
        // decode HTML entities
        responseContent = System.Net.WebUtility.HtmlDecode(innerHtml);
        // remove all HTML tags
        responseContent = HtmlRegex().Replace(responseContent, string.Empty);
        return responseContent;
    }
    
    
    private const string SearchEndpoint = "https://api.genius.com/search?q=";
    
    private readonly HttpClient _httpClient = new();
    private const string ClientId = "la6heSbzjJ0lp9R7tHG5vW_A7upv-UwmaSW_B7BJMRWRpY_rGqFK4X9Ajt79K-35"; // not a secret - Genius is free to use
    private const string AccessToken = "smK8LGPcMwVpGhN8leCeRe9dyFxA20sVKnSEb54aZOFnXPdfBc_2ALLYgrZn6Aij"; // not a secret - Genius is free to use
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip
    };

    [System.Text.RegularExpressions.GeneratedRegex("<.*?>")]
    private static partial System.Text.RegularExpressions.Regex HtmlRegex();
 
    private bool _disposed;
    
    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            // Dispose managed resources
            _httpClient.Dispose();
        }
        // dispose unmanaged resources
            
        _disposed = true;
    }
    
    public void Dispose()
    {
        Dispose(disposing: true);
        // GC.SuppressFinalize(this); // Uncomment if unmanaged resources are disposed
    }
    
    // Uncomment if unmanaged resources are disposed
    // ~GeniusService()
    // {
    //     Dispose(disposing: false);
    // }
    
}