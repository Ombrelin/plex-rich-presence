using DiscordRPC;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Models;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class GenericSessionRenderer : IPlexSessionRenderer
{
    private readonly IClock clock;

    public GenericSessionRenderer(IClock clock)
    {
        this.clock = clock;
    }

    public virtual RichPresence RenderSession(PlexSession session)
    {
        (_, DateTime startTimeStamp, DateTime endTimeStamps) = RenderPlayerState(session);
        return new RichPresence
        {
            Type = ActivityType.Watching,
            StatusDisplay = StatusDisplayType.Name,
            Details = session.MediaGrandParentTitle + " - " + session.MediaParentTitle,
            State = session.MediaTitle,
            Timestamps = new Timestamps
            {
                Start = startTimeStamp,
                End = endTimeStamps
            }
        };
    }

    protected (string playerState, DateTime startTimeStamp, DateTime endTimeStamp) RenderPlayerState(PlexSession session)
    {
        return session.PlayerState switch
        {
            PlexPlayerState.Buffering => ("⟲", clock.Now.AddSeconds(ComputeSessionStartTime(session) * -1).ToUniversalTime(), clock.Now.AddSeconds(ComputeSessionRemainingTime(session)).ToUniversalTime()),
            PlexPlayerState.Paused => ("⏸", clock.Now.AddSeconds(ComputeSessionStartTime(session) * -1).ToUniversalTime(), clock.Now.AddSeconds(ComputeSessionRemainingTime(session)).ToUniversalTime()),
            PlexPlayerState.Playing => ("▶",
                clock.Now.AddSeconds(ComputeSessionStartTime(session) * -1).ToUniversalTime(), clock.Now.AddSeconds(ComputeSessionRemainingTime(session)).ToUniversalTime()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static long ComputeSessionRemainingTime(PlexSession session) =>
        (session.Duration - session.ViewOffset) / 1000;

    private static long ComputeSessionStartTime(PlexSession session) =>
        session.ViewOffset / 1000;
}