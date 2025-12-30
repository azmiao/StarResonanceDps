using System.Windows;
using System.Windows.Input;

namespace StarResonanceDpsAnalysis.WPF.Views;

public partial class SkillLogView : Window
{
    public SkillLogView()
    {
        InitializeComponent();
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
