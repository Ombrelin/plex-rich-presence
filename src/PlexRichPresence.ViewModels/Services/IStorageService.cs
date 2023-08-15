namespace PlexRichPresence.ViewModels.Services;

public interface IStorageService
{
    Task Init();
    Task PutAsync(string key, string value);
    Task<string> GetAsync(string key);
    Task<bool> ContainsKeyAsync(string key);
    Task RemoveAsync(string key);
}