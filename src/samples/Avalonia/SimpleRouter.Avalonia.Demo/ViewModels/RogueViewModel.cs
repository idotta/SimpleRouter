using CommunityToolkit.Mvvm.Input;
using System;

namespace SimpleRouter.Avalonia.Demo.ViewModels;

public partial class RogueViewModel : ViewModelBase, IRoute
{
    public RogueViewModel(IRouterHost routerHost, int number)
    {
        RouterHost = routerHost ?? throw new ArgumentNullException(nameof(routerHost));
        Number = number + 1;
    }

    public int Number { get; }

    public string RouteName => nameof(RogueViewModel);

    public IRouterHost RouterHost { get; }

    [RelayCommand]
    private void NavigateToRogue1() => RouterHost.Router.NavigateTo<RogueViewModel>(RouterHost, Number);

    [RelayCommand]
    private void NavigateToRogue2() => RouterHost.Router.NavigateTo(typeof(RogueViewModel), RouterHost, Number);
}
