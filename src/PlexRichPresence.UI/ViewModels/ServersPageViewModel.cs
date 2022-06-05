using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plex.Api.Factories;
using Plex.Library.ApiModels.Accounts;
using Plex.Library.ApiModels.Servers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlexRichPresence.UI.ViewModels;

[INotifyPropertyChanged]
public partial class ServersPageViewModel
{

    public ObservableCollection<Server> Servers { get; set; } = new();
    private readonly IPlexFactory plexFactory;

    public ServersPageViewModel(IPlexFactory plexFactory)
    {
        this.plexFactory = plexFactory;
        
    }

    [ICommand]
    public async Task GetServers()
    {
        string plexToken = await SecureStorage.GetAsync("plex_token");
        PlexAccount account = this.plexFactory.GetPlexAccount(plexToken);
        Debug.WriteLine(account.Username);
        
        foreach (Server server in await account.Servers())
        {
            Debug.WriteLine(server.Name);
            Debug.WriteLine(server.Address);
            //Servers.Add(server);
        }
    }
}

