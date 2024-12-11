using System.Text.Json.Serialization;

namespace MYMC.Models.Genius;

public sealed record Hit(
    [property: JsonPropertyName("type")]
    string Type,
    [property: JsonPropertyName("result")]
    Result Result);