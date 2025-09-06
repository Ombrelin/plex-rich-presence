using DiscordRPC;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class GenericSessionRenderer(IClock clock) : IPlexSessionRenderer
{
    public virtual RichPresence RenderSession(PlexSession session)
    {
        var presence = new RichPresence
        {
            Type = ActivityType.Watching,
            StatusDisplay = StatusDisplayType.Name,
            Details = session.MediaGrandParentTitle + " - " + session.MediaParentTitle,
            State = session.MediaTitle,
            Assets = new Assets()
        };

        RenderPlayerState(session, presence);

        return presence;
    }

    protected void RenderPlayerState(PlexSession session, RichPresence presence)
    {
        switch (session.PlayerState)
        {
            case PlexPlayerState.Buffering:
                presence.Assets.SmallImageKey = "sand-clock";
                break;
            case PlexPlayerState.Paused:
                presence.Assets.SmallImageKey = "pause-circle";
                break;

            case PlexPlayerState.Playing:
                presence.Timestamps = new Timestamps
                {
                    Start = clock.Now.AddSeconds(ComputeSessionStartTime(session) * -1).ToUniversalTime(),
                    End = clock.Now.AddSeconds(ComputeSessionRemainingTime(session)).ToUniversalTime()
                };
                break;

            case PlexPlayerState.Idle:
                presence.Assets.SmallImageKey = "sleep-mode";
                break;

            default:
                break;
        }
    }

    private static long ComputeSessionRemainingTime(PlexSession session) =>
        (session.Duration - session.ViewOffset) / 1000;

    private static long ComputeSessionStartTime(PlexSession session) =>
        session.ViewOffset / 1000;
}