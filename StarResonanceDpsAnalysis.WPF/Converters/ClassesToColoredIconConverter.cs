using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using StarResonanceDpsAnalysis.Core.Models;

namespace StarResonanceDpsAnalysis.WPF.Converters;

public class ClassesToColoredIconConverter : IValueConverter
{
    private readonly Dictionary<Classes, ImageSource?> _iconCache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Classes classes) return null;

        if (_iconCache.TryGetValue(classes, out var cached) && cached is not null)
            return cached;

        var app = Application.Current;
        var keyToTry = $"Classes{value}Icon";

        var res = app?.TryFindResource(keyToTry);
        if (res is not ImageSource img) return null;
        _iconCache[classes] = img;
        return img;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException($"{nameof(ClassesToColoredIconConverter)} does not support ConvertBack.");
    }

}