using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StarResonanceDpsAnalysis.WPF.Controls;

public class CustomProgressBar : ProgressBar
{
    private static readonly DependencyPropertyKey ProgressRatePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(ProgressRate),
        typeof(double),
        typeof(CustomProgressBar),
        new PropertyMetadata(0d));

    public static readonly DependencyProperty ProgressRateProperty = ProgressRatePropertyKey.DependencyProperty;

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(CustomProgressBar),
        new PropertyMetadata(new CornerRadius(4)));

    public static readonly DependencyProperty ProgressCornerRadiusProperty = DependencyProperty.Register(
        nameof(ProgressCornerRadius),
        typeof(CornerRadius),
        typeof(CustomProgressBar),
        new PropertyMetadata(new CornerRadius(4)));

    public static readonly DependencyProperty BarPaddingProperty = DependencyProperty.Register(
        nameof(BarPadding),
        typeof(Thickness),
        typeof(CustomProgressBar),
        new PropertyMetadata(new Thickness(0)));

    public static readonly DependencyProperty ProgressBorderBrushProperty = DependencyProperty.Register(
        nameof(ProgressBorderBrush),
        typeof(Brush),
        typeof(CustomProgressBar),
        new PropertyMetadata(Brushes.Transparent));

    public static readonly DependencyProperty ProgressBorderThicknessProperty = DependencyProperty.Register(
        nameof(ProgressBorderThickness),
        typeof(Thickness),
        typeof(CustomProgressBar),
        new PropertyMetadata(new Thickness(0)));

    static CustomProgressBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomProgressBar),
            new FrameworkPropertyMetadata(typeof(CustomProgressBar)));
    }

    public double ProgressRate => (double)GetValue(ProgressRateProperty);

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public CornerRadius ProgressCornerRadius
    {
        get => (CornerRadius)GetValue(ProgressCornerRadiusProperty);
        set => SetValue(ProgressCornerRadiusProperty, value);
    }

    public Thickness BarPadding
    {
        get => (Thickness)GetValue(BarPaddingProperty);
        set => SetValue(BarPaddingProperty, value);
    }

    public Brush ProgressBorderBrush
    {
        get => (Brush)GetValue(ProgressBorderBrushProperty);
        set => SetValue(ProgressBorderBrushProperty, value);
    }

    public Thickness ProgressBorderThickness
    {
        get => (Thickness)GetValue(ProgressBorderThicknessProperty);
        set => SetValue(ProgressBorderThicknessProperty, value);
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        UpdateProgressRate();
    }

    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    {
        base.OnMaximumChanged(oldMaximum, newMaximum);
        UpdateProgressRate();
    }

    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    {
        base.OnMinimumChanged(oldMinimum, newMinimum);
        UpdateProgressRate();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsIndeterminateProperty)
        {
            UpdateProgressRate();
        }
    }

    private void UpdateProgressRate()
    {
        if (IsIndeterminate)
        {
            SetValue(ProgressRatePropertyKey, 1d);
            return;
        }

        var range = Maximum - Minimum;
        var value = Value;
        double rate;

        if (double.IsNaN(range) || range <= 0 || double.IsNaN(value))
        {
            rate = 0d;
        }
        else
        {
            rate = (value - Minimum) / range;
            if (double.IsNaN(rate) || double.IsInfinity(rate))
            {
                rate = 0d;
            }
        }

        rate = Math.Max(0d, Math.Min(1d, rate));
        SetValue(ProgressRatePropertyKey, rate);
    }
}
