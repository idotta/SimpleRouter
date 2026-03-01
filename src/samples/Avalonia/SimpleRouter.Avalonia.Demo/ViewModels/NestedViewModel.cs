using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace SimpleRouter.Avalonia.Demo.ViewModels;

[Route(Group = "Main")]
[GeneratedRouteFactory(Group = "Nested")]
public partial class NestedViewModel : ViewModelBase, IRoute, IRouterHost
{
    public NestedViewModel(IRouterHost routerHost)
    {
        RouterHost = routerHost ?? throw new ArgumentNullException(nameof(routerHost));
        Router = new Router(CreateGeneratedRouteFactory());
        Router.OnRouteChanged += (sender, e) =>
        {
            CurrentRoute = e.Next;
            StackSize = Router.Stack.Count;
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
