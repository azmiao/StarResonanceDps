using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using StarResonanceDpsAnalysis.WPF.ViewModels;

namespace StarResonanceDpsAnalysis.WPF.Views;

/// <summary>
/// SkillBreakdownView.xaml 的交互逻辑
/// </summary>
public partial class SkillBreakdownView : Window
{
    private readonly SkillBreakdownViewModel _viewModel;

    public SkillBreakdownView(SkillBreakdownViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        DataContext = vm;

        Loaded += OnLoaded;
        Closed += OnClosed; // ⭐ 添加窗口关闭事件处理
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SyncSelectorWithTab();
    }

    /// <summary>
    /// ⭐ 窗口关闭时停止实时更新并释放资源
    /// </summary>
    private void OnClosed(object? sender, EventArgs e)
    {
        _viewModel.StopRealTimeUpdate();
        _viewModel.Dispose();
    }

    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Sync the custom selector buttons with the TabControl
        SyncSelectorWithTab();
    }

    private void TabSelector_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton tb || !int.TryParse(tb.Tag?.ToString(), out var index)) return;

        // Update TabControl selection
        MainTabControl.SelectedIndex = index;
    }

    private void SyncSelectorWithTab()
    {
        if (TabControlIndexChanger == null || MainTabControl == null) return;

        var selectedIndex = MainTabControl.SelectedIndex;

        foreach (var child in LogicalTreeHelper.GetChildren(TabControlIndexChanger))
        {
            if (child is not ToggleButton t || !int.TryParse(t.Tag?.ToString(), out var tagIndex)) continue;
            t.IsChecked = tagIndex == selectedIndex;
        }
    }

    private void Footer_ConfirmClick(object sender, RoutedEventArgs e)
    {
    }

    private void Footer_CancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
    }
}