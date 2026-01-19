using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StarResonanceDpsAnalysis.WPF.Converters;

/// <summary>
/// Converts skill name to a random (but consistent) color brush.
/// Supports MultiBinding for more flexible usage.
/// </summary>
public class SkillNameToRandomColorBrushConverter : IMultiValueConverter
{
    private static readonly Color[] Colors =
    [
        // Cyan/Blue
        Color.FromRgb(0x22, 0xD3, 0xEE),
        // Purple
        Color.FromRgb(0xA7, 0x8B, 0xFA),
        // Green/Teal
        Color.FromRgb(0x34, 0xD3, 0x99),
        // Darker Cyan
        Color.FromRgb(0x06, 0xB6, 0xD4),
        // Light Blue
        Color.FromRgb(0x60, 0xA5, 0xFA),
        // Magenta/Purple
        Color.FromRgb(0xE8, 0x79, 0xF9),
        // Orange
        Color.FromRgb(0xFB, 0xBF, 0x24),
        // Red/Pink
        Color.FromRgb(0xFB, 0x71, 0x85),
        // Indigo
        Color.FromRgb(0x81, 0x8C, 0xF8),
        // Lime
        Color.FromRgb(0xA3, 0xE6, 0x35),
        // Pink/Rose
        Color.FromRgb(0xF4, 0x72, 0xB6),
        // Yellow
        Color.FromRgb(0xFA, 0xCC, 0x15),
        // Violet
        Color.FromRgb(0xC0, 0x84, 0xFC),
        // Emerald
        Color.FromRgb(0x34, 0xD3, 0x99),
        // Sky
        Color.FromRgb(0x38, 0xBD, 0xF8)
    ];

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // Try to get skill name from first value
        string? name = null;
        if (values != null && values.Length > 0)
        {
            name = values[0] as string;
        }

        if (string.IsNullOrEmpty(name))
        {
            // Default fallback
            return new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8));
        }

        // Use a stable hash to ensure the same skill always gets the same color
        var index = Math.Abs(GetStableHashCode(name)) % Colors.Length;
        return new SolidColorBrush(Colors[index]);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static int GetStableHashCode(string str)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in str)
            {
                hash = hash * 31 + c;
            }
            return hash;
        }
    }
}
