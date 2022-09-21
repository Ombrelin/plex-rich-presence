using PlexRichPresence.PlexActivity;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.ViewModels.Test.Fakes;

public class FakePlexActivityService : IPlexActivityService
{
    public bool IsOwner { get; private set; }
    public FakePlexSessionStrategy Strategy { get; } = new FakePlexSessionStrategy();

    public IPlexSessionStrategy GetStrategy(bool isOwner)
    {
        IsOwner = isOwner;
        return Strategy;
    }
}