using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PlexRichPresence.Core;
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
    private readonly ILogger<PlexActivityPageViewModel> logger;

    [ObservableProperty] private string currentActivity = "Idle";
    [ObservableProperty] private string plexServerIp = string.Empty;
    [ObservableProperty] private int plexServerPort;
    [ObservableProperty] private bool isPlexServerOwned;
    [ObservableProperty] private bool isServerUnreachable;
    [ObservableProperty] private bool enableIdleStatus = true;
    [ObservableProperty] private string thumbnailUrl = string.Empty;

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
        PlexServerIp = await storageService.GetAsync("serverIp");
        PlexServerPort = int.Parse(await storageService.GetAsync("serverPort"));

        if (await storageService.ContainsKeyAsync("plex_token"))
        {
            await InitStrategyWithToken();
        }
        else
        {
            logger.LogError("No PLEX token is settings going back to login screen");
            await this.navigationService.NavigateToAsync("login");
        }
    }

    private async Task InitStrategyWithToken()
    {
        userToken = await storageService.GetAsync("plex_token");
        userName = await storageService.GetAsync("plexUserName");
        IsPlexServerOwned = bool.Parse(await storageService.GetAsync("isServerOwned"));
        if (await storageService.ContainsKeyAsync("enableIdleStatus"))
        {
            EnableIdleStatus = bool.Parse(await storageService.GetAsync("enableIdleStatus"));
        }
    }

    [RelayCommand]
    private async Task StartActivity()
    {
        try
        {
            await foreach (PlexSession session in plexActivityService.GetSessions(
                               IsPlexServerOwned,
                               userName,
                               PlexServerIp,
                               PlexServerPort,
                               userToken)
                          )
            {
                CurrentActivity = session.MediaTitle;
                ThumbnailUrl = session.Thumbnail;

                if (CurrentActivity is "Idle" && !EnableIdleStatus)
                {
                    discordService.StopRichPresence();
                }
                else
                {
                    discordService.SetDiscordPresenceToPlexSession(session);
                }
            }
        }
        catch (ApplicationException e)
        {
            if (e.Message is "Unsuccessful response from 3rd Party API")
            {
                logger.LogError("No PLEX token is settings going back to login screen");
                await this.navigationService.NavigateToAsync("login");
            }
            else
            {
                logger.LogError(e, "Could not connect to server");
                IsServerUnreachable = true;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Could not connect to server");
            IsServerUnreachable = true;
        }
    }

    [RelayCommand]
    private async Task ChangeServer()
    {
        plexActivityService.Disconnect();
        await navigationService.NavigateToAsync("servers");
    }
}