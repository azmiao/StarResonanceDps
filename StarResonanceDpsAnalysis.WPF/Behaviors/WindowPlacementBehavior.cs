using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace StarResonanceDpsAnalysis.WPF.Behaviors;

/// <summary>
/// A behavior that allows binding the window placement (position and size) to a ViewModel command or property.
/// </summary>
public class WindowPlacementBehavior : Behavior<Window>
{
    public static readonly DependencyProperty StartUpStateProperty =
        DependencyProperty.Register(nameof(StartUpState), typeof(Rectangle?), typeof(WindowPlacementBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SaveCommandProperty =
        DependencyProperty.Register(nameof(SaveCommand), typeof(ICommand), typeof(WindowPlacementBehavior),
            new PropertyMetadata(null));

    public Rectangle? StartUpState
    {
        get => (Rectangle?)GetValue(StartUpStateProperty);
        set => SetValue(StartUpStateProperty, value);
    }

    public ICommand? SaveCommand
    {
        get => (ICommand?)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SourceInitialized += OnSourceInitialized;
        AssociatedObject.Closing += OnClosing;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.SourceInitialized -= OnSourceInitialized;
        AssociatedObject.Closing -= OnClosing;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        if (StartUpState.HasValue)
        {
            var rect = StartUpState.Value;
            // Check if the saved position is visible on any monitor
            if (IsRectVisibleOnScreen(rect))
            {
                AssociatedObject.Left = rect.X;
                AssociatedObject.Top = rect.Y;
                if (rect.Width > 0) AssociatedObject.Width = rect.Width;
                if (rect.Height > 0) AssociatedObject.Height = rect.Height;
            }
        }
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (SaveCommand == null) return;
        var bounds = AssociatedObject.WindowState == WindowState.Normal
            ? new Rect(AssociatedObject.Left, AssociatedObject.Top, AssociatedObject.Width, AssociatedObject.Height)
            : AssociatedObject.RestoreBounds;

        if (!double.IsFinite(bounds.Left) || !double.IsFinite(bounds.Top) ||
            !double.IsFinite(bounds.Width) || !double.IsFinite(bounds.Height) ||
            !(bounds.Width > 0) || !(bounds.Height > 0)) return;
        var rect = new Rectangle((int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height);
        if (SaveCommand.CanExecute(rect))
        {
            SaveCommand.Execute(rect);
        }
    }

    private bool IsRectVisibleOnScreen(Rectangle rect)
    {
        var winRect = new RECT { Left = rect.Left, Top = rect.Top, Right = rect.Right, Bottom = rect.Bottom };
        // MONITOR_DEFAULTTONULL (0) - returns NULL if not intersecting any monitor
        return MonitorFromRect(ref winRect, 0) != IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromRect(ref RECT lprc, uint dwFlags);
}