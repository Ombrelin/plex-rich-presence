namespace PlexRichPresence.ViewModels.Services;

public interface IBrowserService
{
    Task OpenAsync(string url);
}