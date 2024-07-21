using System;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace PlexRichPresence.UI.Avalonia;

public static class Extensions
{
    public static IServiceProvider GetServiceProvider(this IResourceHost control)
    {
        return (IServiceProvider)Application.Current.FindResource(typeof(IServiceProvider));
    }

    public static T CreateInstance<T>(this IResourceHost control)
    {
        return control.GetServiceProvider().GetService<T>() ??
               throw new InvalidOperationException($"Viewmodel of type {typeof(T).Name} not in DI");
    }
}