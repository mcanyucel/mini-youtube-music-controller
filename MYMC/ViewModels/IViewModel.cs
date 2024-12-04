namespace MYMC.ViewModels;

public interface IViewModel
{
    public static int GetHash(string viewModelTypeName, IDictionary<string, object>? parameters = null)
    {
        return parameters is null 
            ? viewModelTypeName.GetHashCode() 
            : HashCode.Combine(viewModelTypeName, parameters);
    }
}