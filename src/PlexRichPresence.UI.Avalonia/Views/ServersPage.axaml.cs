using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PlexRichPresence.UI.Avalonia.Views;

public partial class ServersPage : UserControl
{
    public ServersPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}