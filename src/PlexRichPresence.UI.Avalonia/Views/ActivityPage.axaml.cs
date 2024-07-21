using System;
using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PlexRichPresence.ViewModels;

namespace PlexRichPresence.UI.Avalonia.Views;

public partial class ActivityPage : UserControl
{
    public ActivityPage()
    {
        InitializeComponent();
        var plexActivityViewModel = this.CreateInstance<PlexActivityPageViewModel>();
        DataContext = plexActivityViewModel;
        Dispatcher.UIThread.Post(async () =>
        {
            await plexActivityViewModel.InitStrategyCommand.ExecuteAsync(null);
            await plexActivityViewModel.StartActivityCommand.ExecuteAsync(null);
        });

        plexActivityViewModel.PropertyChanged += (sender, args) =>
        {
            Dispatcher.UIThread.Post(async () =>
            {
                if (args.PropertyName is nameof(PlexActivityPageViewModel.ThumbnailUrl))
                {
                    using HttpClient client = new();
                    var thumbnailUrl = plexActivityViewModel.ThumbnailUrl;
                    var imageControl = this.FindControl<Image>("thumbnail");
                    if (thumbnailUrl != string.Empty)
                    {
                        var response = await client.GetAsync(new Uri(thumbnailUrl));
                        var imageStream = await response.Content.ReadAsStreamAsync();
                        
                        imageControl.Source = new Bitmap(imageStream);
                    }
                    else
                    {
                        imageControl.Source = null;
                    }
                }
            });
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}