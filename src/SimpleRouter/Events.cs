namespace SimpleRouter;

public sealed class RouteChangingEventArgs(IRoute? previous, IRoute? next) : EventArgs
{
    public IRoute? Previous { get; } = previous;
    public IRoute? Next { get; } = next;
}

public sealed class RouteChangedEventArgs(IRoute? next) : EventArgs
{
    public IRoute? Next { get; } = next;
}