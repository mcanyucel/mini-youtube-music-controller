using System.Text.Json;

namespace MYMC.Models;

public class ShuffleStateMessage : YoutubeMusicMessage
{
    public bool IsShuffled { get; }
    
    public ShuffleStateMessage(JsonElement root)
    {
        MessageType = YoutubeMusicMessageType.ShuffleStateChanged;
        IsShuffled = root.GetProperty("isShuffleOn").GetBoolean();
    }
}