using DiscordRPC;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class MusicSessionRenderer(IClock clock) : GenericSessionRenderer(clock)
{
    public override RichPresence RenderSession(PlexSession session)
    {
        var presence = new RichPresence
        {
            Type = ActivityType.Listening,
            StatusDisplay = StatusDisplayType.State,
            Details = $"{session.MediaTitle}",
            State = $"{session.MediaGrandParentTitle}",
            Assets = new Assets()
            {
                LargeImageKey = session.Thumbnail
            }
        };

        RenderPlayerState(session, presence);

        return presence;
    }
}