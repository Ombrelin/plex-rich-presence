using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace PlexRichPresence.UI.Avalonia;

public static class Extensions
{
    public static IServiceProvider GetServiceProvider(this IResourceHost control)
    {
        return (IServiceProvider) App.Current.FindResource(typeof(IServiceProvider));
    }

    public static T CreateInstance<T>(this IResourceHost control)
    {
        return ActivatorUtilities.CreateInstance<T>(control.GetServiceProvider());
    }
}