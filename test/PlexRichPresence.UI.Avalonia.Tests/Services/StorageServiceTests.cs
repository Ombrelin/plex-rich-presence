using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using PlexRichPresence.UI.Avalonia.Services;
using Xunit;

namespace PlexRichPresence.UI.Avalonia.Tests.Services;

public class StorageServiceTests
{
    public StorageServiceTests()
    {
        File.Delete("storedData.json");
    }

    [Fact]
    public async Task Put_CreatesFileWithDataAsJson()
    {
        // Given
        var service = new StorageService(Directory.GetCurrentDirectory());

        // When
        await service.PutAsync("test key", "test value");

        // Then
        File.Exists("storedData.json").Should().BeTrue();
        JObject data = JObject.Parse(await File.ReadAllTextAsync("storedData.json"));
        data["test key"].Should().BeEmpty("test value");
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
        bool result = await service.ContainsKeyAsync("test key");

        // THen
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ContainsKey_DataAbsent_ReturnsFalse()
    {
        // Given
        var service = new StorageService(Directory.GetCurrentDirectory());

        // When
        bool result = await service.ContainsKeyAsync("test key");

        // THen
        result.Should().BeFalse();
    }
}