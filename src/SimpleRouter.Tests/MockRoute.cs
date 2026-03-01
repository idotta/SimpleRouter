namespace SimpleRouter.Tests;

internal sealed class MockRoute(IRouterHost routerHost) : IRoute
{
    public string RouteName => "MockRoute";

    public IRouterHost RouterHost { get; } = routerHost;
}

internal sealed class MockRouteWithParams(IRouterHost routerHost, int p1, string p2) : IRoute
{
    public string RouteName => "MockRoute";

    public IRouterHost RouterHost { get; } = routerHost;
    public int P1 { get; } = p1;
    public string P2 { get; } = p2;
}
