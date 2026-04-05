using CommunityToolkit.Mvvm.ComponentModel;

namespace Argus.GUI.ViewModels;

public sealed partial class QuarantineViewModel : ObservableObject
{
    [ObservableProperty] private int _itemCount;
    [ObservableProperty] private string _status = "No files in quarantine";
}
