using DiscordRPC;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class GenericSessionRenderer(IClock clock) : IPlexSessionRenderer
{
    public virtual RichPresence RenderSession(PlexSession session)
    {
        DiscordPlayerState playerState = RenderPlayerState(session);
        
        return new RichPresence
        {
            Type = ActivityType.Watching,
            StatusDisplay = StatusDisplayType.Name,
            Details = session.MediaGrandParentTitle + " - " + session.MediaParentTitle,
            State = session.MediaTitle,
            Assets = new Assets
            {
                SmallImageKey = playerState.SmallAssetImageKey
            },
            Timestamps = playerState.Timestamps
        };
    }

    protected DiscordPlayerState RenderPlayerState(PlexSession session)
    {
        return session.PlayerState switch
        {
            PlexPlayerState.Buffering => new DiscordPlayerState("sand-clock"),
            PlexPlayerState.Paused => new DiscordPlayerState("pause-circle"),
            PlexPlayerState.Playing => new DiscordPlayerState(null, new Timestamps
                {
                    Start = clock.Now.AddSeconds(ComputeSessionStartTime(session) * -1).ToUniversalTime(),
                    End = clock.Now.AddSeconds(ComputeSessionRemainingTime(session)).ToUniversalTime()
                }),
            PlexPlayerState.Idle => new DiscordPlayerState("sleep-mode"),
            _ => new DiscordPlayerState()
        };
    }

    private static long ComputeSessionRemainingTime(PlexSession session) =>
        (session.Duration - session.ViewOffset) / 1000;

    private static long ComputeSessionStartTime(PlexSession session) =>
        session.ViewOffset / 1000;
}