using PlexRichPresence.ViewModels.Models;

namespace PlexRichPresence.DiscordRichPresence.Tests;

public class FakePlexSession : IPlexSession
{
    public string MediaTitle { get; set; }
    public uint MediaIndex { get; set; }
    public string MediaParentTitle { get; set; }
    public uint MediaParentIndex { get; set; }
    public string MediaGrandParentTitle { get; set; }
    public PlexPlayerState PlayerState { get; set; }
    public PlexMediaType MediaType { get; set; }
    public long Duration { get; set; }
    public long ViewOffset { get; set; }
}