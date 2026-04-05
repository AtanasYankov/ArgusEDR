using CommunityToolkit.Mvvm.ComponentModel;

namespace Argus.GUI.ViewModels;

public sealed partial class DefenderViewModel : ObservableObject
{
    [ObservableProperty] private string _status = "Real-time protection active";
}
