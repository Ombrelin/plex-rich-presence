using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PlexRichPresence.ViewModels;

namespace PlexRichPresence.UI.Avalonia.Views;

public partial class LoginPage : UserControl
{
    public LoginPage()
    {
        InitializeComponent();
        DataContext = this.CreateInstance<LoginPageViewModel>();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}