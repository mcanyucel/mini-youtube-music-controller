using System.Text.Json;

namespace MYMC.Models;

public class ShareUrlResultMessage : YoutubeMusicMessage
{
    public bool IsSuccess { get; }
    public string? Url { get; }
    public string? Error { get; }

    public ShareUrlResultMessage(JsonElement root)
    {
        MessageType = YoutubeMusicMessageType.ShareUrlResult;
        IsSuccess = root.GetProperty("isSuccess").GetBoolean();
        Url = root.GetProperty("url").ValueKind == JsonValueKind.Null ? null : root.GetProperty("url").GetString();
        Error = root.GetProperty("error").ValueKind == JsonValueKind.Null ? null : root.GetProperty("error").GetString();
    }
}