using DiscordRPC;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class GenericSessionRenderer : IPlexSessionRenderer
{
    private readonly IClock _clock;

    public GenericSessionRenderer(IClock clock)
    {
        _clock = clock;
    }

    public virtual RichPresence RenderSession(PlexSession session)
    {
        (_, var endTimeStamps) = RenderPlayerState(session);
        return new RichPresence
        {
            Details = session.MediaGrandParentTitle + " - " + session.MediaParentTitle,
            State = session.MediaTitle,
            Timestamps = new Timestamps
            {
                End = endTimeStamps
            }
        };
    }

    protected (string playerState, DateTime endTimeStamp) RenderPlayerState(PlexSession session)
    {
        return session.PlayerState switch
        {
            PlexPlayerState.Buffering => ("⟲", _clock.Now.ToUniversalTime()),
            PlexPlayerState.Paused => ("⏸", _clock.Now.ToUniversalTime()),
            PlexPlayerState.Playing => ("▶",
                _clock.Now.AddSeconds(ComputeSessionRemainingTime(session)).ToUniversalTime()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static long ComputeSessionRemainingTime(PlexSession session) =>
        (session.Duration - session.ViewOffset) / 1000;
}