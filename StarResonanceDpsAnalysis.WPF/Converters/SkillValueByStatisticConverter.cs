using System;
using System.Globalization;
using System.Windows.Data;
using StarResonanceDpsAnalysis.WPF.Models;
using StarResonanceDpsAnalysis.WPF.ViewModels;

namespace StarResonanceDpsAnalysis.WPF.Converters;

/// <summary>
/// Returns a skill value property (TotalValue/HitCount/CritCount/Average/etc.)
/// for the given statistic type (Damage/Healing/TakenDamage) and SkillItemViewModel.
/// </summary>
public class SkillValueByStatisticConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [var statObj, var skillObj])
        {
            return Binding.DoNothing;
        }

        if (statObj is not StatisticType statistic || skillObj is not SkillItemViewModel skill)
        {
            return Binding.DoNothing;
        }

        var propertyName = parameter as string;
        var skillValue = statistic switch
        {
            StatisticType.Healing => skill.Heal,
            StatisticType.TakenDamage or StatisticType.NpcTakenDamage => skill.TakenDamage,
            _ => skill.Damage
        };

        if (skillValue == null)
        {
            return Binding.DoNothing;
        }

        object result = propertyName switch
        {
            nameof(SkillItemViewModel.SkillValue.TotalValue) => skillValue.TotalValue,
            nameof(SkillItemViewModel.SkillValue.HitCount) => skillValue.HitCount,
            nameof(SkillItemViewModel.SkillValue.CritCount) => skillValue.CritCount,
            nameof(SkillItemViewModel.SkillValue.Average) => skillValue.Average,
            nameof(SkillItemViewModel.SkillValue.CritRate) => skillValue.CritRate,
            nameof(SkillItemViewModel.SkillValue.LuckyCount) => skillValue.LuckyCount,
            nameof(SkillItemViewModel.SkillValue.LuckyValue) => skillValue.LuckyValue,
            nameof(SkillItemViewModel.SkillValue.NormalValue) => skillValue.NormalValue,
            nameof(SkillItemViewModel.SkillValue.CritValue) => skillValue.CritValue,
            _ => Binding.DoNothing
        };

        if (result == Binding.DoNothing)
        {
            return Binding.DoNothing;
        }

        return System.Convert.ToString(result, culture) ?? string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
