using System.Text.Json;

namespace MYMC.Models;

public class PlayStateMessage : YoutubeMusicMessage
{
    public bool IsPlaying { get; }

    public PlayStateMessage(JsonElement root)
    {
        MessageType = YoutubeMusicMessageType.PlayStateChanged;
        IsPlaying = root.GetProperty("isPlaying").GetBoolean();
    }
}