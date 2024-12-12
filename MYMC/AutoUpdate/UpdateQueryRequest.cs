namespace MYMC.AutoUpdate;

public class UpdateQueryRequest(string appIdentity, int stateSeed)
{
    public string AppIdentity { get; } = appIdentity ?? throw new ArgumentNullException(nameof(appIdentity));
    public int StateSeed { get; } = stateSeed;
}