using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PlexRichPresence.ViewModels;

namespace PlexRichPresence.UI.Avalonia.Views;

public partial class ActivityPage : UserControl
{
    public ActivityPage()
    {
        InitializeComponent();
        var plexActivityViewModel = this.CreateInstance<PlexActivityPageViewModel>();
        this.DataContext = plexActivityViewModel;
        Dispatcher.UIThread.Post(async () =>
        {
            await plexActivityViewModel.InitStrategyCommand.ExecuteAsync(null);
            await plexActivityViewModel.StartActivityCommand.ExecuteAsync(null);
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}