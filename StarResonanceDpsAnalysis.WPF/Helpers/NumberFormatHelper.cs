using System.Globalization;
using System.Windows;
using StarResonanceDpsAnalysis.WPF.Localization;
using StarResonanceDpsAnalysis.WPF.Models;
using StarResonanceDpsAnalysis.WPF.Properties;

namespace StarResonanceDpsAnalysis.WPF.Helpers;

internal static class NumberFormatHelper
{
    public static bool TryToDouble(object? input, out double value)
    {
        value = double.NaN;
        if (input == null || ReferenceEquals(input, DependencyProperty.UnsetValue))
        {
            return false;
        }

        switch (input)
        {
            case double d:
                value = d;
                return !double.IsNaN(d);
            case float f:
                value = f;
                return true;
            case decimal m:
                value = (double)m;
                return true;
            case int i:
                value = i;
                return true;
            case long l:
                value = l;
                return true;
            case short s:
                value = s;
                return true;
            case byte b:
                value = b;
                return true;
            case string str when double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                value = parsed;
                return true;
            case IConvertible convertible:
                try
                {
                    value = convertible.ToDouble(CultureInfo.InvariantCulture);
                    return true;
                }
                catch
                {
                    // ignored
                }

                break;
        }

        try
        {
            value = Convert.ToDouble(input, CultureInfo.InvariantCulture);
            return !double.IsNaN(value);
        }
        catch
        {
            value = double.NaN;
            return false;
        }
    }

    public static NumberDisplayMode ParseDisplayMode(object? source, NumberDisplayMode fallback = NumberDisplayMode.KMB)
    {
        if (source is null)
        {
            return fallback;
        }

        if (source is NumberDisplayMode mode)
        {
            return mode;
        }

        var text = source.ToString();
        if (string.IsNullOrWhiteSpace(text))
        {
            return fallback;
        }

        if (Enum.TryParse<NumberDisplayMode>(text, true, out var parsed))
        {
            return parsed;
        }

        if (IsLocalizedModeName(text, NumberDisplayMode.Wan))
        {
            return NumberDisplayMode.Wan;
        }

        if (IsLocalizedModeName(text, NumberDisplayMode.KMB))
        {
            return NumberDisplayMode.KMB;
        }

        return fallback;
    }

    public static string FormatHumanReadable<T>(T value, NumberDisplayMode mode, CultureInfo culture)
    {
        if (!TryToDouble(value, out var doubleValue))
        {
            return value?.ToString() ?? string.Empty;
        }

        return FormatHumanReadable(doubleValue, mode, culture);
    }

    public static string FormatHumanReadable(double value, NumberDisplayMode mode, CultureInfo culture)
    {
        var sign = value < 0 ? "-" : string.Empty;
        value = Math.Abs(value);

        if (mode == NumberDisplayMode.Wan)
        {
            var suffixWan = GetSuffix("NumberSuffix_Wan", culture);
            var suffixYi = GetSuffix("NumberSuffix_Yi", culture);
            var suffixZhao = GetSuffix("NumberSuffix_Zhao", culture);

            if (value >= 1_000_000_000_000d)
            {
                return sign + (value / 1_000_000_000_000d).ToString("0.##", culture) + suffixZhao;
            }

            if (value >= 100_000_000)
            {
                return sign + (value / 100_000_000d).ToString("0.##", culture) + suffixYi;
            }

            if (value >= 10_000)
            {
                return sign + (value / 10_000d).ToString("0.##", culture) + suffixWan;
            }

            return sign + value.ToString("0.##", culture);
        }

        var suffixB = GetSuffix("NumberSuffix_B", culture);
        var suffixM = GetSuffix("NumberSuffix_M", culture);
        var suffixK = GetSuffix("NumberSuffix_K", culture);

        if (value >= 1_000_000_000)
        {
            return sign + (value / 1_000_000_000d).ToString("0.##", culture) + suffixB;
        }

        if (value >= 1_000_000)
        {
            return sign + (value / 1_000_000d).ToString("0.##", culture) + suffixM;
        }

        if (value >= 1_000)
        {
            return sign + (value / 1_000d).ToString("0.##", culture) + suffixK;
        }

        return sign + value.ToString("0.##", culture);
    }

    private static string GetSuffix(string key, CultureInfo culture)
    {
        return LocalizationManager.Instance.GetString(key, culture)
               ?? LocalizationManager.Instance.GetString(key, CultureInfo.InvariantCulture)
               ?? string.Empty;
    }

    private static bool IsLocalizedModeName(string text, NumberDisplayMode mode)
    {
        var key = mode == NumberDisplayMode.Wan
            ? ResourcesKeys.NumberDisplay_Wan
            : ResourcesKeys.NumberDisplay_KMB;

        var current = LocalizationManager.Instance.GetString(key, CultureInfo.CurrentUICulture);
        if (!string.IsNullOrWhiteSpace(current) &&
            string.Equals(text, current, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var invariant = LocalizationManager.Instance.GetString(key, CultureInfo.InvariantCulture);
        return !string.IsNullOrWhiteSpace(invariant) &&
               string.Equals(text, invariant, StringComparison.OrdinalIgnoreCase);
    }
}
