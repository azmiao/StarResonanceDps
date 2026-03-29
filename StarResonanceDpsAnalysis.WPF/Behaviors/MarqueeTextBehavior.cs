using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;

namespace StarResonanceDpsAnalysis.WPF.Behaviors;

public enum MarqueeState
{
    Idle,
    Playing,
    Finished
}

public class MarqueeTextBehavior : Behavior<FrameworkElement>
{
    public static readonly DependencyProperty ContainerProperty =
        DependencyProperty.Register(nameof(Container), typeof(FrameworkElement), typeof(MarqueeTextBehavior),
            new PropertyMetadata(null, OnContainerChanged));

    public static readonly DependencyProperty IsAnimationEnabledProperty =
        DependencyProperty.Register(nameof(IsAnimationEnabled), typeof(bool), typeof(MarqueeTextBehavior),
            new PropertyMetadata(true, OnIsAnimationEnabledChanged));

    public static readonly DependencyProperty IsOverflowingProperty =
        DependencyProperty.Register(nameof(IsOverflowing), typeof(bool), typeof(MarqueeTextBehavior),
            new PropertyMetadata(false));

    public static readonly DependencyProperty PlayStateProperty = DependencyProperty.Register(
        nameof(PlayState), typeof(MarqueeState), typeof(MarqueeTextBehavior), new PropertyMetadata(default(MarqueeState)));

    private Storyboard? _storyboard;

    public FrameworkElement? Container
    {
        get => (FrameworkElement)GetValue(ContainerProperty);
        set => SetValue(ContainerProperty, value);
    }

    public bool IsAnimationEnabled
    {
        get => (bool)GetValue(IsAnimationEnabledProperty);
        set => SetValue(IsAnimationEnabledProperty, value);
    }

    public bool IsOverflowing
    {
        get => (bool)GetValue(IsOverflowingProperty);
        set => SetValue(IsOverflowingProperty, value);
    }

    public MarqueeState PlayState
    {
        get => (MarqueeState)GetValue(PlayStateProperty);
        set => SetValue(PlayStateProperty, value);
    }

    private static void OnIsAnimationEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MarqueeTextBehavior)d).UpdateAnimation();
    }

    private static void OnContainerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (MarqueeTextBehavior)d;
        behavior.SetupListeners(e.OldValue as FrameworkElement, e.NewValue as FrameworkElement);
        behavior.UpdateAnimation();
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SizeChanged += OnSizeChanged;
        AssociatedObject.Loaded += OnLoaded;
        SetupListeners(null, Container);
        UpdateAnimation();
    }

    protected override void OnDetaching()
    {
        StopAnimation();
        AssociatedObject.SizeChanged -= OnSizeChanged;
        AssociatedObject.Loaded -= OnLoaded;
        SetupListeners(Container, null);
        base.OnDetaching();
    }

    private void SetupListeners(FrameworkElement? oldContainer, FrameworkElement? newContainer)
    {
        if (oldContainer != null)
        {
            oldContainer.SizeChanged -= OnSizeChanged;
        }

        if (newContainer != null)
        {
            newContainer.SizeChanged += OnSizeChanged;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAnimation();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (AssociatedObject == null || Container == null) return;

        if (!IsAnimationEnabled)
        {
            IsOverflowing = false; // Reset status when animation is disabled
            StopAnimation();
            Canvas.SetLeft(AssociatedObject, 0);
            return;
        }

        var contentWidth = AssociatedObject.ActualWidth;
        var containerWidth = Container.ActualWidth;

        // If not loaded effectively
        if (containerWidth <= 0 || contentWidth <= 0) return;

        // Add a small tolerance
        if (contentWidth > containerWidth + 0.5)
        {
            IsOverflowing = true;
            StartAnimation(contentWidth, containerWidth);
        }
        else
        {
            IsOverflowing = false;
            StopAnimation();
            ResetOpacityMask();
            Canvas.SetLeft(AssociatedObject, 0);
        }
    }

    private void StartAnimation(double contentWidth, double containerWidth)
    {
        StopAnimation();

        var scrollDistance = contentWidth - containerWidth;
        var speed = 30.0;
        var scrollTime = scrollDistance / speed;
        if (scrollTime < 1.0) scrollTime = 1.0;

        var fadeDuration = TimeSpan.FromSeconds(0.5); // Smooth transition speed
        var pauseStartTime = TimeSpan.FromSeconds(2);
        var scrollEndTime = pauseStartTime + TimeSpan.FromSeconds(scrollTime);
        var totalLoopTime = scrollEndTime + TimeSpan.FromSeconds(2);

        var sb = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        // --- 1. Movement Animation ---
        var posAnim = new DoubleAnimationUsingKeyFrames();
        posAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, TimeSpan.Zero));
        posAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0, pauseStartTime));
        posAnim.KeyFrames.Add(new LinearDoubleKeyFrame(-scrollDistance, scrollEndTime));
        posAnim.KeyFrames.Add(new LinearDoubleKeyFrame(-scrollDistance, totalLoopTime));
        Storyboard.SetTarget(posAnim, AssociatedObject);
        Storyboard.SetTargetProperty(posAnim, new PropertyPath("(Canvas.Left)"));
        sb.Children.Add(posAnim);

        // --- 2. Left Stop Fade (Cross-fade at 2s) ---
        var leftFade = new ColorAnimationUsingKeyFrames();
        leftFade.KeyFrames.Add(new DiscreteColorKeyFrame(Colors.Black, TimeSpan.Zero)); // Idle: Hidden
        leftFade.KeyFrames.Add(new LinearColorKeyFrame(Colors.Transparent, pauseStartTime + fadeDuration)); // Playing: Fade in
        Storyboard.SetTarget(leftFade, Container);
        Storyboard.SetTargetProperty(leftFade, new PropertyPath("OpacityMask.GradientStops[0].Color"));
        sb.Children.Add(leftFade);

        // --- 3. Right Stop Fade (Cross-fade at scroll end) ---
        var rightFade = new ColorAnimationUsingKeyFrames();
        rightFade.KeyFrames.Add(new DiscreteColorKeyFrame(Colors.Transparent, TimeSpan.Zero)); // Idle: Visible
        rightFade.KeyFrames.Add(new LinearColorKeyFrame(Colors.Transparent, scrollEndTime)); // Playing: Visible
        rightFade.KeyFrames.Add(new LinearColorKeyFrame(Colors.Black, scrollEndTime + fadeDuration)); // Finished: Fade out
        Storyboard.SetTarget(rightFade, Container);
        Storyboard.SetTargetProperty(rightFade, new PropertyPath("OpacityMask.GradientStops[3].Color"));
        sb.Children.Add(rightFade);

        _storyboard = sb;
        sb.Begin();
    }

    private void StopAnimation()
    {
        if (_storyboard == null) return;
        _storyboard.Stop();
        _storyboard = null;
    }

    private void ResetOpacityMask()
    {
        if (Container?.OpacityMask is LinearGradientBrush brush)
        {
            // Check if the brush is frozen
            if (brush.IsFrozen)
            {
                // Clone creates a mutable copy
                brush = brush.Clone();
                Container.OpacityMask = brush;
            }

            foreach (var stop in brush.GradientStops)
            {
                // Now safe to clear animations and change color
                stop.BeginAnimation(GradientStop.ColorProperty, null);
                stop.Color = Colors.Black;
            }
        }
        PlayState = MarqueeState.Idle;
    }
}