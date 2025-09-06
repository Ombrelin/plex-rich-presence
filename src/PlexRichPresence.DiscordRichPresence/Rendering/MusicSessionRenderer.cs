using DiscordRPC;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class MusicSessionRenderer(IClock clock) : GenericSessionRenderer(clock)
{
    public override RichPresence RenderSession(PlexSession session)
    {
        DiscordPlayerState playerState = RenderPlayerState(session);
        return new RichPresence
        {
            Type = ActivityType.Listening,
            StatusDisplay = StatusDisplayType.State,
            Details = $"{session.MediaTitle}",
            State = $"{session.MediaGrandParentTitle}",
            Assets = new Assets
            {
                SmallImageKey = playerState.SmallAssetImageKey,
                LargeImageKey = session.Thumbnail
            },
            Timestamps = playerState.Timestamps
        };
    }
}