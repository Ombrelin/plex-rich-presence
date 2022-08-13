using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    [ObservableProperty] private string username;
    [ObservableProperty] private string thumbnailUrl;
    [ObservableProperty] private bool loading = true;
    [ObservableProperty] private AccountServer? selectedServer;
    [ObservableProperty] private string customServerIp;
    [ObservableProperty] private string customServerPort;
    [ObservableProperty] private bool useCustomServer;

    public ServersPageViewModel(
        IPlexAccountClient plexAccountClient,
        IStorageService storageService,
        INavigationService navigationService)
    {
        this.plexAccountClient = plexAccountClient;
        this.storageService = storageService;
        this.navigationService = navigationService;
    }

    [RelayCommand]
    public async Task GetData()
    {
        await Task.WhenAll(
            this.GetServers(),
            this.GetUsernameAndThumbnail()
        );
        this.Loading = false;
    }

    public async Task GetUsernameAndThumbnail()
    {
        string plexToken = await this.storageService.GetAsync("plex_token");
        PlexAccount account = await this.plexAccountClient.GetPlexAccountAsync(plexToken);
        this.Username = account.Username;
        this.ThumbnailUrl = account.Thumb;
    }

    public async Task GetServers()
    {
        string plexToken = await this.storageService.GetAsync("plex_token");
        AccountServerContainer serverContainer = await this.plexAccountClient.GetAccountServersAsync(plexToken);

        foreach (AccountServer server in serverContainer.Servers)
        {
            Servers.Add(server);
        }
    }

    [RelayCommand]
    public async Task ValidateServerSelection()
    {
        var (selectedServerIp, selectedServerPort) = GetSelectedServerInfo();

        await this.storageService.PutAsync("serverIp", selectedServerIp);
        await this.storageService.PutAsync("serverPort", selectedServerPort.ToString());

        await this.navigationService.NavigateToAsync("activity");
    }

    private (string, int) GetSelectedServerInfo()
    {
        if (this.UseCustomServer)
        {
            return (this.CustomServerIp, int.Parse(this.CustomServerPort));
        }
        else if (this.SelectedServer is not null)
        {
            return (this.SelectedServer.Address, this.SelectedServer.Port);
        }

        throw new ArgumentException("No server selected");
    }
}