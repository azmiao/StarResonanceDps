using System;
using System.Globalization;
using System.Windows.Data;

namespace StarResonanceDpsAnalysis.WPF.Converters;

/// <summary>
/// WPF converter that converts empty strings to null.
/// Used as a fallback for bindings to prevent ImageSourceConverter errors.
/// </summary>
public class EmptyStringToNullConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : str;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
