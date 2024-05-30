using Moq;

namespace SimpleRouter.Tests;

public class RouterTests
{
    [Fact]
    public void TestNavigateBack_WithEmptyStack_ShouldReturnNull()
    {
        // Arrange
        var mockRoute = new MockRoute(new Mock<IRouterHost>().Object);
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRoute))).Returns(mockRoute);
        var router = new Router(routeFactoryMock.Object);

        // Act
        var result = router.NavigateBack();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TestNavigateBack_WithNonEmptyStack_ShouldRemoveLastRouteAndReturnCurrent()
    {
        // Arrange
        var mockRoute = new MockRoute(new Mock<IRouterHost>().Object);
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRoute))).Returns(mockRoute);
        var router = new Router(routeFactoryMock.Object);
        var mockRoute1 = new MockRoute(new Mock<IRouterHost>().Object);
        var mockRoute2 = new MockRoute(new Mock<IRouterHost>().Object);
        router.NavigateTo(mockRoute1);
        router.NavigateTo(mockRoute2);

        // Act
        var result = router.NavigateBack();

        // Assert
        Assert.Equal(mockRoute1, result);
        Assert.Equal(mockRoute1, router.Current);
    }

    [Fact]
    public void TestNavigateTo_GenericType_ShouldCreateRouteAndSetAsCurrent()
    {
        // Arrange
        var mockRoute = new MockRoute(new Mock<IRouterHost>().Object);
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRoute))).Returns(mockRoute);
        var router = new Router(routeFactoryMock.Object);

        // Act
        var result = router.NavigateTo<MockRoute>();

        // Assert
        Assert.Equal(mockRoute, result);
        Assert.Equal(mockRoute, router.Current);
    }

    [Fact]
    public void TestNavigateTo_GenericTypeWithParameters_ShouldCreateRouteAndSetAsCurrent()
    {
        // Arrange
        var mockRoute = new MockRouteWithParams(new Mock<IRouterHost>().Object, 1, "test");
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRouteWithParams), It.IsAny<object[]>()))
            .Returns((Type type, object[] parameters) => mockRoute);
        var router = new Router(routeFactoryMock.Object);

        // Act
        var result = router.NavigateTo<MockRouteWithParams>(1, "test");

        // Assert
        Assert.Equal(mockRoute, result);
        Assert.Equal(mockRoute, router.Current);
    }

    [Fact]
    public void TestNavigateTo_GenericTypeWithParameters_ShouldCreateRouteWithCorrectParameters()
    {
        // Arrange
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRouteWithParams), It.IsAny<object[]>()))
            .Returns((Type type, object[] parameters) => new MockRouteWithParams(new Mock<IRouterHost>().Object, (int)parameters[0], (string)parameters[1]));
        var router = new Router(routeFactoryMock.Object);

        // Act
        var result = router.NavigateTo<MockRouteWithParams>(1, "test") as MockRouteWithParams;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.P1);
        Assert.Equal("test", result.P2);
    }

    [Fact]
    public void TestNavigateTo_Type_ShouldCreateRouteAndSetAsCurrent()
    {
        // Arrange
        var mockRoute = new MockRoute(new Mock<IRouterHost>().Object);
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRoute))).Returns(mockRoute);
        var router = new Router(routeFactoryMock.Object);

        // Act
        var result = router.NavigateTo(typeof(MockRoute));

        // Assert
        Assert.Equal(mockRoute, result);
        Assert.Equal(mockRoute, router.Current);
    }

    [Fact]
    public void TestNavigateTo_TypeWithParameters_ShouldCreateRouteAndSetAsCurrent()
    {
        // Arrange
        var mockRoute = new MockRouteWithParams(new Mock<IRouterHost>().Object, 1, "test");
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRouteWithParams), It.IsAny<object[]>()))
            .Returns((Type type, object[] parameters) => mockRoute);
        var router = new Router(routeFactoryMock.Object);

        // Act
        var result = router.NavigateTo(typeof(MockRouteWithParams), 1, "test");

        // Assert
        Assert.Equal(mockRoute, result);
        Assert.Equal(mockRoute, router.Current);
    }

    [Fact]
    public void TestNavigateTo_TypeWithParameters_ShouldCreateRouteWithCorrectParameters()
    {
        // Arrange
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRouteWithParams), It.IsAny<object[]>()))
            .Returns((Type type, object[] parameters) => new MockRouteWithParams(new Mock<IRouterHost>().Object, (int)parameters[0], (string)parameters[1]));
        var router = new Router(routeFactoryMock.Object);

        // Act
        var result = router.NavigateTo(typeof(MockRouteWithParams), 1, "test") as MockRouteWithParams;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.P1);
        Assert.Equal("test", result.P2);
    }

    [Fact]
    public void TestNavigateTo_IRoute_ShouldSetAsCurrent()
    {
        // Arrange
        var mockRoute = new MockRoute(new Mock<IRouterHost>().Object);
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRoute))).Returns(mockRoute);
        var router = new Router(routeFactoryMock.Object);

        // Act
        router.NavigateTo(mockRoute);

        // Assert
        Assert.Equal(mockRoute, router.Current);
    }

    [Fact]
    public void TestNavigateToAndReset_GenericType_ShouldCreateRouteAndSetAsCurrentAndResetStack()
    {
        // Arrange
        var mockRoute = new MockRoute(new Mock<IRouterHost>().Object);
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRoute))).Returns(mockRoute);
        var router = new Router(routeFactoryMock.Object);
        router.NavigateTo(new MockRoute(new Mock<IRouterHost>().Object));
        router.NavigateTo(new MockRoute(new Mock<IRouterHost>().Object));

        // Act
        var result = router.NavigateToAndReset<MockRoute>();

        // Assert
        Assert.Equal(mockRoute, result);
        Assert.Equal(mockRoute, router.Current);
        Assert.Single(router.Stack);
    }

    [Fact]
    public void TestNavigateToAndReset_GenericTypeWithParameters_ShouldCreateRouteAndSetAsCurrentAndResetStack()
    {
        // Arrange
        var mockRoute = new MockRouteWithParams(new Mock<IRouterHost>().Object, 1, "test");
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRouteWithParams), It.IsAny<object[]>()))
            .Returns((Type type, object[] parameters) => mockRoute);
        var router = new Router(routeFactoryMock.Object);

        // Act
        var result = router.NavigateToAndReset<MockRouteWithParams>(1, "test");

        // Assert
        Assert.Equal(mockRoute, result);
        Assert.Equal(mockRoute, router.Current);
        Assert.Single(router.Stack);
    }

    [Fact]
    public void TestNavigateToAndReset_Type_ShouldCreateRouteAndSetAsCurrentAndResetStack()
    {
        // Arrange
        var mockRoute = new MockRoute(new Mock<IRouterHost>().Object);
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRoute))).Returns(mockRoute);
        var router = new Router(routeFactoryMock.Object);
        router.NavigateTo(new MockRoute(new Mock<IRouterHost>().Object));
        router.NavigateTo(new MockRoute(new Mock<IRouterHost>().Object));

        // Act
        var result = router.NavigateToAndReset(typeof(MockRoute));

        // Assert
        Assert.Equal(mockRoute, result);
        Assert.Equal(mockRoute, router.Current);
        Assert.Single(router.Stack);
    }

    [Fact]
    public void TestNavigateToAndReset_TypeWithParameters_ShouldCreateRouteAndSetAsCurrentAndResetStack()
    {
        // Arrange
        var mockRoute = new MockRouteWithParams(new Mock<IRouterHost>().Object, 1, "test");
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRouteWithParams), It.IsAny<object[]>()))
            .Returns((Type type, object[] parameters) => mockRoute);
        var router = new Router(routeFactoryMock.Object);

        // Act
        var result = router.NavigateToAndReset(typeof(MockRouteWithParams), 1, "test");

        // Assert
        Assert.Equal(mockRoute, result);
        Assert.Equal(mockRoute, router.Current);
        Assert.Single(router.Stack);
    }

    [Fact]
    public void TestNavigateToAndReset_IRoute_ShouldSetAsCurrentAndResetStack()
    {
        // Arrange
        var mockRoute = new MockRoute(new Mock<IRouterHost>().Object);
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRoute))).Returns(mockRoute);
        var router = new Router(routeFactoryMock.Object);
        router.NavigateTo(new MockRoute(new Mock<IRouterHost>().Object));
        router.NavigateTo(new MockRoute(new Mock<IRouterHost>().Object));

        // Act
        router.NavigateToAndReset(mockRoute);

        // Assert
        Assert.Equal(mockRoute, router.Current);
        Assert.Single(router.Stack);
    }

    [Fact]
    public void TestNavigateTo_WithInvalidParams_ShouldThrowException()
    {
        // Arrange
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRouteWithParams))).Throws<InvalidOperationException>();
        var router = new Router(routeFactoryMock.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => router.NavigateTo<MockRouteWithParams>());
    }

    [Fact]
    public void TestNavigateTo_MultipleRoutes_ShouldHandleStackCorrectly()
    {
        // Arrange
        var routeFactoryMock = new Mock<RouteFactory>();
        var mockRoute1 = new MockRoute(new Mock<IRouterHost>().Object);
        var mockRoute2 = new MockRoute(new Mock<IRouterHost>().Object);
        routeFactoryMock.SetupSequence(factory => factory(typeof(MockRoute))).Returns(mockRoute1).Returns(mockRoute2);
        var router = new Router(routeFactoryMock.Object);

        // Act
        router.NavigateTo<MockRoute>();
        router.NavigateTo<MockRoute>();
        var result = router.NavigateBack();

        // Assert
        Assert.Equal(mockRoute1, result);
        Assert.Equal(mockRoute1, router.Current);
    }

    [Fact]
    public void TestNavigateTo_WithNullParameter_ShouldThrowException()
    {
        // Arrange
        var routeFactoryMock = new Mock<RouteFactory>();
        var router = new Router(routeFactoryMock.Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => router.NavigateTo((IRoute?)null));
    }

    [Fact]
    public void TestNavigateToAndReset_WithEmptyStack_ShouldWorkCorrectly()
    {
        // Arrange
        var mockRoute = new MockRoute(new Mock<IRouterHost>().Object);
        var routeFactoryMock = new Mock<RouteFactory>();
        routeFactoryMock.Setup(factory => factory(typeof(MockRoute))).Returns(mockRoute);
        var router = new Router(routeFactoryMock.Object);

        // Act
        var result = router.NavigateToAndReset<MockRoute>();

        // Assert
        Assert.Equal(mockRoute, result);
        Assert.Equal(mockRoute, router.Current);
        Assert.Single(router.Stack);
    }
}