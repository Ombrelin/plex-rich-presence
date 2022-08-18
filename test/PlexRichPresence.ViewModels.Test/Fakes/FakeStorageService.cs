﻿using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels.Test.Fakes;

public class FakeStorageService : IStorageService
{
    private readonly Dictionary<string, string> data = new ();
    
    public Task PutAsync(string key, string value)
    {
        data[key] = value;
        return Task.CompletedTask;
    }

    public Task<string> GetAsync(string key) => Task.FromResult(data[key]);
    public Task<bool> ContainsKeyAsync(string key)
    {
        throw new NotImplementedException();
    }
}