using Avalonia.Controls;
using Moq;
using SimpleRouter.Avalonia;

namespace SimpleRouter.Tests;

public class AvaloniaRouteViewHostTests
{
    [Fact]
    public void NavigateToRoute_RouteIsNull_SetsDefaultContent()
    {
        // Arrange
        var routeViewHost = new RouteViewHost();
        var defaultContent = new object();
        routeViewHost.DefaultContent = defaultContent;
        var router = new Mock<IRouter>();
        router.Setup(r => r.Current).Returns<IRoute?>(null);

        // Act
        routeViewHost.Router = router.Object;

        // Assert
        Assert.Equal(defaultContent, routeViewHost.Content);
    }

    [Fact]
    public void NavigateToRoute_ViewLocatorIsNull_SetsRouteAsContent()
    {
        // Arrange
        var routeViewHost = new RouteViewHost();
        var route = new Mock<IRoute>().Object;
        var router = new Mock<IRouter>();
        router.Setup(r => r.Current).Returns(route);

        // Act
        routeViewHost.Router = router.Object;

        // Assert
        Assert.Equal(route, routeViewHost.Content);
    }

    [Fact]
    public void NavigateToRoute_ViewLocatorBuildReturnsNull_SetsDefaultContent()
    {
        // Arrange
        var routeViewHost = new RouteViewHost();
        var route = new Mock<IRoute>().Object;
        var viewLocator = new Mock<ViewLocatorBase>();
        var defaultContent = new object();
        var router = new Mock<IRouter>();
        router.Setup(r => r.Current).Returns(route);
        routeViewHost.ViewLocator = viewLocator.Object;
        viewLocator.Setup(v => v.Build(route)).Returns<Control?>(null);
        routeViewHost.DefaultContent = defaultContent;

        // Act
        routeViewHost.Router = router.Object;

        // Assert
        Assert.Equal(defaultContent, routeViewHost.Content);
    }

    [Fact]
    public void NavigateToRoute_ViewLocatorBuildReturnsView_SetsViewAsContent()
    {
        // Arrange
        var routeViewHost = new RouteViewHost();
        var route = new Mock<IRoute>().Object;
        var view = new Mock<Control>().Object;
        var viewLocator = new Mock<ViewLocatorBase>();
        routeViewHost.ViewLocator = viewLocator.Object;
        viewLocator.Setup(v => v.Build(route)).Returns(view);
        var router = new Mock<IRouter>();
        router.Setup(r => r.Current).Returns(route);

        // Act
        routeViewHost.Router = router.Object;

        // Assert
        Assert.Equal(view, routeViewHost.Content);
        Assert.Equal(route, view.DataContext);
    }
}
