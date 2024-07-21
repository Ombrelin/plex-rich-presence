using Plex.ServerApi.PlexModels.Media;
using Plex.ServerApi.PlexModels.Server.Sessions;
using PlexRichPresence.Core;

namespace PlexRichPresence.PlexActivity;

public class PlexSessionMapper
{
    public PlexSession Map(SessionMetadata metadata, string plexServerHost, string plexToken) =>
        new(
            metadata.Title,
            (uint)metadata.Index,
            metadata.ParentTitle,
            (uint)metadata.ParentIndex,
            metadata.GrandparentTitle,
            GetPlayerState(metadata.Player?.State ?? "buffering"),
            GetMediaType(metadata.Type),
            metadata.Duration,
            metadata.ViewOffset,
            metadata.Thumb is null ? null : $"{plexServerHost}{metadata.Thumb[1..]}?X-Plex-Token={plexToken}"
        );

    public PlexSession Map(Metadata metadata, string state, long viewOffset, string plexServerHost, string plexToken) =>
        new(
            metadata.Title,
            Convert.ToUInt32(metadata.Index),
            metadata.ParentTitle,
            Convert.ToUInt32(metadata.ParentIndex),
            metadata.GrandparentTitle,
            GetPlayerState(state),
            GetMediaType(metadata.Type),
            metadata.Duration,
            viewOffset,
            metadata.Thumb is null ? null : $"{plexServerHost}{metadata.Thumb[1..]}?X-Plex-Token={plexToken}"
        );


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