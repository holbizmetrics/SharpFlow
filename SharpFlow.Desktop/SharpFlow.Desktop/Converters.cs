// ====================================================
// Converters.cs - Simple converters
// ====================================================

using System;

using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SharpFlow.Desktop;

public class NodeColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "HttpRequest" => new SolidColorBrush(Color.FromRgb(40, 167, 69)),   // Green
            "Timer" => new SolidColorBrush(Color.FromRgb(255, 193, 7)),         // Orange/Yellow
            _ => new SolidColorBrush(Color.FromRgb(108, 117, 125))              // Gray
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class SelectionColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
            return new SolidColorBrush(Color.FromRgb(0, 123, 255)); // Blue when selected

        return new SolidColorBrush(Colors.Transparent); // Transparent when not selected
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue;
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}