using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels.Test.Fakes;

public class FakeBrowserService : IBrowserService
{
    public List<string> OpenedUrls { get; } = new List<string>();

    public Task OpenAsync(string url)
    {
        OpenedUrls.Add(url);
        return Task.CompletedTask;
    }
}