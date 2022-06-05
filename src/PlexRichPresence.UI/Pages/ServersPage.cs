using CommunityToolkit.Maui.Markup;
using Plex.Library.ApiModels.Servers;
using PlexRichPresence.UI.Pages.Base;
using PlexRichPresence.UI.ViewModels;


namespace PlexRichPresence.UI.Pages;
public class ServersPage : BaseContentPage<ServersPageViewModel>
{
    public ServersPage(ServersPageViewModel viewModel) : base(viewModel)
    {
       
        Title = "Choose a PLEX server";
        Content = new ScrollView
        {
            Content = new ListView
            {
                BackgroundColor = Colors.Transparent,
                SelectionMode = ListViewSelectionMode.Single,
                ItemTemplate = new ServerItemTemplate()
            }
            .Bind(ListView.ItemsSourceProperty, nameof(ServersPageViewModel.Servers))
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        this.ViewModel.GetServersCommand.ExecuteAsync(null);
    }

}

public class ServerItemTemplate : DataTemplate
{
    public ServerItemTemplate() : base(GetContent)
    {

    }

    static TextCell GetContent() => new TextCell()
        .Bind(TextCell.TextProperty, "Name")
        .Bind(TextCell.TextProperty, "Address");
}

