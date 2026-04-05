using Argus.Defender.Guard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace Argus.GUI.ViewModels;

public sealed partial class PrivacyGuardViewModel : ObservableObject
{
    private readonly GuardEnforcer _enforcer;
    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private int _changesCount;

    public ObservableCollection<GuardToggle> Toggles { get; } = new();

    public IEnumerable<IGrouping<string, GuardToggle>> GroupedToggles =>
        Toggles.GroupBy(t => t.Category);

    public PrivacyGuardViewModel(GuardEnforcer enforcer)
    {
        _enforcer = enforcer;
        LoadToggles();
    }

    private void LoadToggles()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "Guard", "GuardConfig.json");
        if (!File.Exists(configPath)) return;
        var config = JsonSerializer.Deserialize<GuardConfig>(File.ReadAllText(configPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (config is null) return;
        foreach (var t in config.Toggles) Toggles.Add(t);
    }

    [RelayCommand]
    private void EnableAll() { foreach (var t in Toggles) t.Enabled = true; }

    [RelayCommand]
    private void DisableAll() { foreach (var t in Toggles) t.Enabled = false; }

    [RelayCommand]
    private async Task ApplySelectedAsync()
    {
        StatusMessage = "Applying privacy settings...";
        // Wrap selected toggles in a GuardConfig for the enforcer API
        var selected = new GuardConfig
        {
            Toggles = Toggles.Where(t => t.Enabled).ToList()
        };
        await Task.Run(() => _enforcer.ApplyAll(selected));
        StatusMessage = $"Applied {Toggles.Count(t => t.Enabled)} privacy settings.";
    }
}
