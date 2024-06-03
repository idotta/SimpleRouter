using Avalonia.Controls;
using Moq;
using SimpleRouter.Avalonia;

namespace SimpleRouter.Tests;

public class AvaloniaViewLocatorTests
{
    [Fact]
    public void Build_WithNullParameter_ReturnsDefaultContent()
    {
        // Arrange
        var viewLocator = new Mock<ViewLocatorBase>() { CallBase = true };
        var defaultContent = new Control();
        viewLocator.Object.DefaultContent = defaultContent;

        // Act
        var result = viewLocator.Object.Build(null);

        // Assert
        Assert.Equal(defaultContent, result);
    }

    [Fact]
    public void Build_WithNonNullParameter_ResolvesControl()
    {
        // Arrange
        var viewLocator = new Mock<ViewLocatorBase>() { CallBase = true };
        var route = new Mock<IRoute>();
        var resolvedControl = new Control();
        viewLocator.Object.DefaultContent = new Control();
        viewLocator.Setup(x => x.ResolveControl(route.Object)).Returns(resolvedControl);

        // Act
        var result = viewLocator.Object.Build(route.Object);

        // Assert
        Assert.Equal(resolvedControl, result);
    }

    [Fact]
    public void Match_WithDataOfTypeIRoute_ReturnsTrue()
    {
        // Arrange
        var viewLocator = new Mock<ViewLocatorBase>() { CallBase = true }.Object;
        var data = new Mock<IRoute>().Object;

        // Act
        var result = viewLocator.Match(data);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Match_WithDataNotOfTypeIRoute_ReturnsFalse()
    {
        // Arrange
        var viewLocator = new Mock<ViewLocatorBase>() { CallBase = true }.Object;
        var data = new object();

        // Act
        var result = viewLocator.Match(data);

        // Assert
        Assert.False(result);
    }


}