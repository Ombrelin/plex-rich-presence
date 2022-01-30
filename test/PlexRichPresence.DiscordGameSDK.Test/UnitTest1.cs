using Xunit;

namespace PlexRichPresence.DiscordGameSDK.Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var sdk  = new DiscordGameSdk();
        sdk.UpdateActivity();
    }
}