using DiscordRPC;
using PlexRichPresence.Core;

namespace PlexRichPresence.DiscordRichPresence;

public interface IPlexSessionRenderer
{
    RichPresence RenderSession(PlexSession session);
}