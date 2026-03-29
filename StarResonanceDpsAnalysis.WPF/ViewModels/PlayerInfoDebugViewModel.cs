using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarResonanceDpsAnalysis.Core.Data;
using StarResonanceDpsAnalysis.Core.Data.Models;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public partial class PlayerInfoDebugViewModel(IDataStorage storage): BaseViewModel
{

    [ObservableProperty] private IReadOnlyList<PlayerInfo> _allPlayerInfos = new List<PlayerInfo>();

    [ObservableProperty] private PlayerInfo? _selectedPlayerInfo;

    [ObservableProperty] private string _filterText = string.Empty;

    [ObservableProperty] private ICollectionView _filteredPlayerInfos = CollectionViewSource.GetDefaultView(new List<PlayerInfo>());

    partial void OnAllPlayerInfosChanged(IReadOnlyList<PlayerInfo> value)
    {
        FilteredPlayerInfos = CollectionViewSource.GetDefaultView(value);
        FilteredPlayerInfos.Filter = FilterPlayerInfo;
    }

    partial void OnFilterTextChanged(string value)
    {
        FilteredPlayerInfos.Refresh();
    }

    private bool FilterPlayerInfo(object item)
    {
        if (item is not PlayerInfo player) return false;
        if (string.IsNullOrWhiteSpace(FilterText)) return true;

        return (player.Name?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
               player.UID.ToString().Contains(FilterText);
    }

    [RelayCommand]
    private void Refresh()
    {
        if (storage != null)
        {
            AllPlayerInfos = storage.ReadOnlyPlayerInfoDatas.Values
                .ToList().AsReadOnly();
        }
    }

    public PlayerInfoDebugViewModel() : this(null!)
    {
        var list = new List<PlayerInfo>();
        var p1 = new PlayerInfo
        {
            Name = "Design Player 1",
            UID = 1001001,
            Level = 65,
            ProfessionID = 11,
            SubProfessionName = "Lightblade",
            CombatPower = 34500,
            HP = 120000,
            MaxHP = 120000,
            Critical = 500,
            Lucky = 200,
            CombatState = true,
            CombatStateTime = 123456789
        };
        list.Add(p1);
        list.Add(new PlayerInfo
        {
            Name = "Design Player 2",
            UID = 1001002,
            Level = 65,
            ProfessionID = 12,
            SubProfessionName = "Spellblade",
            CombatPower = 34600,
            HP = 90000,
            MaxHP = 95000,
        });

        _allPlayerInfos = list;
        _filteredPlayerInfos = CollectionViewSource.GetDefaultView(_allPlayerInfos);
        _selectedPlayerInfo = p1;
    }
}