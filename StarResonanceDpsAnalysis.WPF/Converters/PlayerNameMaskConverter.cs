using System.Globalization;
using System.Windows.Data;
using StarResonanceDpsAnalysis.WPF.Helpers;

namespace StarResonanceDpsAnalysis.WPF.Converters;

/// <summary>
/// Masks player names based on provided flags.
/// </summary>
public class PlayerNameMaskConverter : IMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 4) return null;

        var rawName = values[0] as string ?? string.Empty;
        var uid = values[1]?.ToString() ?? string.Empty;
        var maskGlobal = values[2] is true;
        var maskTemp = values[3] is true;

        var useUid = string.IsNullOrEmpty(rawName);

        var showingName = useUid ? uid : rawName;
        if (maskGlobal || maskTemp)
        {
            showingName = NameMasker.Mask(showingName);
        }

        return useUid ? $"UID: {showingName}" : showingName;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}