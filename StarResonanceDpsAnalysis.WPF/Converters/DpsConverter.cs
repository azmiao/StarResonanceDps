using System.Globalization;
using System.Windows.Data;
using StarResonanceDpsAnalysis.WPF.Models;
using StarResonanceDpsAnalysis.WPF.Helpers;

namespace StarResonanceDpsAnalysis.WPF.Converters;

/// <summary>
/// Multi value converter that computes DPS (damage per second) and formats it with the same
/// rules as <see cref="HumanReadableNumberConverter"/>.
/// Expected bindings:
/// values[0] = total damage (numeric)
/// values[1] = duration (TimeSpan or numeric seconds)
/// values[2] (optional) = mode (NumberDisplayMode or string "KMB"/"Wan")
/// values[3] (optional) = pre-computed ValuePerSecond (double)
/// values[4] (optional) = bool flag: true to use converter-based DPS, false to use ValuePerSecond
/// </summary>
public sealed class DpsConverter : IMultiValueConverter
{
    public object Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        const string notAvailable = "N/A";

        if (values == null || values.Length < 2)
        {
            return notAvailable;
        }

        // Parse optional display mode (default KMB)
        var mode = NumberDisplayMode.KMB;
        if (values.Length > 2 && values[2] != null)
        {
            mode = NumberFormatHelper.ParseDisplayMode(values[2], mode);
        }
        else if (parameter != null)
        {
            mode = NumberFormatHelper.ParseDisplayMode(parameter, mode);
        }

        // Optional toggle: switch between converter-based and direct ValuePerSecond
        var useConverterBased = true;
        if (values.Length > 4 && values[4] is bool flag)
        {
            useConverterBased = flag;
        }

        // Optional pre-computed ValuePerSecond
        double? valuePerSecond = null;
        if (values.Length > 3 && NumberFormatHelper.TryToDouble(values[3], out var vps))
        {
            valuePerSecond = vps;
        }

        if (!useConverterBased && valuePerSecond.HasValue && double.IsFinite(valuePerSecond.Value))
        {
            return NumberFormatHelper.FormatHumanReadable(valuePerSecond.Value, mode, culture);
        }

        if (!NumberFormatHelper.TryToDouble(values[0], out var total))
        {
            return notAvailable;
        }

        double seconds;
        if (values[1] is TimeSpan timeSpan)
        {
            seconds = timeSpan.TotalSeconds;
        }
        else if (!NumberFormatHelper.TryToDouble(values[1], out seconds))
        {
            return notAvailable;
        }

        if (seconds <= 0.0 || double.IsNaN(seconds) || double.IsInfinity(seconds))
        {
            return notAvailable;
        }

        var dps = total / seconds;
        var formatted = NumberFormatHelper.FormatHumanReadable(dps, mode, culture);
        return formatted;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}