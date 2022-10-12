using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PlexRichPresence.PlexActivity;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels;

[INotifyPropertyChanged]
public partial class PlexActivityPageViewModel
{
    private readonly IPlexActivityService plexActivityService;
    private readonly IStorageService storageService;
    private readonly IDiscordService discordService;
    private readonly INavigationService navigationService;
    private string userToken = string.Empty;
    private string userName = string.Empty;
    private ILogger<PlexActivityPageViewModel> logger;

    [ObservableProperty] private string currentActivity = "Idle";
    [ObservableProperty] private string plexServerIp = string.Empty;
    [ObservableProperty] private int plexServerPort;
    [ObservableProperty] private bool isPlexServerOwned;
    [ObservableProperty] private bool isServerUnreachable;

    public PlexActivityPageViewModel(
        IPlexActivityService plexActivityService,
        IStorageService storageService,
        INavigationService navigationService,
        IDiscordService discordService,
        ILogger<PlexActivityPageViewModel> logger
        )
    {
        this.plexActivityService = plexActivityService;
        this.storageService = storageService;
        this.navigationService = navigationService;
        this.discordService = discordService;
        this.logger = logger;
    }



    [RelayCommand]
    private async Task InitStrategy()
    {
        this.PlexServerIp = await this.storageService.GetAsync("serverIp");
        this.PlexServerPort = int.Parse(await this.storageService.GetAsync("serverPort"));
        this.userToken = await this.storageService.GetAsync("plex_token");
        this.userName = await this.storageService.GetAsync("plexUserName");
        this.IsPlexServerOwned = bool.Parse(await this.storageService.GetAsync("isServerOwned"));
    }

    [RelayCommand]
    private async Task StartActivity()
    {
        try
        {
            await foreach (IPlexSession session in this.plexActivityService.GetSessions(
                               this.IsPlexServerOwned,
                               userName,
                               this.PlexServerIp,
                               this.PlexServerPort,
                               this.userToken)
                          )
            {
                this.CurrentActivity = session.MediaTitle;
                this.discordService.SetDiscordPresenceToPlexSession(session);
            }
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Could not connect to server");
            this.IsServerUnreachable = true;
        }

    }

    [RelayCommand]
    private async Task ChangeServer()
    {
        this.plexActivityService.Disconnect();
        await this.navigationService.NavigateToAsync("servers");
    }
}