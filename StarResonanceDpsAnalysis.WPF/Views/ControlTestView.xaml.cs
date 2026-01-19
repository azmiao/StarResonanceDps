using System.Windows;

namespace StarResonanceDpsAnalysis.WPF.Views
{
    /// <summary>
    /// Test.xaml 的交互逻辑
    /// </summary>
    public partial class ControlTestView : Window
    {
        public long TestTotal { get; } = 256_800;
        public long TestNormalDamage { get; } = 180_000;
        public long TestCritDamage { get; } = 60_000;
        public long TestLuckyDamage { get; } = 16_800;
        public double TestAverage { get; } = 8_560;
        public long TestHits { get; } = 42;
        public double TestCritRate { get; } = 0.285;
        public long TestCritCount { get; } = 12;
        public long TestLuckyCount { get; } = 4;
        public double TestLuckyRate { get; } = 0.095;

        public ControlTestView()
        {
            InitializeComponent();

            DataContext = this;
        }
    }
}
