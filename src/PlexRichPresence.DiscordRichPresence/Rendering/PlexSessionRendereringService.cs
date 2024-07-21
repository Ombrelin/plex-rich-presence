using DiscordRPC;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlexRichPresence.Core;

namespace PlexRichPresence.DiscordRichPresence.Rendering;

public class PlexSessionRenderingService
{
    private readonly PlexSessionRendererFactory _rendererFactory;
    private readonly ILogger<PlexSessionRenderingService> _logger;

    public PlexSessionRenderingService(PlexSessionRendererFactory rendererFactory,
        ILogger<PlexSessionRenderingService> logger)
    {
        _rendererFactory = rendererFactory;
        _logger = logger;
    }

    public RichPresence RenderSession(PlexSession session)
    {
        var renderedSession = _rendererFactory
            .BuildRendererForSession(session)
            .RenderSession(session);

        _logger.LogInformation("Rendered Plex Session : {Session}", JsonConvert.SerializeObject(renderedSession));

        return renderedSession;
    }
}