using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Plex.ServerApi.Clients.Interfaces;
using Plex.ServerApi.PlexModels.Account;

namespace PlexRichPresence.ViewModels;

[INotifyPropertyChanged]
public partial class ServersPageViewModel
{
    public ObservableCollection<AccountServer> Servers { get; set; } = new();
    private readonly IPlexAccountClient plexAccountClient;

    [ObservableProperty] private string username;
    [ObservableProperty] private string thumbnailUrl;
    [ObservableProperty] private bool loading = true;
    [ObservableProperty] private AccountServer selectedServer;
    
    public ServersPageViewModel(IPlexAccountClient plexAccountClient)
    {
        this.plexAccountClient = plexAccountClient;
    }

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
        string plexToken = await SecureStorage.GetAsync("plex_token");
        PlexAccount account = await this.plexAccountClient.GetPlexAccountAsync(plexToken);
        this.Username = account.Username;
        this.ThumbnailUrl = account.Thumb;
    }

    public async Task GetServers()
    {
        string plexToken = await SecureStorage.GetAsync("plex_token");
        AccountServerContainer serverContainer = await this.plexAccountClient.GetAccountServersAsync(plexToken);

        foreach (AccountServer server in serverContainer.Servers)
        {
            Servers.Add(server);
        }
    }
}