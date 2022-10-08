using System;
using System.IO;
using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PlexRichPresence.ViewModels;

namespace PlexRichPresence.UI.Avalonia.Views;

public partial class ServersPage : UserControl
{
    public ServersPage()
    {
        InitializeComponent();
        ServersPageViewModel serversPageViewModel = this.CreateInstance<ServersPageViewModel>();
        DataContext = serversPageViewModel;
        Dispatcher.UIThread.Post(async () =>
        {
            await serversPageViewModel.GetDataCommand.ExecuteAsync(null);
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(new Uri(serversPageViewModel.ThumbnailUrl));
            Stream imageStream = await response.Content.ReadAsStreamAsync();
            var imageControl = this.FindControl<Image>("profilePicture");
            imageControl.Source = new Bitmap(imageStream);
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}