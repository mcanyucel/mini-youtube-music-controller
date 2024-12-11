using System.Text.Json.Serialization;

namespace MYMC.Models.Genius;

public sealed record Response(
    [property: JsonPropertyName("hits")]
    IEnumerable<Hit> Hits
);