using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MYMC.Converters;

public class AccentToBrushConverter : IValueConverter
{
    private readonly Dictionary<string, SolidColorBrush> _brushCache = [];
    private static readonly Dictionary<string, (byte R, byte G, byte B)> Colors = new()
    {
        { "Red", (0xE5, 0x14, 0x00) },
        { "Green", (0x60, 0xA9, 0x17) },
        { "Blue", (0x1B, 0xA1, 0xE2) },
        { "Purple", (0x6A, 0x00, 0xFF) },
        { "Orange", (0xFA, 0x68, 0x00) },
        { "Lime", (0xA4, 0xC4, 0x00) },
        { "Emerald", (0x00, 0x8A, 0x00) },
        { "Teal", (0x00, 0xAB, 0xA9) },
        { "Cyan", (0x1B, 0xA1, 0xE2) },
        { "Cobalt", (0x00, 0x50, 0xEF) },
        { "Indigo", (0x6A, 0x00, 0xFF) },
        { "Violet", (0xAA, 0x00, 0xFF) },
        { "Pink", (0xF4, 0x72, 0xD0) },
        { "Magenta", (0xD8, 0x00, 0x73) },
        { "Crimson", (0xA2, 0x00, 0x25) },
        { "Amber", (0xF0, 0xA3, 0x0A) },
        { "Yellow", (0xE3, 0xC8, 0x00) },
        { "Brown", (0x82, 0x5A, 0x2C) },
        { "Olive", (0x6D, 0x87, 0x64) },
        { "Steel", (0x64, 0x76, 0x87) },
        { "Mauve", (0x76, 0x60, 0x8A) },
        { "Taupe", (0x87, 0x79, 0x4E) },
        { "Sienna", (0xA0, 0x52, 0x2D) }
    };
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string accentName)
            return DependencyProperty.UnsetValue;
        
        if (!Colors.ContainsKey(accentName))
            return DependencyProperty.UnsetValue;
        
        if (_brushCache.TryGetValue(accentName, out var cachedBrush))
            return cachedBrush;

        if (!Colors.TryGetValue(accentName, out var colorValues)) return DependencyProperty.UnsetValue;
        
        var newBrush = new SolidColorBrush(Color.FromRgb(colorValues.R, colorValues.G, colorValues.B));
        newBrush.Freeze(); // Freeze for thread safety and performance
        _brushCache[accentName] = newBrush;
        return newBrush;

    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}