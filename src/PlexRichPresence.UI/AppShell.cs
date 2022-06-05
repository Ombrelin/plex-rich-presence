using PlexRichPresence.UI.Pages;

namespace PlexRichPresence.UI;

internal class AppShell : Shell
{
    public AppShell(LoginPage page)
    {

        Items.Add(page);

        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("servers", typeof(ServersPage));
    }
}