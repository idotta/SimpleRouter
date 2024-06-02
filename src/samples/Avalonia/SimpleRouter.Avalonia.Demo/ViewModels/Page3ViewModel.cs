using CommunityToolkit.Mvvm.Input;
using System;

namespace SimpleRouter.Avalonia.Demo.ViewModels;

public partial class Page3ViewModel : ViewModelBase, IRoute
{
    public Page3ViewModel(IRouterHost routerHost, int number)
    {
        RouterHost = routerHost ?? throw new ArgumentNullException(nameof(routerHost));
        Number = number + 1;
    }

    public int Number { get; }

    public string RouteName => nameof(Page3ViewModel);

    public IRouterHost RouterHost { get; }

    [RelayCommand]
    private void NavigateToPage3Generic() => RouterHost.Router.NavigateTo<Page3ViewModel>(RouterHost, Number);

    [RelayCommand]
    private void NavigateToPage3Type() => RouterHost.Router.NavigateTo(typeof(Page3ViewModel), RouterHost, Number);
}