using System.Text.Json;

namespace MYMC.Models;

public class TimeInfoMessage : YoutubeMusicMessage
{
    public TimeInfo TimeInfo { get; }
    
    public TimeInfoMessage(JsonElement root)
    {
        MessageType = YoutubeMusicMessageType.TimeInfoChanged;
        var timeInfoNode = root.GetProperty("timeInfo");
        TimeInfo = new TimeInfo(
            timeInfoNode.GetProperty("progress").GetSingle(),
            timeInfoNode.GetProperty("total").GetSingle()
        );
    }
    
}