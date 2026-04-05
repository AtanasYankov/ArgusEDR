using CommunityToolkit.Mvvm.ComponentModel;

namespace Argus.GUI.ViewModels;

public sealed partial class ScannerViewModel : ObservableObject
{
    [ObservableProperty] private string _status = "Scanner ready - awaiting IPC integration";
}
