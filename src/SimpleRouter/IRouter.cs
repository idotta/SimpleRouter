namespace SimpleRouter;

public interface IRouter
{
    public IReadOnlyList<IRoute?> Stack { get; }
    public IRoute? Current { get; }

    public event EventHandler<RouteChangingEventArgs>? OnRouteChanging;

    public event EventHandler<RouteChangedEventArgs>? OnRouteChanged;

    IRoute? NavigateTo<T>() where T : IRoute;

    IRoute? NavigateTo<T>(params object[] parameters) where T : IRoute;

    IRoute? NavigateTo(Type type);

    IRoute? NavigateTo(Type type, params object[] parameters);

    void NavigateTo(IRoute destination);

    IRoute? NavigateToAndReset<T>() where T : IRoute;

    IRoute? NavigateToAndReset<T>(params object[] parameters) where T : IRoute;

    IRoute? NavigateToAndReset(Type type);

    IRoute? NavigateToAndReset(Type type, params object[] parameters);

    void NavigateToAndReset(IRoute destination);

    IRoute? NavigateBack();
}