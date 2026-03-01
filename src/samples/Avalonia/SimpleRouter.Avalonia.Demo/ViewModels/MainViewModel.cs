using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SimpleRouter.Avalonia.Demo.ViewModels;

[GeneratedRouteFactory(Group = "Main")]
public partial class MainViewModel : ViewModelBase, IRouterHost
{
    public MainViewModel()
    {
        Router = new Router(CreateGeneratedRouteFactory());
        Router.OnRouteChanged += (sender, e) =>
        {
            CurrentRoute = e.Next;
            StackSize = Router.Stack.Count;
        };
        // Comment the following line to start with the default content
        ResetToPage1();
    }

    public IRouter Router { get; }
    public IRouter MainRouter => Router;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateBackCommand))]
    private IRoute? _currentRoute;

    [ObservableProperty]
    private int _stackSize;

    [RelayCommand]
    private void ResetToPage1() => Router.NavigateToAndReset(new Page1ViewModel(this));

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void NavigateBack()
    {
        Router.NavigateBack();
    }

    private bool CanNavigateBack() => Router.Stack.Count > 1;
}
