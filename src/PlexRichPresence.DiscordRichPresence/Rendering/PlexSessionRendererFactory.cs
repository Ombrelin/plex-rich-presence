using PlexRichPresence.Core;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class PlexSessionRendererFactory
{
    private readonly IClock clock;

    public PlexSessionRendererFactory(IClock clock)
    {
        this.clock = clock;
    }

    public IPlexSessionRenderer BuildRendererForSession(PlexSession session) => session.MediaType switch
    {
        PlexMediaType.Movie => new MovieSessionRenderer(this.clock),
        PlexMediaType.Episode => new SerieSessionRenderer(this.clock),
        PlexMediaType.Track => new MusicSessionRenderer(this.clock),
        PlexMediaType.Idle => new IdleSessionRenderer(),
        _ => new GenericSessionRenderer(this.clock)
    };
}