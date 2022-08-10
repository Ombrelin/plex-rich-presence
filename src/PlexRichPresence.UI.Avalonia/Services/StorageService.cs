using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.UI.Avalonia.Services;

public class StorageService : IStorageService
{
    private readonly Dictionary<string, string> data = new ();
    
    public Task PutAsync(string key, string value)
    {
        data[key] = value;
        return Task.CompletedTask;
    }

    public Task<string> GetAsync(string key) => Task.FromResult(data[key]);
}