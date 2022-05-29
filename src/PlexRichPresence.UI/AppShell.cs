using PlexRichPresence.UI.Pages;

namespace PlexRichPresence.UI;

class AppShell : Shell
{
    public AppShell(LoginPage page)
    {
        Items.Add(page);
    }
}