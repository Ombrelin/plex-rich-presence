using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlexRichPresence.PlexActivity;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels;

[INotifyPropertyChanged]
public partial class PlexActivityPageViewModel
{
    private readonly IPlexActivityService plexActivityService;
    private IPlexSessionStrategy? plexActivityStrategy;
    private readonly IStorageService storageService;
    private readonly INavigationService navigationService;
    private string userToken = string.Empty;
    private string userId = string.Empty;

    [ObservableProperty] private string currentActivity = "Idle";
    [ObservableProperty] private string plexServerIp = string.Empty;
    [ObservableProperty] private int plexServerPort;
    [ObservableProperty] private bool isPlexServerOwned;

    public PlexActivityPageViewModel(
        IPlexActivityService plexActivityService,
        IStorageService storageService,
        INavigationService navigationService
    )
    {
        this.plexActivityService = plexActivityService;
        this.storageService = storageService;
        this.navigationService = navigationService;
    }

    [RelayCommand]
    private async Task InitStrategy()
    {
        this.PlexServerIp = await this.storageService.GetAsync("serverIp");
        this.PlexServerPort = int.Parse(await this.storageService.GetAsync("serverPort"));
        this.userToken = await this.storageService.GetAsync("plex_token");
        this.IsPlexServerOwned = bool.Parse(await this.storageService.GetAsync("isServerOwned"));

        this.plexActivityStrategy = this.plexActivityService.GetStrategy(this.IsPlexServerOwned);
    }

    [RelayCommand]
    private async Task StartActivity()
    {
        if (this.plexActivityStrategy is null)
        {
            throw new InvalidOperationException("Strategy not initialised");
        }

        await foreach (PlexSession session in this.plexActivityStrategy.GetSessions(
                           userId,
                           this.PlexServerIp,
                           this.PlexServerPort,
                           this.userToken)
                      )
        {
            this.CurrentActivity = session.Title;
        }
    }

    [RelayCommand]
    private async Task ChangeServer()
    {
        await this.navigationService.NavigateToAsync("servers");
    }
}