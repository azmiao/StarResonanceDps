using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace StarResonanceDpsAnalysis.WPF.Converters;

/// <summary>
/// 将多个布尔值转换为可见性。所有值为true时返回Visible,否则返回Collapsed。
/// </summary>
public class MultiBoolToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length == 0)
   return Visibility.Collapsed;

  // 检查是否所有值都是true
        bool allTrue = values.OfType<bool>().All(b => b);
        
        return allTrue ? Visibility.Visible : Visibility.Collapsed;
 }

 public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
