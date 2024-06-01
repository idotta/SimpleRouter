using Avalonia;
using Avalonia.Controls;

namespace SimpleRouter.Avalonia;

public class RouteViewHost : TransitioningContentControl
{
    /// <summary>
    /// <see cref="AvaloniaProperty"/> for the <see cref="Router"/> property.
    /// </summary>
    public static readonly StyledProperty<IRouter?> RouterProperty =
        AvaloniaProperty.Register<RouteViewHost, IRouter?>(nameof(Router));

    /// <summary>
    /// <see cref="AvaloniaProperty"/> for the <see cref="DefaultContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> DefaultContentProperty =
        AvaloniaProperty.Register<RouteViewHost, object?>(nameof(DefaultContent));

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutedViewHost"/> class.
    /// </summary>
    public RouteViewHost()
    {
        RouterObserver routerObserver = new(this);
        RouterProperty.Changed.Subscribe(routerObserver);
    }

    private sealed class RouterObserver : IObserver<AvaloniaPropertyChangedEventArgs<IRouter?>>
    {
        private readonly RouteViewHost _host;

        public RouterObserver(RouteViewHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(AvaloniaPropertyChangedEventArgs<IRouter?> value)
        {
            var (oldValue, newValue) = value.GetOldAndNewValue<IRouter?>();
            if (oldValue != null)
            {
                oldValue.OnRouteChanged -= NewValue_OnRouteChanged;
            }
            if (newValue != null)
            {
                newValue.OnRouteChanged += NewValue_OnRouteChanged;
            }
        }

        private void NewValue_OnRouteChanged(object? sender, RouteChangedEventArgs e)
        {
            _host.NavigateToRoute(e.Next);
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="IRouter"/> of the view model stack.
    /// </summary>
    public IRouter? Router
    {
        get => GetValue(RouterProperty);
        set => SetValue(RouterProperty, value);
    }

    /// <summary>
    /// Gets or sets the content displayed whenever there is no page currently routed.
    /// </summary>
    public object? DefaultContent
    {
        get => GetValue(DefaultContentProperty);
        set => SetValue(DefaultContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the view locator used by this router.
    /// </summary>
    public ViewLocatorBase? ViewLocator { get; set; }

    protected override Type StyleKeyOverride => typeof(TransitioningContentControl);

    /// <summary>
    /// Invoked when <see cref="Router"/> navigates to a new route.
    /// </summary>
    /// <param name="route">Route to which the user navigates.</param>
    private void NavigateToRoute(IRoute? route)
    {
        if (Router == null)
        {
            System.Diagnostics.Debug.WriteLine("Router property is null. Falling back to default content.");
            Content = DefaultContent;
            return;
        }

        if (route == null)
        {
            System.Diagnostics.Debug.WriteLine("Route is null. Falling back to default content.");
            Content = DefaultContent;
            return;
        }

        if (ViewLocator == null)
        {
            Content = route;
            return;
        }

        var view = ViewLocator.Build(route);
        if (view == null)
        {
            Content = DefaultContent;
            return;
        }
        
        view.DataContext = route;
        Content = view;
    }
}