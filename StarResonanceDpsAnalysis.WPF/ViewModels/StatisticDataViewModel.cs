using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

[DebuggerDisplay("Name:{Player?.Name};Value:{Value}")]
public partial class StatisticDataViewModel(DebugFunctions debug) : BaseViewModel, IComparable<StatisticDataViewModel>
{
    [ObservableProperty] private ulong _duration;
    [ObservableProperty] private long _index;
    [ObservableProperty] private double _percent;
    [ObservableProperty] private double _percentOfMax;
    [ObservableProperty] private PlayerInfoViewModel _player = new();
    [ObservableProperty] private ulong _value;
    
    // ? 新增: 技能列表刷新触发器
    // 每次FilteredSkillList改变时递增此值,触发绑定更新
    [ObservableProperty] private int _skillListRefreshTrigger = 0;

    public DebugFunctions Debug { get; } = debug;
    public SkillDataCollection Damage { get; } = new();
    public SkillDataCollection Heal { get; } = new();
    public SkillDataCollection TakenDamage { get; } = new();

    public int CompareTo(StatisticDataViewModel? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }

    public void Reset()
    {
        // Reset numeric fields
        Duration = 0;
        Index = 0;
        Percent = 0;
        PercentOfMax = 0;
        Value = 0;

        Damage.Reset();
        Heal.Reset();
        TakenDamage.Reset();

        // Reset player info
        Player = new PlayerInfoViewModel();
    }

    public partial class SkillDataCollection : BaseViewModel
    {
        [ObservableProperty] private IReadOnlyList<SkillItemViewModel> _filteredSkillList = [];
        [ObservableProperty] private IReadOnlyList<SkillItemViewModel> _totalSkillList = [];

        [ObservableProperty] private ObservableCollection<(TimeSpan duration, double section, double total)> _dps = new();

        public void Reset()
        {
            this.FilteredSkillList = [];
            this.TotalSkillList = [];
            if (Dps.Count > 0) this.Dps.Clear();
        }
        
        /// <summary>
        /// ? 新增: 从TotalSkillList重新过滤出FilteredSkillList
        /// 用于鼠标悬停时实时刷新技能列表
        /// </summary>
        /// <param name="limit">显示条数限制,0表示显示全部</param>
        public void RefreshFilteredList(int limit)
        {
            if (TotalSkillList == null || TotalSkillList.Count == 0)
            {
                FilteredSkillList = [];
                return;
            }
          
            // 从TotalSkillList中取Top N
    var newFiltered = limit > 0 
        ? TotalSkillList.Take(limit).ToList() 
     : TotalSkillList.ToList();
        
         // ? 关键: 赋值会触发PropertyChanged,通知UI更新
    FilteredSkillList = newFiltered;
        }

        public event Action<IReadOnlyList<SkillItemViewModel>?>? SkillChanged;

        protected virtual void RaiseSkillChanged(IReadOnlyList<SkillItemViewModel> obj)
        {
            SkillChanged?.Invoke(obj);
        }

        partial void OnFilteredSkillListChanged(IReadOnlyList<SkillItemViewModel> value)
        {
            RaiseSkillChanged(value);
        }

        partial void OnTotalSkillListChanged(IReadOnlyList<SkillItemViewModel> value)
        {
            RaiseSkillChanged(value);
        }
    }
}