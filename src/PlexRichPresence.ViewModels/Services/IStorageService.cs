namespace PlexRichPresence.ViewModels.Services;

public interface IStorageService
{
    Task PutAsync(string key, string value);
    Task<string> GetAsync(string key);
}