using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarResonanceDpsAnalysis.WPF.Models;
using StarResonanceDpsAnalysis.WPF.Services;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public partial class SkillLogViewModel : ObservableObject
{
    private readonly ISkillLogService _skillLogService;

    public ObservableCollection<SkillLogItem> Logs => _skillLogService.Logs;

    // Design-time constructor
    public SkillLogViewModel()
    {
        _skillLogService = new SkillLogService();
        // Add dummy data for design time
        if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
        {
            _skillLogService.AddLog(new SkillLogItem { Timestamp = System.DateTime.Now, SkillName = "Test Skill", TotalValue = 1234, Count = 1 });
        }
    }

    public SkillLogViewModel(ISkillLogService skillLogService)
    {
        _skillLogService = skillLogService;
    }

    [RelayCommand]
    private void Clear()
    {
        _skillLogService.Clear();
    }

    [RelayCommand]
    private void Close(Window window)
    {
        window?.Close();
    }
}
