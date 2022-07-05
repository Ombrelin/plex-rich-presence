using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.UI.Service;

public class NavigationService : INavigationService
{
    public Task NavigateToAsync(string page) => Shell.Current.GoToAsync(page);
IBrowserService