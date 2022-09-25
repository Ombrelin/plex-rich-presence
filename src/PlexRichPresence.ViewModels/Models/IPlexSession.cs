namespace PlexRichPresence.ViewModels.Models;

public interface IPlexSession
{
    string MediaTitle { get; }
    uint MediaIndex{ get; }
    
    string MediaParentTitle { get; }
    uint MediaParentIndex { get; }
    string MediaGrandParentTitle{ get; }
    PlexPlayerState PlayerState { get; }
    PlexMediaType MediaType{ get; }
    long Duration { get; }
    long ViewOffset { get; }
}