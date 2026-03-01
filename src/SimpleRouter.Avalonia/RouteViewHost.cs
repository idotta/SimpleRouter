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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RouterProperty)
        {
            if (change.OldValue is IRouter oldRouter)
                oldRouter.OnRouteChanged -= Router_OnRouteChanged;

            if (change.NewValue is IRouter newRouter)
            {
                newRouter.OnRouteChanged += Router_OnRouteChanged;
                NavigateToRoute(newRouter.Current);
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (Router is { } router)
            router.OnRouteChanged -= Router_OnRouteChanged;
        base.OnDetachedFromVisualTree(e);
    }

    private void Router_OnRouteChanged(object? sender, RouteChangedEventArgs e)
    {
        NavigateToRoute(e.Next);
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
        // Apply null to guarantee that the current content is unloaded before the new content is loaded
        Content = null;
        // If Router property is null, fall back to default content
        if (Router is null)
        {
            Content = DefaultContent;
            return;
        }
        // If the route is null, fall back to default content
        if (route is null)
        {
            Content = DefaultContent;
            return;
        }
        // If the view locator is null, assume the route is resolvable by the ContentControl
        if (ViewLocator is null)
        {
            Content = route;
            return;
        }
        // Resolve and apply the route
        var view = ViewLocator.Build(route);
        if (view is null)
        {
            Content = DefaultContent;
            return;
        }
        view.DataContext = route;
        Content = view;
    }
}
