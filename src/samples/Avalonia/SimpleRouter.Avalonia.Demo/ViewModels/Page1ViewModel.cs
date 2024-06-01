using CommunityToolkit.Mvvm.Input;
using System;

namespace SimpleRouter.Avalonia.Demo.ViewModels;

public partial class Page1ViewModel : ViewModelBase, IRoute
{
    public Page1ViewModel(IRouterHost routerHost)
    {
        RouterHost = routerHost ?? throw new ArgumentNullException(nameof(routerHost));
    }

    public string RouteName => nameof(Page1ViewModel);

    public IRouterHost RouterHost { get; }

    [RelayCommand]
    private void NavigateToPage2() => RouterHost.Router.NavigateTo(new Page2ViewModel(RouterHost));

    [RelayCommand]
    private void ResetToPage2() => RouterHost.Router.NavigateToAndReset<Page2ViewModel>(RouterHost);
}