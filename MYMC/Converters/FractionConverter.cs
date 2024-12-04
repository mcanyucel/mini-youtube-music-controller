using System.Globalization;
using System.Windows.Data;

namespace MYMC.Converters;

public class FractionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var dimensionParsed = int.TryParse(value?.ToString(), CultureInfo.InvariantCulture, out var dimension);
        var fractionParsed = double.TryParse(parameter?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var fraction);
        
        if (dimensionParsed && fractionParsed)
        {
            return dimension * fraction;
        }

        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("This converter does not support ConvertBack.");
    }
}