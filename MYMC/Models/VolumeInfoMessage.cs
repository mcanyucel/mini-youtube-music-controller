using System.Text.Json;

namespace MYMC.Models;

public class VolumeInfoMessage : YoutubeMusicMessage
{
    public int Volume { get; }
    
    public VolumeInfoMessage(JsonElement root)
    {
        MessageType = YoutubeMusicMessageType.VolumeChanged;
        Volume = root.GetProperty("volume").GetInt32();
    }
    
}