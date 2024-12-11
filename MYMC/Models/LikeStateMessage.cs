using System.Text.Json;

namespace MYMC.Models;

public class LikeStateMessage : YoutubeMusicMessage
{
    public bool IsLiked { get; }

    public LikeStateMessage(JsonElement root)
    {
        MessageType = YoutubeMusicMessageType.LikeStateChanged;
        IsLiked = root.GetProperty("isLiked").GetBoolean();
    }
    
}