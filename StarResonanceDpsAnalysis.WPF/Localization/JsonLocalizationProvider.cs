using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using WPFLocalizeExtension.Providers;

namespace StarResonanceDpsAnalysis.WPF.Localization;

public class JsonLocalizationProvider : ILocalizationProvider
{
    private readonly string _basePath;

    private readonly (string resourceName, string pattern)[] _filenamePatterns =
    [
        ("Monster", "Monster\\monster.{0}.json"),
        ("DebugData", "DebugData\\debugData.{0}.json"),
        ("Skills", "Skill\\skills.{0}.json")
    ];

    private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _resources = new();

    public JsonLocalizationProvider(string basePath)
    {
        _basePath = basePath;
        AvailableCultures = new ObservableCollection<CultureInfo>(GetAvailableCultures());
    }

    public FullyQualifiedResourceKeyBase? GetFullyQualifiedResourceKey(string key, DependencyObject? target)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        var parts = key.Split(':');
        string? assembly = null;
        string? dictionary = null;
        var realKey = key;

        if (parts.Length == 1)
        {
            // Only key
            realKey = parts[0];
        }
        else if (parts.Length == 2)
        {
            // Dictionary:Key
            dictionary = parts[0];
            realKey = parts[1];
        }
        else if (parts.Length >= 3)
        {
            // Assembly:Dictionary:Key
            assembly = parts[0];
            dictionary = parts[1];
            realKey = parts[^1];
        }

        return new FQAssemblyDictionaryKey(realKey, assembly, dictionary);
    }

    public object? GetLocalizedObject(string? key, DependencyObject? target, CultureInfo? culture)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        var fqKey = GetFullyQualifiedResourceKey(key, target) as FQAssemblyDictionaryKey;
        if (fqKey == null)
            return null;

        var lookupKey = fqKey.Key;
        var dictionary = fqKey.Dictionary?.ToLowerInvariant();

        var current = culture ?? CultureInfo.CurrentUICulture;

        // Try culture chain
        while (!Equals(current, CultureInfo.InvariantCulture))
        {
            var lang = current.Name;
            EnsureResourcesLoaded(lang);

            if (_resources.TryGetValue(lang, out var dicts))
            {
                if (dictionary != null)
                {
                    // Dictionary specified
                    if (dicts.TryGetValue(dictionary, out var d) && d.TryGetValue(lookupKey, out var val))
                        return val;
                }
                else
                {
                    // No dictionary given → search all
                    foreach (var d in dicts.Values)
                    {
                        if (d.TryGetValue(lookupKey, out var val))
                            return val;
                    }
                }
            }

            current = current.Parent;
        }

        return null;
    }

    public ObservableCollection<CultureInfo>? AvailableCultures { get; }
    public event ProviderChangedEventHandler? ProviderChanged;
    public event ProviderErrorEventHandler? ProviderError;
    public event ValueChangedEventHandler? ValueChanged;

    private void EnsureResourcesLoaded(string lang)
    {
        if (_resources.ContainsKey(lang))
            return;

        var dicts = new Dictionary<string, Dictionary<string, string>>();
        _resources[lang] = dicts;

        foreach (var (name, pattern) in _filenamePatterns)
        {
            var path = Path.Combine(_basePath, string.Format(pattern, lang));
            if (!File.Exists(path))
                continue;

            try
            {
                var json = File.ReadAllText(path);
                var d = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (d != null)
                    dicts[name.ToLowerInvariant()] = d;
            }
            catch
            {
                // ignore malformed files
            }
        }
    }

    public void UpdateCultureResources(CultureInfo culture)
    {
        _resources.Remove(culture.Name);
        ProviderChanged?.Invoke(this, new ProviderChangedEventArgs(null));
    }

    // Discover cultures based on JSON files
    private IEnumerable<CultureInfo> GetAvailableCultures()
    {
        if (!Directory.Exists(_basePath))
            yield break;

        var found = new HashSet<CultureInfo>();

        foreach (var (resName, pattern) in _filenamePatterns)
        {
            var folder = Path.GetDirectoryName(pattern);
            var fullFolder = Path.Combine(_basePath, folder ?? "");

            if (!Directory.Exists(fullFolder))
                continue;

            foreach (var file in Directory.GetFiles(fullFolder, "*.json"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var parts = name.Split('.');
                if (parts.Length != 2 || !parts[0].Equals(resName, StringComparison.OrdinalIgnoreCase)) continue;
                try
                {
                    found.Add(new CultureInfo(parts[1]));
                }
                catch
                {
                    // ignored
                }
            }
        }

        foreach (var c in found)
            yield return c;
    }

    protected virtual void OnProviderError(ProviderErrorEventArgs args)
    {
        ProviderError?.Invoke(this, args);
    }

    protected virtual void OnValueChanged(ValueChangedEventArgs args)
    {
        ValueChanged?.Invoke(this, args);
    }
}