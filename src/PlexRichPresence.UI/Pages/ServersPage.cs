using CommunityToolkit.Maui.Markup;
using PlexRichPresence.UI.Pages.Base;
using PlexRichPresence.ViewModels;


namespace PlexRichPresence.UI.Pages;

public class ServersPage : BaseContentPage<ServersPageViewModel>
{
    public ServersPage(ServersPageViewModel viewModel) : base(viewModel)
    {
        Title = "Choose a PLEX server";
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Margin = new Thickness(0, 16, 0, 0),
                Children =
                {
                    new VerticalStackLayout
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        Spacing = 16,
                        Children =
                        {
                            new HorizontalStackLayout
                            {
                                HorizontalOptions = LayoutOptions.Center,
                                Spacing = 8,
                                Children =
                                {
                                    new Label().Text("Welcome"),
                                    new Label().Bind(Label.TextProperty, nameof(ServersPageViewModel.Username),
                                        BindingMode.OneWay)
                                }
                            },
                            new Image
                            {
                                HorizontalOptions = LayoutOptions.Center,
                                WidthRequest = 128,
                                HeightRequest = 128
                            }.Bind(Image.SourceProperty, nameof(ServersPageViewModel.ThumbnailUrl),
                                BindingMode.OneWay,
                                (string url) => url == null ? null : ImageSource.FromUri(new Uri(url))
                            ),
                            new Label().Text("Please choose a server from the list : "),
                            new ListView
                                {
                                    HorizontalOptions = LayoutOptions.Center,
                                    BackgroundColor = Colors.Transparent,
                                    SelectionMode = ListViewSelectionMode.Single,
                                    ItemTemplate = new ServerItemTemplate()
                                }
                                .Bind(ListView.ItemsSourceProperty, nameof(ServersPageViewModel.Servers))
                                .Bind(ListView.SelectedItemProperty, nameof(ServersPageViewModel.SelectedServer),
                                    BindingMode.OneWayToSource),
                            new Button
                            {
                                HorizontalOptions = LayoutOptions.Center
                            }.Text("Continue")
                        }
                    }.Bind(
                        IsVisibleProperty,
                        nameof(this.ViewModel.Loading),
                        BindingMode.OneWay,
                        (bool loading) => loading ? Visibility.Visible : Visibility.Hidden),
                    new ActivityIndicator
                        {
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            IsRunning = true
                        }
                        .Bind(
                            ActivityIndicator.IsRunningProperty,
                            nameof(this.ViewModel.Loading),
                            BindingMode.OneWay
                        )
                }
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Dispatcher.Dispatch(async () => { await this.ViewModel.GetData(); });
    }
}

public class ServerItemTemplate : DataTemplate
{
    public ServerItemTemplate() : base(GetContent)
    {
    }

    static TextCell GetContent() => new TextCell()
        .Bind(TextCell.TextProperty, "Name")
        .Bind(TextCell.DetailProperty, "Address");
}