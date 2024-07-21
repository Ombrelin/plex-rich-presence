using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels.Test.Fakes;

public class FakeStorageService : IStorageService
{
    private readonly Dictionary<string, string> _data = new();

    public FakeStorageService() { }
    public FakeStorageService(Dictionary<string, string> initialData) => _data = initialData;

    public Task Init() => Task.CompletedTask;

    public Task PutAsync(string key, string value)
    {
        _data[key] = value;
        return Task.CompletedTask;
    }

    public Task<string> GetAsync(string key) => Task.FromResult(_data[key]);
    public Task<bool> ContainsKeyAsync(string key) => Task.FromResult(_data.ContainsKey(key));

    public Task RemoveAsync(string key)
    {
        _data.Remove(key);
        return Task.CompletedTask;
    }
}