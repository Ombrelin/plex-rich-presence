using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.UI.Service;

public class StorageService : IStorageService
{
    public Task PutAsync(string key, string value)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetAsync(string key)
    {
        throw new NotImplementedException();
    }
}