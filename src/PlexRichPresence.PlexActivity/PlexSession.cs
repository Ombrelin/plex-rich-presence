using Plex.ServerApi.PlexModels.Media;
using Plex.ServerApi.PlexModels.Server.Sessions;
using PlexRichPresence.ViewModels.Models;

namespace PlexRichPresence.PlexActivity;

public class PlexSession : IPlexSession
{
    public string MediaTitle { get; }
    public uint MediaIndex { get; }

    public string MediaParentTitle { get; }
    public uint MediaParentIndex { get; }
    public string MediaGrandParentTitle { get; }
    public PlexPlayerState PlayerState { get; }
    public PlexMediaType MediaType { get; }
    public long Duration { get; }
    public long ViewOffset { get; }
    
    
    
    public PlexSession(SessionMetadata metadata)
    {
        MediaTitle = metadata.Title;
        MediaIndex = metadata.Index is null ? 0u : uint.Parse(metadata.Index);
        MediaParentTitle = metadata.ParentTitle;
        MediaParentIndex = metadata.Index is null ? 0u : uint.Parse(metadata.ParentIndex);
        MediaGrandParentTitle = metadata.GrandparentTitle;
        PlayerState = GetPlayerState(metadata.Player?.State);
        MediaType = GetMediaType(metadata.Type);
        Duration = metadata.Index is null ? 0L : long.Parse(metadata.Duration);
        ViewOffset = metadata.Index is null ? 0L : long.Parse(metadata.ViewOffset);
    }

    public PlexSession(Metadata metadata, string state, long viewOffset)
    {
        MediaTitle = metadata.Title;
        MediaIndex = Convert.ToUInt32(metadata.Index);
        MediaParentTitle = metadata.ParentTitle;
        MediaParentIndex = Convert.ToUInt32(metadata.ParentIndex);
        MediaGrandParentTitle = metadata.GrandparentTitle;
        PlayerState = GetPlayerState(state);
        MediaType = GetMediaType(metadata.Type);
        Duration = metadata.Duration;
        ViewOffset = viewOffset;
    }

    private static PlexMediaType GetMediaType(string type) => type switch
    {
        "movie" => PlexMediaType.Movie,
        "episode" => PlexMediaType.Episode,
        "track" => PlexMediaType.Track,
        _ => PlexMediaType.Unknown
    };

    private static PlexPlayerState GetPlayerState(string state) => state switch
    {
        "buffering" => PlexPlayerState.Buffering,
        "paused" => PlexPlayerState.Paused,
        _ => PlexPlayerState.Playing
    };
}