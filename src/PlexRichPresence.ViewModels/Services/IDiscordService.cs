using PlexRichPresence.PlexActivity;
using PlexRichPresence.ViewModels.Models;

namespace PlexRichPresence.ViewModels.Services;

public interface IDiscordService
{
    void SetDiscordPresenceToPlexSession(IPlexSession session);
}