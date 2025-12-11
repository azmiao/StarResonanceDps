using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WPFLocalizeExtension.Providers;
using Forms = System.Windows.Forms;

namespace StarResonanceDpsAnalysis.WPF.Controls;

public class DpsIndicatorControl : Control
{
    // Percentage value in range 0..Maximum. Use double for proper binding with ProgressBar-like behavior.
    public static readonly DependencyProperty PercentageProperty = DependencyProperty.Register(
        nameof(Percentage), typeof(double), typeof(DpsIndicatorControl),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPercentageChanged));

    // AnimatedPercentage: used by the template to animate visual changes when Percentage updates
    public static readonly DependencyProperty AnimatedPercentageProperty = DependencyProperty.Register(
        nameof(AnimatedPercentage), typeof(double), typeof(DpsIndicatorControl),
        new PropertyMetadata(0d));

    // Maximum value used to scale Percentage (default 100)
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(double), typeof(DpsIndicatorControl),
        new PropertyMetadata(100d));

    // Background brush for the track
    public static readonly DependencyProperty IndicatorBackgroundProperty = DependencyProperty.Register(
        nameof(IndicatorBackground), typeof(Brush), typeof(DpsIndicatorControl),
        new PropertyMetadata(Brushes.LightGray));

    // Foreground / fill brush for the indicator
    public static readonly DependencyProperty IndicatorForegroundProperty = DependencyProperty.Register(
        nameof(IndicatorForeground), typeof(Brush), typeof(DpsIndicatorControl),
        new PropertyMetadata(Brushes.DodgerBlue));

    // CornerRadius for rounded track/indicator
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius), typeof(CornerRadius), typeof(DpsIndicatorControl),
        new PropertyMetadata(new CornerRadius(4)));

    // Template used to render overlay content on top of the progress indicator.
    public static readonly DependencyProperty OverlayTemplateProperty = DependencyProperty.Register(
        nameof(OverlayTemplate), typeof(DataTemplate), typeof(DpsIndicatorControl),
        new PropertyMetadata(null));

    // Content to be passed to the overlay template.
    public static readonly DependencyProperty OverlayContentProperty = DependencyProperty.Register(
        nameof(OverlayContent), typeof(object), typeof(DpsIndicatorControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty PopupTemplateProperty = DependencyProperty.Register(
        nameof(PopupTemplate), typeof(DataTemplate), typeof(DpsIndicatorControl),
        new PropertyMetadata(default(DataTemplate?), OnPopupTemplateChanged));

    public static readonly DependencyProperty PopupContentProperty = DependencyProperty.Register(
        nameof(PopupContent), typeof(object), typeof(DpsIndicatorControl),
        new PropertyMetadata(default, OnPopupContentChanged));

    public static readonly DependencyProperty TrackOpacityProperty = DependencyProperty.Register(
        nameof(TrackOpacity), typeof(double), typeof(DpsIndicatorControl), new PropertyMetadata(default(double)));

    public static readonly DependencyProperty SkillDisplayLimitProperty = DependencyProperty.Register(
        nameof(SkillDisplayLimit), typeof(int), typeof(DpsIndicatorControl), new PropertyMetadata(8));

    public static readonly DependencyProperty MouseEnterCommandProperty = DependencyProperty.Register(
        nameof(MouseEnterCommand), typeof(ICommand), typeof(DpsIndicatorControl),
        new PropertyMetadata(default(ICommand?)));

    public static readonly DependencyProperty MouseLeaveCommandProperty = DependencyProperty.Register(
        nameof(MouseLeaveCommand), typeof(ICommand), typeof(DpsIndicatorControl),
        new PropertyMetadata(default(ICommand?)));

    private Popup? _popup;

    static DpsIndicatorControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DpsIndicatorControl),
            new FrameworkPropertyMetadata(typeof(DpsIndicatorControl)));
    }

    public DpsIndicatorControl()
    {
        // ? 修改: MouseEnter时触发tooltip数据刷新
        MouseEnter += OnMouseEnterRefreshTooltip;
        MouseLeave += OnMouseLeaveResetHover;
    }

    public ICommand? MouseEnterCommand
    {
        get => (ICommand?)GetValue(MouseEnterCommandProperty);
        set => SetValue(MouseEnterCommandProperty, value);
    }

    public ICommand? MouseLeaveCommand
    {
        get => (ICommand?)GetValue(MouseLeaveCommandProperty);
        set => SetValue(MouseLeaveCommandProperty, value);
    }

    public double TrackOpacity
    {
        get => (double)GetValue(TrackOpacityProperty);
        set => SetValue(TrackOpacityProperty, value);
    }

    public int SkillDisplayLimit
    {
        get => (int)GetValue(SkillDisplayLimitProperty);
        set => SetValue(SkillDisplayLimitProperty, value);
    }

    public double Percentage
    {
        get => (double)GetValue(PercentageProperty);
        set => SetValue(PercentageProperty, value);
    }

    public double AnimatedPercentage
    {
        get => (double)GetValue(AnimatedPercentageProperty);
        set => SetValue(AnimatedPercentageProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public Brush? IndicatorBackground
    {
        get => (Brush?)GetValue(IndicatorBackgroundProperty);
        set => SetValue(IndicatorBackgroundProperty, value);
    }

    public Brush? IndicatorForeground
    {
        get => (Brush?)GetValue(IndicatorForegroundProperty);
        set => SetValue(IndicatorForegroundProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public DataTemplate? OverlayTemplate
    {
        get => (DataTemplate?)GetValue(OverlayTemplateProperty);
        set => SetValue(OverlayTemplateProperty, value);
    }

    public object? OverlayContent
    {
        get => GetValue(OverlayContentProperty);
        set => SetValue(OverlayContentProperty, value);
    }

    public DataTemplate? PopupTemplate
    {
        get => (DataTemplate?)GetValue(PopupTemplateProperty);
        set => SetValue(PopupTemplateProperty, value);
    }

    public object? PopupContent
    {
        get => (object?)GetValue(PopupContentProperty);
        set => SetValue(PopupContentProperty, value);
    }

    private static void OnPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DpsIndicatorControl ctl) return;

        var newVal = (double)e.NewValue;

        // Create smooth animation from current AnimatedPercentage to new Percentage
        var animation = new DoubleAnimation
        {
            To = newVal,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        // Begin animation on AnimatedPercentageProperty
        ctl.BeginAnimation(AnimatedPercentageProperty, animation);
    }

    private static void OnPopupTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Debug.WriteLine($"[DpsIndicatorControl] PopupTemplate changed: {e.NewValue?.GetType().Name ?? "null"}");
    }

    private static void OnPopupContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var oldPlayerName = (e.OldValue as dynamic)?.Player?.Name ?? "null";
        var newPlayerName = (e.NewValue as dynamic)?.Player?.Name ?? "null";
        Debug.WriteLine($"[DpsIndicatorControl] PopupContent changed: {oldPlayerName} -> {newPlayerName}");
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("PART_Popup") is Popup popup)
        {
            _popup = popup;
            popup.Placement = PlacementMode.Custom;
            popup.CustomPopupPlacementCallback = PlacePopup;
        }
    }

    private void OnMouseEnterRefreshTooltip(object sender, MouseEventArgs e)
    {
        if (MouseEnterCommand?.CanExecute(SkillDisplayLimit) == true)
        {
            MouseEnterCommand.Execute(SkillDisplayLimit);
        }
    }

    private void OnMouseLeaveResetHover(object sender, MouseEventArgs e)
    {
        if (MouseLeaveCommand?.CanExecute(null) == true)
        {
            MouseLeaveCommand.Execute(null);
        }
    }

    private CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
    {
        const double gap = 10;
        const double defOffsetY = -4;
        const double defPopupWidth = 250;

        // Default: right side
        var preferred = new Point(targetSize.Width + gap, defOffsetY);

        // If we cannot get placement target or screen info, use default
        if (_popup?.PlacementTarget is not UIElement target)
        {
            return [new CustomPopupPlacement(preferred, PopupPrimaryAxis.Horizontal)];
        }

        while (target is not ListBoxItem)
        {
            target = (UIElement)target.GetParent(true);
        }

        // Compute target top-left in screen coordinates
        var targetScreenPoint = target.PointToScreen(new Point(0, 0));
        var screenPoint = new System.Drawing.Point((int)targetScreenPoint.X, (int)targetScreenPoint.Y);
        var screen = Forms.Screen.FromPoint(screenPoint);
        var screenRight = screen.WorkingArea.Right;

        // If showing on the right would overflow screen, flip to left
        var rightEdge = targetScreenPoint.X + target.RenderSize.Width + gap + defPopupWidth;
        var useLeft = rightEdge > screenRight;
        var placement = useLeft
            ? new Point(-popupSize.Width, defOffsetY)
            : preferred;

        return [new CustomPopupPlacement(placement, PopupPrimaryAxis.Horizontal)];
    }
}