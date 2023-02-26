using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Account;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels;

[INotifyPropertyChanged]
public partial class ServersPageViewModel
{
    public ObservableCollection<AccountServer> Servers { get; set; } = new();
    private readonly IPlexAccountClient plexAccountClient;
    private readonly IStorageService storageService;
    private readonly INavigationService navigationService;
    private readonly ILogger<PlexActivityPageViewModel> logger;

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string thumbnailUrl = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ValidateServerSelectionCommand))]
    private AccountServer? selectedServer;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ValidateServerSelectionCommand))]
    private string customServerIp = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ValidateServerSelectionCommand))]
    private string customServerPort = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ValidateServerSelectionCommand))]
    private bool useCustomServer;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ValidateServerSelectionCommand))]
    private bool isCustomServerOwned;

    public bool CanValidate => SelectedServer is not null ||
                               (
                                   UseCustomServer
                                   && !string.IsNullOrEmpty(CustomServerIp)
                                   && !string.IsNullOrEmpty(CustomServerPort)
                               );

    public ServersPageViewModel(
        IPlexAccountClient plexAccountClient,
        IStorageService storageService,
        INavigationService navigationService, ILogger<PlexActivityPageViewModel> logger)
    {
        this.plexAccountClient = plexAccountClient;
        this.storageService = storageService;
        this.navigationService = navigationService;
        this.logger = logger;
    }

    [RelayCommand]
    private async Task GetData()
    {
        try
        {
            await Task.WhenAll(
                GetServers(),
                GetUsernameAndThumbnail()
            );
        }
        catch (ApplicationException e)
        {
            if (e.Message is "Unsuccessful response from 3rd Party API")
            {
                logger.LogError("No PLEX token is settings going back to login screen");
                await this.navigationService.NavigateToAsync("login");
            }
        }
    }

    private async Task GetUsernameAndThumbnail()
    {
        string plexToken = await this.storageService.GetAsync("plex_token");
        PlexAccount account = await this.plexAccountClient.GetPlexAccountAsync(plexToken);
        this.Username = account.Username;
        this.ThumbnailUrl = account.Thumb;
    }

    private async Task GetServers()
    {
        string plexToken = await this.storageService.GetAsync("plex_token");
        AccountServerContainer serverContainer = await this.plexAccountClient.GetAccountServersAsync(plexToken);

        foreach (AccountServer server in serverContainer.Servers)
        {
            Servers.Add(server);
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false, CanExecute = "CanValidate")]
    private async Task ValidateServerSelection()
    {
        (string selectedServerIp, int selectedServerPort, bool isSelectedServerOwned) = GetSelectedServerInfo();

        await this.storageService.PutAsync("serverIp", selectedServerIp);
        await this.storageService.PutAsync("serverPort", selectedServerPort.ToString());
        await this.storageService.PutAsync("isServerOwned", isSelectedServerOwned.ToString());

        await this.navigationService.NavigateToAsync("activity");
    }

    private (string, int, bool) GetSelectedServerInfo()
    {
        if (this.UseCustomServer)
        {
            return (this.CustomServerIp, int.Parse(this.CustomServerPort), this.IsCustomServerOwned);
        }
        else if (this.SelectedServer is not null)
        {
            return (this.SelectedServer.Address, this.SelectedServer.Port, this.SelectedServer.Owned == 1);
        }

        throw new ArgumentException("No server selected");
    }

    [RelayCommand]
    private async Task LogOut()
    {
        await this.storageService.RemoveAsync("plex_token");
        await this.navigationService.NavigateToAsync("login");
    }
}