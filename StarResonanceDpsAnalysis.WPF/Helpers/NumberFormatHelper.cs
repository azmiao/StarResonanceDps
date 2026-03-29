using System.Globalization;
using System.Windows;
using StarResonanceDpsAnalysis.WPF.Localization;
using StarResonanceDpsAnalysis.WPF.Models;
using StarResonanceDpsAnalysis.WPF.Properties;

namespace StarResonanceDpsAnalysis.WPF.Helpers;

internal static class NumberFormatHelper
{
    /// <summary>
    /// 在三位计数法下, 数值最大剪裁次数 (3 => 最大达到B)
    /// </summary>
    private const int MAX_KMB_CLIP_TIMES = 3;

    /// <summary>
    /// 在四位计数法下, 数值最大剪裁次数 (3 => 最大达到兆)
    /// </summary>
    private const int MAX_WAN_CLIP_TIMES = 3;

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
            if (value >= 1_000_000_000_000d)
            {
                return $"{sign}{(value / 1_000_000_000_000d).ToString("0.##", culture)}{GetSuffix("NumberSuffix_Zhao", culture)}";
            }

            if (value >= 100_000_000)
            {
                return $"{sign}{(value / 100_000_000d).ToString("0.##", culture)}{GetSuffix("NumberSuffix_Yi", culture)}";
            }

            if (value >= 10_000)
            {
                return $"{sign}{(value / 10_000d).ToString("0.##", culture)}{GetSuffix("NumberSuffix_Wan", culture)}";
            }

            return sign + value.ToString("0.##", culture);
        }

        if (value >= 1_000_000_000)
        {
            return $"{sign}{(value / 1_000_000_000d).ToString("0.##", culture)}{GetSuffix("NumberSuffix_B", culture)}";
        }

        if (value >= 1_000_000)
        {
            return $"{sign}{(value / 1_000_000d).ToString("0.##", culture)}{GetSuffix("NumberSuffix_M", culture)}";
        }

        if (value >= 1_000)
        {
            return $"{sign}{(value / 1_000d).ToString("0.##", culture)}{GetSuffix("NumberSuffix_K", culture)}";
        }

        return sign + value.ToString("0.##", culture);
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

    public static double GetClippedValue(double value, NumberDisplayMode mode)
    {
        var clipTimes = GetClipTimes(value, mode);
        var divisor = Math.Pow(mode == NumberDisplayMode.Wan ? 10_000d : 1_000d, clipTimes);
        if (divisor <= 0)
        {
            return value;
        }

        return value / divisor;
    }

    public static string GetClippedUnit(double value, NumberDisplayMode mode, CultureInfo culture)
    {
        var clipTimes = GetClipTimes(value, mode);
        if (clipTimes <= 0)
        {
            return string.Empty;
        }

        if (mode == NumberDisplayMode.Wan)
        {
            return clipTimes switch
            {
                1 => GetSuffix("NumberSuffix_Wan", culture),
                2 => GetSuffix("NumberSuffix_Yi", culture),
                3 => GetSuffix("NumberSuffix_Zhao", culture),
                _ => string.Empty
            };
        }

        return clipTimes switch
        {
            1 => GetSuffix("NumberSuffix_K", culture),
            2 => GetSuffix("NumberSuffix_M", culture),
            3 => GetSuffix("NumberSuffix_B", culture),
            _ => string.Empty
        };
    }

    private static string GetSuffix(string key, CultureInfo culture)
    {
        return LocalizationManager.Instance.GetString(key, culture)
               ?? LocalizationManager.Instance.GetString(key, CultureInfo.InvariantCulture)
               ?? string.Empty;
    }

    private static int GetClipTimes(double value, NumberDisplayMode mode)
    {
        var absValue = Math.Abs(value);
        var maxClipTimes = mode == NumberDisplayMode.Wan ? MAX_WAN_CLIP_TIMES : MAX_KMB_CLIP_TIMES;
        var step = mode == NumberDisplayMode.Wan ? 10_000d : 1_000d;

        var clipTimes = 0;
        while (clipTimes < maxClipTimes && absValue >= step)
        {
            absValue /= step;
            clipTimes++;
        }

        return clipTimes;
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
