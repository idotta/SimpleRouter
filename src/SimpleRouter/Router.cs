namespace SimpleRouter;

public sealed class Router(RouteFactory createRoute) : IRouter
{
    private const int s_stackLimit = 50;
    private IRoute? _current;
    private readonly List<IRoute?> _stack = [];
    private readonly RouteFactory _createRoute = createRoute ?? throw new ArgumentNullException(nameof(createRoute));

    public IReadOnlyList<IRoute?> Stack => _stack;

    public IRoute? Current
    {
        get => _current;
        private set
        {
            if (value != _current)
            {
                OnRouteChanging?.Invoke(this, new RouteChangingEventArgs(_current, value));
                _current = value;
                AddToStack(_current);
                OnRouteChanged?.Invoke(this, new RouteChangedEventArgs(_current));
            }
        }
    }

    public event EventHandler<RouteChangingEventArgs>? OnRouteChanging;

    public event EventHandler<RouteChangedEventArgs>? OnRouteChanged;

    public IRoute? NavigateBack()
    {
        if (_stack.Count <= 1)
        {
            _stack.Clear();
            return null;
        }
        _stack.RemoveAt(_stack.Count - 1);
        OnRouteChanging?.Invoke(this, new RouteChangingEventArgs(_current, _stack[^1]));
        _current = _stack[^1];
        OnRouteChanged?.Invoke(this, new RouteChangedEventArgs(_current));
        return Current;
    }

    public IRoute? NavigateTo<T>() where T : IRoute
    {
        var destination = _createRoute(typeof(T)) ?? throw new InvalidOperationException("Failed to create view model");
        Current = destination;
        return Current;
    }

    public IRoute? NavigateTo<T>(params object[] parameters) where T : IRoute
    {
        var destination = _createRoute(typeof(T), parameters) ?? throw new InvalidOperationException("Failed to create view model");
        Current = destination;
        return Current;
    }

    public IRoute? NavigateTo(Type type)
    {
        var destination = _createRoute(type) ?? throw new InvalidOperationException("Failed to create view model");
        Current = destination;
        return Current;
    }

    public IRoute? NavigateTo(Type type, params object[] parameters)
    {
        var destination = _createRoute(type, parameters) ?? throw new InvalidOperationException("Failed to create view model");
        Current = destination;
        return Current;
    }

    public void NavigateTo(IRoute destination)
    {
        Current = destination;
    }

    public IRoute? NavigateToAndReset<T>() where T : IRoute
    {
        var destination = _createRoute(typeof(T)) ?? throw new InvalidOperationException("Failed to create view model");
        _stack.Clear();
        Current = destination;
        return Current;
    }

    public IRoute? NavigateToAndReset<T>(params object[] parameters) where T : IRoute
    {
        var destination = _createRoute(typeof(T), parameters);
        _stack.Clear();
        Current = destination;
        return Current;
    }

    public IRoute? NavigateToAndReset(Type type)
    {
        var destination = _createRoute(type) ?? throw new InvalidOperationException("Failed to create view model");
        _stack.Clear();
        Current = destination;
        return Current;
    }

    public IRoute? NavigateToAndReset(Type type, params object[] parameters)
    {
        var destination = _createRoute(type, parameters) ?? throw new InvalidOperationException("Failed to create view model");
        _stack.Clear();
        Current = destination;
        return Current;
    }

    public void NavigateToAndReset(IRoute destination)
    {
        _stack.Clear();
        Current = destination;
    }

    private void AddToStack(IRoute? destination)
    {
        _stack.Add(destination);
        if (_stack.Count > s_stackLimit)
        {
            _stack.RemoveAt(0);
        }
    }
}