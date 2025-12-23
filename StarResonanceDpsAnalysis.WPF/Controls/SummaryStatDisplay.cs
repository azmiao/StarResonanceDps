using System.Windows;
using System.Windows.Controls;

namespace StarResonanceDpsAnalysis.WPF.Controls;

public class SummaryStatDisplay : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(string), typeof(SummaryStatDisplay), new PropertyMetadata(null));

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(string), typeof(SummaryStatDisplay), new PropertyMetadata(string.Empty));

    static SummaryStatDisplay()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SummaryStatDisplay),
            new FrameworkPropertyMetadata(typeof(SummaryStatDisplay)));
    }

    public string? Value
    {
        get => (string?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
}