using System.Text.Json.Serialization;

namespace MYMC.Models.Genius;

public sealed record ReturnData(
    [property: JsonPropertyName("response")]
    Response Response
);