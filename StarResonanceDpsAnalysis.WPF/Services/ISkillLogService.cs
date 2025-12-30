using System.Collections.ObjectModel;
using StarResonanceDpsAnalysis.WPF.Models;

namespace StarResonanceDpsAnalysis.WPF.Services;

public interface ISkillLogService
{
    ObservableCollection<SkillLogItem> Logs { get; }
    void Clear();
    void AddLog(SkillLogItem log);
}
