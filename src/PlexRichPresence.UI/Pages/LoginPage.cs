using CommunityToolkit.Maui.Markup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Layouts;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace PlexRichPresence.UI.Pages
{
    internal class LoginPage : ContentPage
    {
        private enum Column { Description, Input }
        private enum Row { TextEntry }
        public LoginPage()
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
                            Margin = new Thickness(0,0,0,8),
                            Children =
                            {
                                new Label{ VerticalOptions = LayoutOptions.Center }.Text("Login : ").Column(Column.Description).Row(Row.TextEntry),
                                new Entry {
                                    
                                    WidthRequest = 200
                                }.Column(Column.Input).Row(Row.TextEntry)
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
                            Margin = new Thickness(0,0,0,8),
                            Children = {
                                new Label{VerticalOptions = LayoutOptions.Center}.Text("Password : ").Column(Column.Description).Row(Row.TextEntry),
                                new Entry
                                {
                                    
               
                                    IsPassword = true
                                }.Column(Column.Input).Row(Row.TextEntry)
                            }
                        },
                        new HorizontalStackLayout
                        {
                            Margin = new Thickness(0,0,0,8),
                            Children =
                            {
                                new Button{ 
                                    WidthRequest = 200, Text = "Login",
                                    BackgroundColor = Color.FromRgba(0,102,204,255)
                                }
                            }
                        },
                        new HorizontalStackLayout
                        {
                            Children =
                            {
                                new Button{ WidthRequest = 200, Text = "Login in with browser"}
                            }
                        }
                    }
                }
            };
        }
    }
}
