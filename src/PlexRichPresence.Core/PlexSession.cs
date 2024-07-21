namespace PlexRichPresence.Core;

public record PlexSession(
    string MediaTitle,
    uint MediaIndex,
    string MediaParentTitle,
    uint MediaParentIndex,
    string MediaGrandParentTitle,
    PlexPlayerState PlayerState,
    PlexMediaType MediaType,
    long Duration,
    long ViewOffset,
    string? Thumbnail
)
{
    public PlexSession() : this(
        "Idle",
        default,
        string.Empty,
        default,
        string.Empty,
        PlexPlayerState.Idle,
        PlexMediaType.Idle,
        default,
        default,
        string.Empty
    )
    {
    }

}