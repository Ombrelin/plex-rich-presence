using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using PlexRichPresence.ViewModels.Services;

namespace PlexRichPresence.UI.Avalonia.Services;

public class NavigationService : INavigationService
{
    private readonly IDictionary<string, Type> _registeredPage = new Dictionary<string, Type>();
    private readonly Frame _navigationFrame;

    public NavigationService(Frame navigationFrame)
    {
        _navigationFrame = navigationFrame;
    }

    public void RegisterPage(string pageName, Type pageType)
    {
        _registeredPage[pageName] = pageType;
    }

    public Task NavigateToAsync(string page)
    {
        _navigationFrame.Navigate(_registeredPage[page]);
        return Task.CompletedTask;
    }
}