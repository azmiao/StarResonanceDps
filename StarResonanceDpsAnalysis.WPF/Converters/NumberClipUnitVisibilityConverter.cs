using System.Globalization;
using System.Windows;
using System.Windows.Data;
using StarResonanceDpsAnalysis.WPF.Helpers;
using StarResonanceDpsAnalysis.WPF.Localization;
using StarResonanceDpsAnalysis.WPF.Models;

namespace StarResonanceDpsAnalysis.WPF.Converters;

public sealed class NumberClipUnitVisibilityConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[1] is null)
        {
            return Visibility.Collapsed;
        }

        if (!NumberFormatHelper.TryToDouble(values[0], out var number))
        {
            return Visibility.Collapsed;
        }

        var mode = NumberFormatHelper.ParseDisplayMode(values[1], NumberDisplayMode.KMB);

        var unit = NumberFormatHelper.GetClippedUnit(number, mode, LocalizationManager.Instance.CurrentCulture);
        if (string.IsNullOrWhiteSpace(unit))
        {
            return Visibility.Collapsed;
        }

        var isFullWidth = TextHelper.IsFullWidthText(unit);
        var showFullWidth = !string.Equals(parameter?.ToString(), "Ascii", StringComparison.OrdinalIgnoreCase);
        return (showFullWidth ? isFullWidth : !isFullWidth) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
