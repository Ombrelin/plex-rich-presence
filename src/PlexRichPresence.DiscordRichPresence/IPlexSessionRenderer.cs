using DiscordRPC;
using PlexRichPresence.PlexActivity;
using PlexRichPresence.ViewModels.Models;

namespace PlexRichPresence.DiscordRichPresence;

public interface IPlexSessionRenderer
{
    RichPresence RenderSession(IPlexSession session);
}