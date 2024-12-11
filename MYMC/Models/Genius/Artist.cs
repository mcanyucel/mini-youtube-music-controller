using System.Text.Json.Serialization;

namespace MYMC.Models.Genius;

public sealed record Artist(
    [property: JsonPropertyName("api_path")]
    string ApiPath,
    [property: JsonPropertyName("name")]
    string Name);