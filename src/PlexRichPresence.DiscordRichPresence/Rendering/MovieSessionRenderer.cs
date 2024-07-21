using DiscordRPC;
using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class MovieSessionRenderer : GenericSessionRenderer
{
    public override RichPresence RenderSession(PlexSession session)
    {
        (var playerState, var endTimeStamp) = RenderPlayerState(session);
        return new RichPresence
        {
            Details = session.MediaTitle,
            State = playerState.Length < 2 ? playerState + '\x2800' : playerState,
            Timestamps = new Timestamps
            {
                End = endTimeStamp
            }
        };
    }

    public MovieSessionRenderer(IClock clock) : base(clock)
    {
    }
}