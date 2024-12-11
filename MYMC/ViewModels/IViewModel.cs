namespace MYMC.ViewModels;

public interface IViewModel
{
    public const string SongNameParameter = "SongName";
    public const string ArtistParameter = "Artist";
    
    public static int GetHash(string viewModelTypeName, IDictionary<string, object>? parameters = null)
    {
        return parameters is null 
            ? viewModelTypeName.GetHashCode() 
            : HashCode.Combine(viewModelTypeName, parameters);
    }
}