namespace SimpleRouter;

public sealed class Router : IRouter
{
    private readonly RouteFactory _createRoute;
    private readonly int _stackLimit;
    private readonly List<IRoute> _stack = [];
    private IRoute? _current;

    public Router(RouteFactory createRoute, int stackLimit = 50)
    {
        ArgumentNullException.ThrowIfNull(createRoute);
        ArgumentOutOfRangeException.ThrowIfLessThan(stackLimit, 1);
        _createRoute = createRoute;
        _stackLimit = stackLimit;
    }

    public IReadOnlyList<IRoute> Stack => _stack;

    public IRoute? Current => _current;

    public event EventHandler<RouteChangingEventArgs>? OnRouteChanging;

    public event EventHandler<RouteChangedEventArgs>? OnRouteChanged;

    public IRoute? NavigateBack()
    {
        if (_stack.Count <= 1)
        {
            _stack.Clear();
            _current = null;
            return null;
        }
        _stack.RemoveAt(_stack.Count - 1);
        OnRouteChanging?.Invoke(this, new RouteChangingEventArgs(_current, _stack[^1]));
        _current = _stack[^1];
        OnRouteChanged?.Invoke(this, new RouteChangedEventArgs(_current));
        return Current;
    }

    public IRoute NavigateTo<T>() where T : IRoute
    {
        var destination = _createRoute(typeof(T)) ?? throw new InvalidOperationException("Failed to create view model");
        SetCurrent(destination);
        return destination;
    }

    public IRoute NavigateTo<T>(params object[] parameters) where T : IRoute
    {
        var destination = _createRoute(typeof(T), parameters) ?? throw new InvalidOperationException("Failed to create view model");
        SetCurrent(destination);
        return destination;
    }

    public IRoute NavigateTo(Type type)
    {
        var destination = _createRoute(type) ?? throw new InvalidOperationException("Failed to create view model");
        SetCurrent(destination);
        return destination;
    }

    public IRoute NavigateTo(Type type, params object[] parameters)
    {
        var destination = _createRoute(type, parameters) ?? throw new InvalidOperationException("Failed to create view model");
        SetCurrent(destination);
        return destination;
    }

    public void NavigateTo(IRoute destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        SetCurrent(destination);
    }

    public IRoute NavigateToAndReset<T>() where T : IRoute
    {
        var destination = _createRoute(typeof(T)) ?? throw new InvalidOperationException("Failed to create view model");
        _stack.Clear();
        SetCurrent(destination);
        return destination;
    }

    public IRoute NavigateToAndReset<T>(params object[] parameters) where T : IRoute
    {
        var destination = _createRoute(typeof(T), parameters) ?? throw new InvalidOperationException("Failed to create view model");
        _stack.Clear();
        SetCurrent(destination);
        return destination;
    }

    public IRoute NavigateToAndReset(Type type)
    {
        var destination = _createRoute(type) ?? throw new InvalidOperationException("Failed to create view model");
        _stack.Clear();
        SetCurrent(destination);
        return destination;
    }

    public IRoute NavigateToAndReset(Type type, params object[] parameters)
    {
        var destination = _createRoute(type, parameters) ?? throw new InvalidOperationException("Failed to create view model");
        _stack.Clear();
        SetCurrent(destination);
        return destination;
    }

    public void NavigateToAndReset(IRoute destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        _stack.Clear();
        SetCurrent(destination);
    }

    private void SetCurrent(IRoute destination)
    {
        if (destination != _current)
        {
            OnRouteChanging?.Invoke(this, new RouteChangingEventArgs(_current, destination));
            _current = destination;
            AddToStack(_current);
            OnRouteChanged?.Invoke(this, new RouteChangedEventArgs(_current));
        }
    }

    private void AddToStack(IRoute destination)
    {
        _stack.Add(destination);
        if (_stack.Count > _stackLimit)
        {
            _stack.RemoveAt(0);
        }
    }
}
