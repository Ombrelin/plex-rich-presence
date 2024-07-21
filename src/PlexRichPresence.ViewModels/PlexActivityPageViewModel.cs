using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels;

[INotifyPropertyChanged]
public partial class PlexActivityPageViewModel
{
    private readonly IPlexActivityService _plexActivityService;
    private readonly IStorageService _storageService;
    private readonly IDiscordService _discordService;
    private readonly INavigationService _navigationService;
    private string _userToken = string.Empty;
    private string _userName = string.Empty;
    private readonly ILogger<PlexActivityPageViewModel> _logger;

    [ObservableProperty] private string _currentActivity = "Idle";
    [ObservableProperty] private string _plexServerIp = string.Empty;
    [ObservableProperty] private int _plexServerPort;
    [ObservableProperty] private bool _isPlexServerOwned;
    [ObservableProperty] private bool _isServerUnreachable;
    [ObservableProperty] private bool _enableIdleStatus = true;
    [ObservableProperty] private string _thumbnailUrl = string.Empty;

    public PlexActivityPageViewModel(IPlexActivityService plexActivityService, IStorageService storageService, INavigationService navigationService, IDiscordService discordService, ILogger<PlexActivityPageViewModel> logger)
    {
        _plexActivityService = plexActivityService;
        _storageService = storageService;
        _navigationService = navigationService;
        _discordService = discordService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task InitStrategy()
    {
        PlexServerIp = await _storageService.GetAsync("serverIp");
        PlexServerPort = int.Parse(await _storageService.GetAsync("serverPort"));

        if (await _storageService.ContainsKeyAsync("plex_token"))
        {
            await InitStrategyWithToken();
        }
        else
        {
            _logger.LogError("No PLEX token is settings going back to login screen");
            await _navigationService.NavigateToAsync("login");
        }
    }

    private async Task InitStrategyWithToken()
    {
        _userToken = await _storageService.GetAsync("plex_token");
        _userName = await _storageService.GetAsync("plexUserName");
        IsPlexServerOwned = bool.Parse(await _storageService.GetAsync("isServerOwned"));
        if (await _storageService.ContainsKeyAsync("enableIdleStatus"))
        {
            EnableIdleStatus = bool.Parse(await _storageService.GetAsync("enableIdleStatus"));
        }
    }

    [RelayCommand]
    private async Task StartActivity()
    {
        try
        {
            await foreach (var session in _plexActivityService.GetSessions(IsPlexServerOwned, _userName, PlexServerIp, PlexServerPort, _userToken))
            {
                CurrentActivity = session.MediaTitle;
                ThumbnailUrl = session.Thumbnail;

                if (CurrentActivity is "Idle" && !EnableIdleStatus)
                    _discordService.StopRichPresence();
                else
                    _discordService.SetDiscordPresenceToPlexSession(session);
            }
        }
        catch (ApplicationException e)
        {
            if (e.Message is "Unsuccessful response from 3rd Party API")
            {
                _logger.LogError("No PLEX token in settings going back to login screen");
                await _navigationService.NavigateToAsync("login");
            }
            else
            {
                _logger.LogError(e, "Could not connect to server");
                IsServerUnreachable = true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not connect to server");
            IsServerUnreachable = true;
        }
    }

    [RelayCommand]
    private async Task ChangeServer()
    {
        _plexActivityService.Disconnect();
        await _navigationService.NavigateToAsync("servers");
    }
}