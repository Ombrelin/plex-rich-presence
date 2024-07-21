using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Akavache;
using Newtonsoft.Json;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.UI.Avalonia.Services;

public class StorageService : IStorageService
{
    private const string StoredDataJson = "storedData.json";
    private readonly string _storedDataFolder;
    private readonly string _storedDataPath;
    private bool _directoryCreated = false;

    public StorageService(string storedDataFolder)
    {
        Registrations.Start("PlexRichPresence");
        _storedDataFolder = storedDataFolder;
        _storedDataPath = _storedDataFolder + $"/{StoredDataJson}";
    }

    public async Task Init()
    {
        if (!File.Exists(_storedDataPath))
        {
            return;
        }

        var storedData = await ReadStoredData();

        await Task.WhenAll(
            storedData
                .Select(kvp => BlobCache.Secure.InsertObject(kvp.Key, kvp.Value).ToTask())
                .ToArray()
        );
        
        File.Delete(_storedDataPath);
    }

    public async Task PutAsync(string key, string value)
    {
        await BlobCache.Secure.InsertObject(key, value);
    }

    private async Task WriteDataToFile(Dictionary<string, string> storedData)
    {
        await File.WriteAllTextAsync(
            path: _storedDataPath,
            contents: JsonConvert.SerializeObject(storedData)
        );
    }

    private async Task<Dictionary<string, string>> ReadStoredData()
    {
        var storageFileContent = await ReadStorageFileContent();
        return DeserialiseStoredDataDictionary(storageFileContent);
    }

    private Dictionary<string, string> DeserialiseStoredDataDictionary(string storageFileContent)
    {
        return storageFileContent == string.Empty
            ? new Dictionary<string, string>()
            : JsonConvert.DeserializeObject<Dictionary<string, string>>(storageFileContent)
              ?? throw new InvalidOperationException("Error deserialising stored data");
    }

    private async Task<string> ReadStorageFileContent()
    {
        try
        {
            return await File.ReadAllTextAsync(_storedDataPath);
        }
        catch (FileNotFoundException)
        {
            return "";
        }
    }

    public async Task<string> GetAsync(string key)
    {
        return (await BlobCache.Secure.GetObject<string>(key))!;
    }

    public async Task<bool> ContainsKeyAsync(string key)
    {
        try
        {
            await BlobCache.Secure.GetObject<string>(key);
            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    public Task RemoveAsync(string key)
    {
        BlobCache.Secure.Invalidate(key);
        BlobCache.Secure.Vacuum();

        return Task.CompletedTask;
    }
}