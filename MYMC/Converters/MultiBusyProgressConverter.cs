using System.Globalization;
using System.Windows.Data;

namespace MYMC.Converters;

public class MultiBusyProgressConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is bool isBusy && values[1] is bool isDownloadingUpdate)
        {
            return isBusy && !isDownloadingUpdate;
        }
        
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("This converter does not support two-way binding.");
    }
}