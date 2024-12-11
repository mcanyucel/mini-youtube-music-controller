using System.Text.Json;

namespace MYMC.Models;

public class RepeatModeMessage : YoutubeMusicMessage
{
    public RepeatMode RepeatMode { get; }

    public RepeatModeMessage(JsonElement root)
    {
        MessageType = YoutubeMusicMessageType.RepeatModeChanged;
        var modeString = root.GetProperty("mode").GetString();
        RepeatMode =  modeString switch
        {
            "Repeat off" => RepeatMode.Off,
            "Repeat one" => RepeatMode.One,
            "Repeat all" => RepeatMode.All,
            _ => throw new ArgumentException($"Unknown repeat mode: {modeString}")
        };
    }
}