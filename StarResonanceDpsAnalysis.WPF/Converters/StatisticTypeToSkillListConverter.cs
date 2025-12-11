using System.Globalization;
using System.Windows.Data;
using StarResonanceDpsAnalysis.WPF.Models;
using StarResonanceDpsAnalysis.WPF.ViewModels;

namespace StarResonanceDpsAnalysis.WPF.Converters;

/// <summary>
/// Converts current StatisticType to the appropriate skill list from StatisticDataViewModel
/// 根据统计类型返回对应的技能列表:
/// - Damage: 显示伤害技能(实际是玩家造成的技能)
/// - Healing: 显示治疗技能(实际是玩家治疗技能)
/// - TakenDamage: 显示承伤技能(即其实是怪物玩家的技能)
/// - NpcTakenDamage: 显示NPC承伤技能(玩家打NPC的技能 = NPC的TakenDamage)
/// 
/// ? 修改: Tooltip显示的技能数量遵循"技能显示条数"设置
/// </summary>
public class StatisticTypeToSkillListConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return null;

        // values[0]: StatisticDataViewModel (the data context - 当前行的数据)
        // values[1]: StatisticType (from parent ViewModel - 当前统计类型)
        // values[2]: int SkillListRefreshTrigger (触发器,每次改变强制刷新) ← 新增

        if (values[0] is not StatisticDataViewModel slot) return null;
        if (values[1] is not StatisticType statisticType) return null;

        // ? 使用 FilteredSkillList (过滤后的列表),遵循"技能显示设置"配置
        return statisticType switch
        {
            // ? DPS统计 - 显示Top N伤害技能
            StatisticType.Damage => slot.Damage.FilteredSkillList,

            // ? 治疗统计 - 显示Top N治疗技能
            StatisticType.Healing => slot.Heal.FilteredSkillList,

            // ? 玩家承伤统计 - 显示Top N承伤技能
            StatisticType.TakenDamage => slot.TakenDamage.FilteredSkillList,

            // ? NPC承伤统计 - 显示Top N承伤
            StatisticType.NpcTakenDamage => slot.TakenDamage.FilteredSkillList,

            _ => slot.Damage.FilteredSkillList
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}