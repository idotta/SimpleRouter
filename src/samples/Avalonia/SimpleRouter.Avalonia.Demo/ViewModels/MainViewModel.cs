using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace SimpleRouter.Avalonia.Demo.ViewModels;

public partial class MainViewModel : ViewModelBase, IRouterHost
{
    public MainViewModel()
    {
        Router = new Router(new RouteFactory(CreateRoutes));
        Router.OnRouteChanged += (sender, e) =>
        {
            CurrentRoute = e.Next;
            StackSize = Router.Stack.Count;
        };
        ResetToPage1();
    }

    public string Greeting { get; } = "This is main view model!";

    public IRouter Router { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateBackCommand))]
    private IRoute? _currentRoute;

    [ObservableProperty]
    private int _stackSize;

    private static IRoute? CreateRoutes(Type routeType, object[] parameters)
    {
        return routeType.Name switch
        {
            nameof(Page1ViewModel) => new Page1ViewModel((MainViewModel)parameters[0]),
            nameof(Page2ViewModel) => new Page2ViewModel((MainViewModel)parameters[0]),
            nameof(RogueViewModel) => new RogueViewModel((MainViewModel)parameters[0], (int)parameters[1]),
            _ => null,
        };
    }

    [RelayCommand]
    private void ResetToPage1() => Router.NavigateToAndReset(new Page1ViewModel(this));

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void NavigateBack()
    {
        Router.NavigateBack();
    }

    private bool CanNavigateBack() => Router.Stack.Count > 1;
}