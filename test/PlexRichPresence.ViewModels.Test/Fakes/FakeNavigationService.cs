using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels.Test.Fakes;

public class FakeNavigationService : INavigationService
{
    public string CurrentPage { get; private set; } = string.Empty;

    public Task NavigateToAsync(string page)
    {
        CurrentPage = page;
        return Task.CompletedTask;
    }
}