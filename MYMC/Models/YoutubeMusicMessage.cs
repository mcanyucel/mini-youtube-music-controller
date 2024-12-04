﻿using System.Text.Json;

namespace MYMC.Models;

public abstract class YoutubeMusicMessage
{
    public YoutubeMusicMessageType MessageType { get; protected set; }

    public static YoutubeMusicMessage FromJson(string jsonString)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;
            
            var typeStr = root.GetProperty("type").GetString();
            if (!Enum.TryParse<YoutubeMusicMessageType>(typeStr, true, out var messageType))
            {
                throw new ArgumentException($"Unknown message type: {typeStr}");
            }

            return messageType switch
            {
                YoutubeMusicMessageType.PlayStateChanged => new PlayStateMessage(root),
                YoutubeMusicMessageType.VolumeChanged => throw new NotImplementedException(),
                YoutubeMusicMessageType.TrackInfoChanged => throw new NotImplementedException(),
                _ => throw new ArgumentException($"Unknown message type: {typeStr}")
            };
        }
        catch (Exception e)
        {
            throw new ArgumentException("Failed to parse message", e);
        }
    }
}