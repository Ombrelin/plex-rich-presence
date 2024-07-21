using DiscordRPC;
using Microsoft.Extensions.Logging;
using PlexRichPresence.Core;
using PlexRichPresence.DiscordRichPresence.Rendering;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.DiscordRichPresence;

public class DiscordService : IDiscordService
{
    private readonly ILogger<DiscordService> _logger;
    private DiscordRpcClient? _discordRpcClient;
    private readonly PlexSessionRenderingService _plexSessionRenderingService;
    private PlexSession? _currentSession;

    public DiscordService(ILogger<DiscordService> logger, PlexSessionRenderingService plexSessionRenderingService)
    {
        _logger = logger;
        _plexSessionRenderingService = plexSessionRenderingService;
        _discordRpcClient = CreateRpcClient();
    }

    private DiscordRpcClient CreateRpcClient()
    {
        var rpcClient = new DiscordRpcClient(applicationID: "698954724019273770");
        rpcClient.OnError += (_, args) => _logger.LogError(args.Message);
        rpcClient.Initialize();

        return rpcClient;
    }

    public void SetDiscordPresenceToPlexSession(PlexSession session)
    {
        if (session == _currentSession)
        {
            return;
        }

        _currentSession = session;
        var richPresence = _plexSessionRenderingService.RenderSession(session);
        richPresence.Assets = new Assets
        {
            LargeImageKey = "icon"
        };
        _discordRpcClient ??= CreateRpcClient();
        _discordRpcClient.SetPresence(richPresence);
    }

    public void StopRichPresence()
    {
        _discordRpcClient?.Deinitialize();
        _discordRpcClient = null;
        _currentSession = null;
    }
}