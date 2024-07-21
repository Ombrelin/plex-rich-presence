using System;
using FluentAssertions;
using Plex.ServerApi.PlexModels.Media;
using Plex.ServerApi.PlexModels.Server.Sessions;
using Xunit;

namespace PlexRichPresence.PlexActivity.Tests;

public class PlexSessionMapperTests
{
    [Fact]
    public void Map_ThumbWhenSessionMetaData_BuildsThumbUrl()
    {
        // Given
        var mapper = new PlexSessionMapper();
        var plexServerHost = new Uri("http://1.1.1.1:50").ToString();

        // When
        var session = mapper.Map(new SessionMetadata { Thumb = "/library/metadata/40712/thumb/169111949" }, plexServerHost, "token");

        // Then
        session.Thumbnail.Should().Be("http://1.1.1.1:50/library/metadata/40712/thumb/169111949?X-Plex-Token=token");
    }

    [Fact]
    public void Map_ThumbWhenMetaData_BuildsThumbUrl()
    {
        // Given
        var mapper = new PlexSessionMapper();
        var plexServerHost = new Uri("http://1.1.1.1:50").ToString();

        // When
        var session = mapper.Map(new Metadata() { Thumb = "/library/metadata/40712/thumb/169111949" }, "", 1, plexServerHost, "token");

        // Then
        session.Thumbnail.Should().Be("http://1.1.1.1:50/library/metadata/40712/thumb/169111949?X-Plex-Token=token");
    }
}