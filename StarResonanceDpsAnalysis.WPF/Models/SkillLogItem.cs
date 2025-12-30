using System;

namespace StarResonanceDpsAnalysis.WPF.Models;

public class SkillLogItem
{
    public DateTime Timestamp { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public long TotalValue { get; set; }
    public int Count { get; set; }
    public int CritCount { get; set; }
    public int LuckyCount { get; set; }
    public bool IsHeal { get; set; }
    
    // Helper for display
    public bool IsMultiHit => Count > 1;
    public bool HasCrit => CritCount > 0;
    public bool HasLucky => LuckyCount > 0;
}
