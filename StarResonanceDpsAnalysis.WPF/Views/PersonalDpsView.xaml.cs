using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using StarResonanceDpsAnalysis.WPF.Config;
using StarResonanceDpsAnalysis.WPF.ViewModels;

namespace StarResonanceDpsAnalysis.WPF.Views
{
    /// <summary>
    /// PersonalDpsView.xaml 的交互逻辑
    /// </summary>
    public partial class PersonalDpsView : Window
    {
        public PersonalDpsView(PersonalDpsViewModel viewModel, IConfigManager configManager)
        {
            DataContext = viewModel;
            InitializeComponent();
     
            // 从配置中读取置顶状态,与DPS统计窗口保持一致
            Topmost = configManager.CurrentConfig.TopmostEnabled;
        }

        private void ViewBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                Window.GetWindow(this)?.DragMove();
        }
    }
}
