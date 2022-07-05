using CommunityToolkit.Mvvm.ComponentModel;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels;

[INotifyPropertyChanged]
public partial class PlexActivityViewModel
{
    private IPlexActivityService plexActivityService;
    
    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private string currentActivity = "Idle";

    public PlexActivityViewModel(IPlexActivityService plexActivityService)
    {
        this.plexActivityService = plexActivityService;
    }
}