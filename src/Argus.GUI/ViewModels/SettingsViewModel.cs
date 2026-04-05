using Argus.Defender.Dns;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Argus.GUI.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly DnsProtectionService _dns;

    public ObservableCollection<DnsProfile> DnsProfiles { get; } =
        new(DnsProfile.All);

    [ObservableProperty] private DnsProfile _selectedDnsProfile;
    [ObservableProperty] private string _statusMessage = "Select a DNS profile and click Apply.";

    public SettingsViewModel(DnsProtectionService dns)
    {
        _dns = dns;
        _selectedDnsProfile = dns.CurrentProfile;
    }

    [RelayCommand]
    private void ApplyDns()
    {
        try
        {
            _dns.Apply(SelectedDnsProfile);
            StatusMessage = SelectedDnsProfile == DnsProfile.System
                ? "DNS reset to system default."
                : $"DNS set to {SelectedDnsProfile.Name} ({SelectedDnsProfile.Primary}).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to apply DNS settings: {ex.Message}";
            Serilog.Log.Error(ex, "Failed to apply DNS settings");
        }
    }
}