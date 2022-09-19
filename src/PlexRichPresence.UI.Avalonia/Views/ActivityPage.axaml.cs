using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PlexRichPresence.ViewModels;

namespace PlexRichPresence.UI.Avalonia.Views;

public partial class ActivityPage : UserControl
{
    public ActivityPage()
    {
        InitializeComponent();
        var plexActivityViewModel = this.CreateInstance<PlexActivityPageViewModel>();
        this.DataContext = plexActivityViewModel;
        Task.Run(() => plexActivityViewModel.GetDataFromStorageCommand.ExecuteAsync(null));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}