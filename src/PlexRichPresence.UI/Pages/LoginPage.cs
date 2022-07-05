using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Layouts;
using PlexRichPresence.UI.Pages.Base;
using System.Diagnostics;
using PlexRichPresence.ViewModels;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace PlexRichPresence.UI.Pages;

internal class LoginPage : BaseContentPage<LoginPageViewModel>
{
    public LoginPage(LoginPageViewModel viewModel) : base(viewModel)
    {
        Title = "Login to PLEX";
        Content = new ScrollView
        {
            Content = new FlexLayout
            {
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Center,
                JustifyContent = FlexJustify.Center,
                Children =
                {
                    new Grid
                    {
                        WidthRequest = 305,
                        ColumnDefinitions = Columns.Define(
                            (Column.Description, Star),
                            (Column.Input, Stars(2))
                        ),
                        RowDefinitions = Rows.Define(
                            (Row.TextEntry, 36)),
                        Margin = new Thickness(0, 0, 0, 8),
                        Children =
                        {
                            new Label {VerticalOptions = LayoutOptions.Center}.Text("Login : ")
                                .Column(Column.Description).Row(Row.TextEntry),
                            new Entry
                                {
                                    WidthRequest = 200
                                }
                                .Column(Column.Input)
                                .Row(Row.TextEntry)
                                .Bind(Entry.TextProperty, nameof(LoginPageViewModel.Login), BindingMode.TwoWay)
                        }
                    },
                    new Grid
                    {
                        ColumnDefinitions = Columns.Define(
                            (Column.Description, Star),
                            (Column.Input, Stars(2))
                        ),
                        RowDefinitions = Rows.Define(
                            (Row.TextEntry, 36)),
                        WidthRequest = 305,
                        Margin = new Thickness(0, 0, 0, 8),
                        Children =
                        {
                            new Label
                                {
                                    VerticalOptions = LayoutOptions.Center
                                }
                                .Text("Password : ")
                                .Column(Column.Description).Row(Row.TextEntry),
                            new Entry
                                {
                                    IsPassword = true
                                }
                                .Column(Column.Input)
                                .Row(Row.TextEntry)
                                .Bind(Entry.TextProperty, nameof(LoginPageViewModel.Password), BindingMode.TwoWay)
                        }
                    },
                    new HorizontalStackLayout
                    {
                        Margin = new Thickness(0, 0, 0, 8),
                        Children =
                        {
                            new Button
                            {
                                WidthRequest = 200, Text = "Login",
                                BackgroundColor = Color.FromRgba(0, 102, 204, 255)
                            }.Bind(Button.CommandProperty, nameof(LoginPageViewModel.LoginWithCredentialsCommand))
                        }
                    },
                    new HorizontalStackLayout
                    {
                        Children =
                        {
                            new Button
                                {
                                    WidthRequest = 200,
                                    Text = "Login in with browser"
                                }
                                .Bind(Button.CommandProperty, nameof(LoginPageViewModel.LoginWithBrowserCommand))
                        }
                    }
                }
            }
        };
    }

    protected override void OnAppearing()
    {
        Dispatcher.Dispatch(async () => { await NavigateToServerListIfTokenFound(); });
    }

    private async Task NavigateToServerListIfTokenFound()
    {
        string plexToken = await SecureStorage.Default.GetAsync("plex_token");

        if (string.IsNullOrEmpty(plexToken)) return;

        try
        {
            await Shell.Current.GoToAsync("servers");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}