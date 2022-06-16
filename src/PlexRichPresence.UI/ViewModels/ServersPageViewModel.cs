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
using System.Xml;
using System.Xml.Linq;

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
    
    public async Task GetServers()
    {
        string plexToken = await SecureStorage.GetAsync("plex_token");
        PlexAccount account = await Task.Run(() => this.plexFactory.GetPlexAccount(plexToken));
        Debug.WriteLine(plexToken);
        try
        {
            //var servers = (await Task.Run(() => account.Servers().Result)).ToList();

            var response = await new HttpClient().SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://plex.tv/pms/servers.xml/")
            });
            string readAsStreamAsync = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(readAsStreamAsync);
            //Debug.WriteLine(servers.Count);
            //Debug.WriteLine(servers[0].Name);
            //foreach (Server server in servers)
            //{
            //    Debug.WriteLine(server.Name);
            //    Debug.WriteLine(server.Address);
            //    Servers.Add(server);
            //}
        }catch (Exception e)
        {
            Debug.WriteLine(e.StackTrace);
        }

        /*try
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
        catch (Exception e)
        {
            Debug.WriteLine(e.StackTrace);
        }*/
    }
}

