using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using FluentAssertions;
using PlexRichPresence.UI.Avalonia.Services;
using Xunit;

namespace PlexRichPresence.UI.Avalonia.Tests.Services;

public class StorageServiceTests
{
    public StorageServiceTests()
    {
        BlobCache.Secure.InvalidateAll();
        BlobCache.Secure.Vacuum();
    }

    [Fact]
    public async Task Init_MigrateJsonFileDataToAkavache()
    {
        // Given
        var service = new StorageService(Directory.GetCurrentDirectory());
        
        // When
        await service.Init();
        
        // Then
        Assert.Equal("test", await BlobCache.Secure.GetObject<string>("plexUserName"));
        Assert.Equal("test", await BlobCache.Secure.GetObject<string>("plex_token"));
        Assert.Equal("test", await BlobCache.Secure.GetObject<string>("serverIp"));
        Assert.Equal("test", await BlobCache.Secure.GetObject<string>("serverPort"));
        Assert.Equal("True", await BlobCache.Secure.GetObject<string>("isServerOwned"));
        Assert.Equal("True", await BlobCache.Secure.GetObject<string>("enableIdleStatus"));
        Assert.False(File.Exists("storedData.json"));
    }
    
    [Fact]
    public async Task Put_CreatesFileIniAkavache()
    {
        // Given
        var service = new StorageService(Directory.GetCurrentDirectory());

        // When
        await service.PutAsync("test key", "test value");

        // Then
        Assert.Equal("test value", await BlobCache.Secure.GetObject<string>("test key"));
    }

    [Fact]
    public async Task Remove_DeletesFromAkavache()
    {
        // Given
        var service = new StorageService(Directory.GetCurrentDirectory());
        var key = "test key";
        await service.PutAsync(key, "test value");

        // When
        await service.RemoveAsync(key);

        // Then
        Assert.False(await service.ContainsKeyAsync(key));
    }

    [Fact]
    public async Task Get_ReadsValueFromJsonFile()
    {
        // Given
        var service = new StorageService(Directory.GetCurrentDirectory());
        await service.PutAsync("test key", "test value");

        // When
        var result = await service.GetAsync("test key");

        // Then
        result.Should().Be("test value");
    }

    [Fact]
    public async Task Put_NoFile_DontThrow()
    {
        // Given
        var service = new StorageService(Directory.GetCurrentDirectory());

        // When
        var action = async () => await service.PutAsync("test key", "test value");

        // Then
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ContainsKey_DataPresent_ReturnsTrue()
    {
        // Given
        var service = new StorageService(Directory.GetCurrentDirectory());
        await service.PutAsync("test key", "test value");

        // When
        var result = await service.ContainsKeyAsync("test key");

        // THen
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ContainsKey_DataAbsent_ReturnsFalse()
    {
        // Given
        var service = new StorageService(Directory.GetCurrentDirectory());

        // When
        var result = await service.ContainsKeyAsync("test key");

        // THen
        result.Should().BeFalse();
    }
}