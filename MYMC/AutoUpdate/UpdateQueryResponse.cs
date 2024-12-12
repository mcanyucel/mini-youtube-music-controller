namespace MYMC.AutoUpdate;

public class UpdateQueryResponse
{
    public string State { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public string ReleaseNotes { get; init; } = string.Empty;
    public string Sha256Hash { get; init; } = string.Empty;
}