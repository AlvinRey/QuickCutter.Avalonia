using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia;
using System.Globalization;
using System;

namespace QuickCutter_Avalonia.Converters;
public class DataGridRowIndexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DataGridRow row)
            return row.GetIndex() + 1;

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}