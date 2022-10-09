using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.UI.Avalonia.Services;

public class BrowserService : IBrowserService
{
    public Task OpenAsync(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }

        return Task.CompletedTask;
    }
}