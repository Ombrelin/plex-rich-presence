using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels.Test.Fakes;

public class FakeStorageService : IStorageService
{
    private readonly Dictionary<string, string> data = new();

    public FakeStorageService()
    {
    }

    public FakeStorageService(Dictionary<string, string> initialData)
    {
        data = initialData;
    }

    public Task Init() => Task.CompletedTask;

    public Task PutAsync(string key, string value)
    {
        data[key] = value;
        return Task.CompletedTask;
    }

    public Task<string> GetAsync(string key) => Task.FromResult(data[key]);

    public Task<bool> ContainsKeyAsync(string key)
    {
        return Task.FromResult(data.ContainsKey(key));
    }

    public Task RemoveAsync(string key)
    {
        data.Remove(key);
        return Task.CompletedTask;
    }
}