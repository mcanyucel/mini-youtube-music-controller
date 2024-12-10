using System.Text.Json;

namespace MYMC.Models;

public class TrackInfoMessage : YoutubeMusicMessage
{
    public TrackInfo TrackInfo { get; }
    
    public TrackInfoMessage(JsonElement root)
    {
        MessageType = YoutubeMusicMessageType.TrackInfoChanged;
        var trackInfoNode = root.GetProperty("trackInfo");
        TrackInfo = new TrackInfo(
            trackInfoNode.GetProperty("title").GetString() ?? string.Empty,
            trackInfoNode.GetProperty("artist").GetString() ?? string.Empty,
            trackInfoNode.GetProperty("album").GetString() ?? string.Empty,
            trackInfoNode.GetProperty("albumArtUrl").GetString() ?? string.Empty
        );
    }
}