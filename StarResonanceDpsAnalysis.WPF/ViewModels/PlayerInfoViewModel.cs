using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using StarResonanceDpsAnalysis.Core.Models;
using StarResonanceDpsAnalysis.WPF.Helpers;
using StarResonanceDpsAnalysis.WPF.Localization;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

public partial class PlayerInfoViewModel : BaseViewModel
{
    private readonly LocalizationManager _localizationManager;

    [ObservableProperty] private Classes _class = Classes.Unknown;

    /// <summary>
    /// ª√√Œµ»º∂ Dream strength
    /// </summary>
    [ObservableProperty] private int _dreamStrength;

    [ObservableProperty] private string _guild = string.Empty;
    [ObservableProperty] private bool _isNpc;
    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private string _playerInfo = string.Empty;
    [ObservableProperty] private int _powerLevel;
    [ObservableProperty] private ClassSpec _spec = ClassSpec.Unknown;
    [ObservableProperty] private long _uid;
    [ObservableProperty] private bool _mask;

    public PlayerInfoViewModel(LocalizationManager localizationManager)
    {
        _localizationManager = localizationManager;
        _localizationManager.CultureChanged += LocalizationManagerOnCultureChanged;
        PropertyChanged += OnPropertyChanged;
    }

    private void LocalizationManagerOnCultureChanged(object? sender, CultureInfo e)
    {
        UpdatePlayerInfo();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != "PlayerInfo")
        {
            UpdatePlayerInfo();
        }
    }

    private void UpdatePlayerInfo()
    {
        // if player info 
        PlayerInfo = IsNpc
            ? _localizationManager.GetString($"Monster:{Uid}")
            : $"{GetName()} - {GetSpec()} ({PowerLevel}-{DreamStrength})";// Name - Class Spec (PowerLevel-DreamStrength)

        return;

        string GetName()
        {
            var hasName = !string.IsNullOrWhiteSpace(Name);
            var name = hasName switch
            {
                true => Mask ? NameMasker.Mask(Name) : Name,
                false => $"UID:{(Mask ? NameMasker.Mask(Uid.ToString()) : Uid.ToString())}",
            };
            return name;
        }

        string GetSpec()
        {
            var rr = _localizationManager.GetString("ClassSpec_" + Spec);
            return rr;
        }
    }
}