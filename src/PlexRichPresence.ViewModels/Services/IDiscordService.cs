using PlexRichPresence.Core;

namespace PlexRichPresence.ViewModels.Services;

public interface IDiscordService
{
    void SetDiscordPresenceToPlexSession(PlexSession session);
    void StopRichPresence();
}