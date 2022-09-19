using PlexRichPresence.PlexActivity;

namespace PlexRichPresence.ViewModels.Services;

public interface IPlexActivityService
{
    IPlexSessionStrategy GetStrategy(bool isOwner);
}