using System.Globalization;
using System.Windows.Data;
using StarResonanceDpsAnalysis.WPF.Helpers;
using StarResonanceDpsAnalysis.WPF.Localization;
using StarResonanceDpsAnalysis.WPF.Models;

namespace StarResonanceDpsAnalysis.WPF.Converters;

public sealed class NumberClipUnitConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[1] is null)
        {
            return string.Empty;
        }

        if (!NumberFormatHelper.TryToDouble(values[0], out var number))
        {
            return string.Empty;
        }

        var mode = NumberFormatHelper.ParseDisplayMode(values[1], NumberDisplayMode.KMB);

        return NumberFormatHelper.GetClippedUnit(number, mode, LocalizationManager.Instance.CurrentCulture);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
