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
    private const string STORED_DATA_JSON = "storedData.json";
    private readonly string storedDataFolder;
    private readonly string storedDataPath;
    private bool directoryCreated = false;

    public StorageService(string storedDataFolder)
    {
        Registrations.Start("PlexRichPresence");
        this.storedDataFolder = storedDataFolder;
        storedDataPath = this.storedDataFolder + $"/{STORED_DATA_JSON}";
    }

    public async Task Init()
    {
        if (!File.Exists(storedDataPath))
        {
            return;
        }

        var storedData = await ReadStoredData();

        await Task.WhenAll(
            storedData
                .Select(kvp => BlobCache.Secure.InsertObject(kvp.Key, kvp.Value).ToTask())
                .ToArray()
        );
        
        File.Delete(storedDataPath);
    }

    public async Task PutAsync(string key, string value)
    {
        await BlobCache.Secure.InsertObject(key, value);
    }

    private async Task WriteDataToFile(Dictionary<string, string> storedData)
    {
        await File.WriteAllTextAsync(
            path: storedDataPath,
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
            return await File.ReadAllTextAsync(storedDataPath);
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