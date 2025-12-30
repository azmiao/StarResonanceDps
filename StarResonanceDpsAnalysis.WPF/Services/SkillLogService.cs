using System.Collections.ObjectModel;
using StarResonanceDpsAnalysis.WPF.Models;

namespace StarResonanceDpsAnalysis.WPF.Services;

public class SkillLogService : ISkillLogService
{
    public ObservableCollection<SkillLogItem> Logs { get; } = new();

    public void Clear()
    {
        Logs.Clear();
    }

    public void AddLog(SkillLogItem log)
    {
        Logs.Add(log);
    }
}
