namespace SimpleRouter.Tests;

internal class MockRoute : IRoute
{
    public MockRoute(IRouterHost routerHost)
    {
        RouterHost = routerHost;
    }

    public string RouteName => "MockRoute";

    public IRouterHost RouterHost { get; }
}

internal class MockRouteWithParams : IRoute
{
    public MockRouteWithParams(IRouterHost routerHost, int p1, string p2)
    {
        RouterHost = routerHost;
        P1 = p1;
        P2 = p2;
    }

    public string RouteName => "MockRoute";

    public IRouterHost RouterHost { get; }
    public int P1 { get; }
    public string P2 { get; }
}