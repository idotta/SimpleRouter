using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace SimpleRouter.Avalonia.Demo.ViewModels;

public partial class NestedViewModel : ViewModelBase, IRoute, IRouterHost
{
    public NestedViewModel(IRouterHost routerHost)
    {
        RouterHost = routerHost ?? throw new ArgumentNullException(nameof(routerHost));
        Router = new Router(new RouteFactory(CreateRoutes));
        Router.OnRouteChanged += (sender, e) =>
        {
            CurrentRoute = e.Next;
            StackSize = Router.Stack.Count;
        };
    }

    private IRoute? CreateRoutes(Type routeType, object[] parameters)
    {
        return routeType.Name switch
        {
            nameof(Page3ViewModel) => new Page3ViewModel((NestedViewModel)parameters[0], (int)parameters[1]),
            _ => null,
        };
    }

    public string RouteName => nameof(NestedViewModel);

    public IRouterHost RouterHost { get; }

    public IRouter Router { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateBackCommand))]
    private IRoute? _currentRoute;

    [ObservableProperty]
    private int _stackSize;

    [RelayCommand]
    private void ResetToPage3() => Router.NavigateToAndReset(new Page3ViewModel(this, 0));

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void NavigateBack()
    {
        Router.NavigateBack();
    }

    private bool CanNavigateBack() => Router.Stack.Count > 1;
}