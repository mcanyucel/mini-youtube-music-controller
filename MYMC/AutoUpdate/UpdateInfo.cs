namespace MYMC.AutoUpdate;

public class UpdateInfo
{
    public required Version Version { get; init; }
    public required string DownloadUrl { get; init; }
    public required string ReleaseNotes { get; init; }
    public required string Sha256Hash { get; init; }
}