using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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
        this.storedDataFolder = storedDataFolder;
        storedDataPath = this.storedDataFolder + $"/{STORED_DATA_JSON}";
    }

    private void EnsureDirectoryExists()
    {
        if (directoryCreated && Directory.Exists(storedDataFolder)) return;

        Directory.CreateDirectory(storedDataFolder);
        this.directoryCreated = true;
    }

    public async Task PutAsync(string key, string value)
    {
        EnsureDirectoryExists();
        Dictionary<string, string> storedData = await ReadStoredData();

        storedData[key] = value;

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
        catch (FileNotFoundException e)
        {
            return "";
        }
    }

    public async Task<string> GetAsync(string key)
    {
        EnsureDirectoryExists();
        Dictionary<string, string> storedData = await ReadStoredData();
        return storedData[key];
    }

    public async Task<bool> ContainsKeyAsync(string key)
    {
        EnsureDirectoryExists();
        Dictionary<string, string> storedData = await ReadStoredData();
        return storedData.ContainsKey(key);
    }
}