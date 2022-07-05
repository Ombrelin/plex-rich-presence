using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.UI.Service;

public class BrowserService : IBrowserService
{
    public Task OpenAsync(string url) => Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
}