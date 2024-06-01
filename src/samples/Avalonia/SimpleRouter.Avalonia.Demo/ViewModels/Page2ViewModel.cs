using CommunityToolkit.Mvvm.Input;
using System;

namespace SimpleRouter.Avalonia.Demo.ViewModels;

public partial class Page2ViewModel : ViewModelBase, IRoute
{
    public Page2ViewModel(IRouterHost routerHost)
    {
        RouterHost = routerHost ?? throw new ArgumentNullException(nameof(routerHost));
    }

    public string RouteName => nameof(Page2ViewModel);

    public IRouterHost RouterHost { get; }

    [RelayCommand]
    private void NavigateToPage1() => RouterHost.Router.NavigateTo(typeof(Page1ViewModel), RouterHost);

    [RelayCommand]
    private void NavigateToRogue() => RouterHost.Router.NavigateTo(new RogueViewModel(RouterHost, 0));
}