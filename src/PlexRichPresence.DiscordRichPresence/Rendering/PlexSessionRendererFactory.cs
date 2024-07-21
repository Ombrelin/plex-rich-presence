using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class PlexSessionRendererFactory
{
    private readonly IClock _clock;

    public PlexSessionRendererFactory(IClock clock)
    {
        _clock = clock;
    }

    public IPlexSessionRenderer BuildRendererForSession(PlexSession session) => session.MediaType switch
    {
        PlexMediaType.Movie => new MovieSessionRenderer(_clock),
        PlexMediaType.Episode => new SerieSessionRenderer(_clock),
        PlexMediaType.Track => new MusicSessionRenderer(_clock),
        PlexMediaType.Idle => new IdleSessionRenderer(),
        _ => new GenericSessionRenderer(_clock)
    };
}