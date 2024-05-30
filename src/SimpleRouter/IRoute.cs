namespace SimpleRouter;

public interface IRoute
{
    string RouteName { get; }
    IRouterHost RouterHost { get; }
}

public delegate IRoute? RouteFactory(Type routeType, params object[] parameters);