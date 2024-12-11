using System.Text.Json.Serialization;

namespace MYMC.Models.Genius;

public sealed record Stats(
    [property: JsonPropertyName("pageviews")]
    long Pageviews);