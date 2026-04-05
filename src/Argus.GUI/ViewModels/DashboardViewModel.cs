using Argus.Core;
using Argus.GUI.IPC;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Argus.GUI.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject
{
    private readonly GuiPipeBridge _pipeBridge;

    [ObservableProperty] private int _threatsDetected;
    [ObservableProperty] private int _filesScanned;
    [ObservableProperty] private int _quarantinedItems;
    [ObservableProperty] private string _protectionStatus = "Offline";
    [ObservableProperty] private string _lastScanTime = "Never";
    [ObservableProperty] private string _watchdogStatus = "—";
    [ObservableProperty] private string _defenderStatus = "—";
    [ObservableProperty] private string _serviceLabel = "Service: connecting...";

    public DashboardViewModel(GuiPipeBridge pipeBridge)
    {
        _pipeBridge = pipeBridge;
        _pipeBridge.StatusUpdated += OnStatusUpdated;
    }

    private void OnStatusUpdated(ServiceStatus status)
    {
        ProtectionStatus = status.DefenderActive ? "Active" : "Inactive";
        ThreatsDetected = status.ThreatsDetected;
        FilesScanned = status.FilesScanned;
        QuarantinedItems = status.QuarantinedItems;
        LastScanTime = status.LastScanTime?.ToString("MMM dd, yyyy HH:mm") ?? "Never";
        WatchdogStatus = status.WatchdogStatus ?? "—";
        DefenderStatus = status.DefenderStatus ?? "—";
        ServiceLabel = status.ServiceRunning ? "Service: running" : "Service: stopped";
    }
}
