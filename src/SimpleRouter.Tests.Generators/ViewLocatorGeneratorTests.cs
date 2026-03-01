using Microsoft.CodeAnalysis;
using SimpleRouter.SourceGenerators;

namespace SimpleRouter.Tests.Generators;

public class ViewLocatorGeneratorTests
{
    private const string Preamble = @"
namespace SimpleRouter
{
    public interface IRoute { string RouteName { get; } IRouterHost RouterHost { get; } }
    public interface IRouterHost { }
    public delegate IRoute? RouteFactory(System.Type routeType, params object[] parameters);
}
";

    /// <summary>
    /// Mock ViewLocatorBase for tests that need the base class to satisfy SR0003.
    /// Avalonia is not referenced in the test assembly, so we provide a stand-in
    /// that mirrors the namespace and shape the generator checks for.
    /// </summary>
    private const string ViewLocatorBaseMock = @"
namespace Avalonia.Controls
{
    public class Control { }
}

namespace SimpleRouter.Avalonia
{
    public abstract class ViewLocatorBase
    {
        public object? DefaultContent { get; set; }
        public abstract Avalonia.Controls.Control? ResolveControl(SimpleRouter.IRoute? route);
    }
}
";

    // ---------------------------------------------------------------
    // Attribute emission
    // ---------------------------------------------------------------

    [Fact]
    public void EmitsRouteViewAttribute()
    {
        var source = Preamble;
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);

        Assert.Contains(sources, s => s.HintName == "RouteViewAttribute.g.cs");
    }

    [Fact]
    public void EmitsGeneratedViewLocatorAttribute()
    {
        var source = Preamble;
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);

        Assert.Contains(sources, s => s.HintName == "GeneratedViewLocatorAttribute.g.cs");
    }

    // ---------------------------------------------------------------
    // Basic generation (no groups)
    // ---------------------------------------------------------------

    [Fact]
    public void SingleMapping_NoGroup_ProducesLocator()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace TestApp
{
    public class HomeRoute : SimpleRouter.IRoute
    {
        public string RouteName => ""Home"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    [SimpleRouter.RouteView(typeof(HomeRoute))]
    public class HomeView : Avalonia.Controls.Control
    {
    }

    [SimpleRouter.GeneratedViewLocator]
    public partial class MyLocator : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);
        var diagnostics = GeneratorTestHelper.GetDiagnostics<ViewLocatorGenerator>(source);

        // Filter out SR0005 warnings since Control mock may not fully satisfy check
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
        Assert.Empty(errors);

        var locatorSource = Assert.Single(sources, s => s.HintName == "MyLocator.ViewLocator.g.cs");
        Assert.Contains("ResolveControl", locatorSource.Source);
        Assert.Contains("global::TestApp.HomeRoute", locatorSource.Source);
        Assert.Contains("global::TestApp.HomeView", locatorSource.Source);
        Assert.Contains("DataContext", locatorSource.Source);
    }

    [Fact]
    public void MultipleMappings_NoGroup_ProducesLocatorWithAll()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace TestApp
{
    public class Route1 : SimpleRouter.IRoute
    {
        public string RouteName => ""R1"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    public class Route2 : SimpleRouter.IRoute
    {
        public string RouteName => ""R2"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    public class Route3 : SimpleRouter.IRoute
    {
        public string RouteName => ""R3"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    [SimpleRouter.RouteView(typeof(Route1))]
    public class View1 : Avalonia.Controls.Control { }

    [SimpleRouter.RouteView(typeof(Route2))]
    public class View2 : Avalonia.Controls.Control { }

    [SimpleRouter.RouteView(typeof(Route3))]
    public class View3 : Avalonia.Controls.Control { }

    [SimpleRouter.GeneratedViewLocator]
    public partial class MyLocator : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);
        var locatorSource = Assert.Single(sources, s => s.HintName == "MyLocator.ViewLocator.g.cs");

        Assert.Contains("global::TestApp.Route1", locatorSource.Source);
        Assert.Contains("global::TestApp.View1", locatorSource.Source);
        Assert.Contains("global::TestApp.Route2", locatorSource.Source);
        Assert.Contains("global::TestApp.View2", locatorSource.Source);
        Assert.Contains("global::TestApp.Route3", locatorSource.Source);
        Assert.Contains("global::TestApp.View3", locatorSource.Source);
    }

    [Fact]
    public void NoMappings_ProducesEmptyLocator()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace TestApp
{
    [SimpleRouter.GeneratedViewLocator]
    public partial class MyLocator : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);
        var locatorSource = Assert.Single(sources, s => s.HintName == "MyLocator.ViewLocator.g.cs");

        // Should generate ResolveControl but with only DefaultContent fallback
        Assert.Contains("ResolveControl", locatorSource.Source);
        Assert.Contains("DefaultContent", locatorSource.Source);
    }

    // ---------------------------------------------------------------
    // Group scoping
    // ---------------------------------------------------------------

    [Fact]
    public void MatchingGroup_OnlyIncludesMatchingMappings()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace TestApp
{
    public class RouteA : SimpleRouter.IRoute
    {
        public string RouteName => ""A"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    public class RouteB : SimpleRouter.IRoute
    {
        public string RouteName => ""B"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    [SimpleRouter.RouteView(typeof(RouteA), Group = ""A"")]
    public class ViewA : Avalonia.Controls.Control { }

    [SimpleRouter.RouteView(typeof(RouteB), Group = ""B"")]
    public class ViewB : Avalonia.Controls.Control { }

    [SimpleRouter.GeneratedViewLocator(Group = ""A"")]
    public partial class LocatorA : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);
        var locatorSource = Assert.Single(sources, s => s.HintName == "LocatorA.ViewLocator.g.cs");

        Assert.Contains("global::TestApp.RouteA", locatorSource.Source);
        Assert.Contains("global::TestApp.ViewA", locatorSource.Source);
        Assert.DoesNotContain("global::TestApp.RouteB", locatorSource.Source);
        Assert.DoesNotContain("global::TestApp.ViewB", locatorSource.Source);
    }

    [Fact]
    public void NoGroup_DoesNotIncludeGroupedMappings()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace TestApp
{
    public class GroupedRoute : SimpleRouter.IRoute
    {
        public string RouteName => ""Grouped"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    public class UngroupedRoute : SimpleRouter.IRoute
    {
        public string RouteName => ""Ungrouped"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    [SimpleRouter.RouteView(typeof(GroupedRoute), Group = ""A"")]
    public class GroupedView : Avalonia.Controls.Control { }

    [SimpleRouter.RouteView(typeof(UngroupedRoute))]
    public class UngroupedView : Avalonia.Controls.Control { }

    [SimpleRouter.GeneratedViewLocator]
    public partial class DefaultLocator : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);
        var locatorSource = Assert.Single(sources, s => s.HintName == "DefaultLocator.ViewLocator.g.cs");

        Assert.Contains("global::TestApp.UngroupedRoute", locatorSource.Source);
        Assert.Contains("global::TestApp.UngroupedView", locatorSource.Source);
        Assert.DoesNotContain("global::TestApp.GroupedRoute", locatorSource.Source);
        Assert.DoesNotContain("global::TestApp.GroupedView", locatorSource.Source);
    }

    [Fact]
    public void GroupedLocator_DoesNotIncludeUngroupedMappings()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace TestApp
{
    public class UngroupedRoute : SimpleRouter.IRoute
    {
        public string RouteName => ""Ungrouped"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    [SimpleRouter.RouteView(typeof(UngroupedRoute))]
    public class UngroupedView : Avalonia.Controls.Control { }

    [SimpleRouter.GeneratedViewLocator(Group = ""A"")]
    public partial class GroupedLocator : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);
        var locatorSource = Assert.Single(sources, s => s.HintName == "GroupedLocator.ViewLocator.g.cs");

        Assert.DoesNotContain("global::TestApp.UngroupedRoute", locatorSource.Source);
        Assert.DoesNotContain("global::TestApp.UngroupedView", locatorSource.Source);
    }

    [Fact]
    public void MultipleLocators_DifferentGroups()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace TestApp
{
    public class RouteX : SimpleRouter.IRoute
    {
        public string RouteName => ""X"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    public class RouteY : SimpleRouter.IRoute
    {
        public string RouteName => ""Y"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    [SimpleRouter.RouteView(typeof(RouteX), Group = ""X"")]
    public class ViewX : Avalonia.Controls.Control { }

    [SimpleRouter.RouteView(typeof(RouteY), Group = ""Y"")]
    public class ViewY : Avalonia.Controls.Control { }

    [SimpleRouter.GeneratedViewLocator(Group = ""X"")]
    public partial class LocatorX : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }

    [SimpleRouter.GeneratedViewLocator(Group = ""Y"")]
    public partial class LocatorY : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);

        var locatorX = Assert.Single(sources, s => s.HintName == "LocatorX.ViewLocator.g.cs");
        Assert.Contains("global::TestApp.RouteX", locatorX.Source);
        Assert.Contains("global::TestApp.ViewX", locatorX.Source);
        Assert.DoesNotContain("global::TestApp.RouteY", locatorX.Source);
        Assert.DoesNotContain("global::TestApp.ViewY", locatorX.Source);

        var locatorY = Assert.Single(sources, s => s.HintName == "LocatorY.ViewLocator.g.cs");
        Assert.Contains("global::TestApp.RouteY", locatorY.Source);
        Assert.Contains("global::TestApp.ViewY", locatorY.Source);
        Assert.DoesNotContain("global::TestApp.RouteX", locatorY.Source);
        Assert.DoesNotContain("global::TestApp.ViewX", locatorY.Source);
    }

    [Fact]
    public void EmptyGroupMatchesDefaultGroup()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace TestApp
{
    public class Route1 : SimpleRouter.IRoute
    {
        public string RouteName => ""R1"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    public class Route2 : SimpleRouter.IRoute
    {
        public string RouteName => ""R2"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    [SimpleRouter.RouteView(typeof(Route1), Group = """")]
    public class View1 : Avalonia.Controls.Control { }

    [SimpleRouter.RouteView(typeof(Route2))]
    public class View2 : Avalonia.Controls.Control { }

    [SimpleRouter.GeneratedViewLocator(Group = """")]
    public partial class ExplicitEmptyLocator : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }

    [SimpleRouter.GeneratedViewLocator]
    public partial class ImplicitDefaultLocator : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);

        // Both locators should include both views since Group="" and no Group are equivalent
        var explicitLocator = Assert.Single(sources, s => s.HintName == "ExplicitEmptyLocator.ViewLocator.g.cs");
        Assert.Contains("global::TestApp.Route1", explicitLocator.Source);
        Assert.Contains("global::TestApp.View1", explicitLocator.Source);
        Assert.Contains("global::TestApp.Route2", explicitLocator.Source);
        Assert.Contains("global::TestApp.View2", explicitLocator.Source);

        var implicitLocator = Assert.Single(sources, s => s.HintName == "ImplicitDefaultLocator.ViewLocator.g.cs");
        Assert.Contains("global::TestApp.Route1", implicitLocator.Source);
        Assert.Contains("global::TestApp.View1", implicitLocator.Source);
        Assert.Contains("global::TestApp.Route2", implicitLocator.Source);
        Assert.Contains("global::TestApp.View2", implicitLocator.Source);
    }

    // ---------------------------------------------------------------
    // Diagnostics
    // ---------------------------------------------------------------

    [Fact]
    public void SR0002_NonPartialLocator()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.GeneratedViewLocator]
    public class NonPartialLocator
    {
    }
}
";
        var diagnostics = GeneratorTestHelper.GetDiagnostics<ViewLocatorGenerator>(source);

        Assert.Contains(diagnostics, d => d.Id == "SR0002");
    }

    [Fact]
    public void SR0003_DoesNotExtendViewLocatorBase()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace TestApp
{
    [SimpleRouter.GeneratedViewLocator]
    public partial class WrongBaseLocator
    {
    }
}
";
        var diagnostics = GeneratorTestHelper.GetDiagnostics<ViewLocatorGenerator>(source);

        Assert.Contains(diagnostics, d => d.Id == "SR0003");
    }

    [Fact]
    public void SR0002_DoesNotGenerateCode()
    {
        var source = Preamble + @"
namespace TestApp
{
    public class SomeRoute : SimpleRouter.IRoute
    {
        public string RouteName => ""Some"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    [SimpleRouter.RouteView(typeof(SomeRoute))]
    public class SomeView { }

    [SimpleRouter.GeneratedViewLocator]
    public class NonPartialLocator
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);

        // Should NOT generate a locator file when the class is not partial
        Assert.DoesNotContain(sources, s => s.HintName == "NonPartialLocator.ViewLocator.g.cs");
    }

    [Fact]
    public void SR0003_DoesNotGenerateCode()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace TestApp
{
    public class SomeRoute : SimpleRouter.IRoute
    {
        public string RouteName => ""Some"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    [SimpleRouter.RouteView(typeof(SomeRoute))]
    public class SomeView : Avalonia.Controls.Control { }

    [SimpleRouter.GeneratedViewLocator]
    public partial class WrongBaseLocator
    {
        // partial but does NOT extend ViewLocatorBase
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);

        // Should NOT generate a locator file when the class doesn't extend ViewLocatorBase
        Assert.DoesNotContain(sources, s => s.HintName == "WrongBaseLocator.ViewLocator.g.cs");
    }

    // ---------------------------------------------------------------
    // Edge cases
    // ---------------------------------------------------------------

    [Fact]
    public void Locator_InGlobalNamespace()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
public class GlobalRoute : SimpleRouter.IRoute
{
    public string RouteName => ""Global"";
    public SimpleRouter.IRouterHost RouterHost { get; } = null!;
}

[SimpleRouter.RouteView(typeof(GlobalRoute))]
public class GlobalView : Avalonia.Controls.Control { }

[SimpleRouter.GeneratedViewLocator]
public partial class GlobalLocator : SimpleRouter.Avalonia.ViewLocatorBase
{
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);
        var locatorSource = Assert.Single(sources, s => s.HintName == "GlobalLocator.ViewLocator.g.cs");

        Assert.Contains("ResolveControl", locatorSource.Source);
        Assert.Contains("global::GlobalRoute", locatorSource.Source);
        Assert.Contains("global::GlobalView", locatorSource.Source);
        // Should NOT wrap in a namespace block
        Assert.DoesNotContain("namespace", locatorSource.Source);
    }

    [Fact]
    public void View_InDifferentNamespace()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace Models
{
    public class DetailRoute : SimpleRouter.IRoute
    {
        public string RouteName => ""Detail"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }
}

namespace Views
{
    [SimpleRouter.RouteView(typeof(Models.DetailRoute))]
    public class DetailView : Avalonia.Controls.Control { }
}

namespace App
{
    [SimpleRouter.GeneratedViewLocator]
    public partial class MyLocator : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);
        var locatorSource = Assert.Single(sources, s => s.HintName == "MyLocator.ViewLocator.g.cs");

        // Should use fully qualified names from different namespaces
        Assert.Contains("namespace App", locatorSource.Source);
        Assert.Contains("global::Models.DetailRoute", locatorSource.Source);
        Assert.Contains("global::Views.DetailView", locatorSource.Source);
    }

    [Fact]
    public void MultipleLocators_SameGroup()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace TestApp
{
    public class SharedRoute : SimpleRouter.IRoute
    {
        public string RouteName => ""Shared"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    [SimpleRouter.RouteView(typeof(SharedRoute), Group = ""Shared"")]
    public class SharedView : Avalonia.Controls.Control { }

    [SimpleRouter.GeneratedViewLocator(Group = ""Shared"")]
    public partial class LocatorOne : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }

    [SimpleRouter.GeneratedViewLocator(Group = ""Shared"")]
    public partial class LocatorTwo : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<ViewLocatorGenerator>(source);

        var locatorOne = Assert.Single(sources, s => s.HintName == "LocatorOne.ViewLocator.g.cs");
        Assert.Contains("global::TestApp.SharedRoute", locatorOne.Source);
        Assert.Contains("global::TestApp.SharedView", locatorOne.Source);

        var locatorTwo = Assert.Single(sources, s => s.HintName == "LocatorTwo.ViewLocator.g.cs");
        Assert.Contains("global::TestApp.SharedRoute", locatorTwo.Source);
        Assert.Contains("global::TestApp.SharedView", locatorTwo.Source);
    }

    [Fact]
    public void SR0005_ViewDoesNotExtendControl()
    {
        var source = Preamble + ViewLocatorBaseMock + @"
namespace TestApp
{
    public class SomeRoute : SimpleRouter.IRoute
    {
        public string RouteName => ""Some"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    [SimpleRouter.RouteView(typeof(SomeRoute))]
    public class NotAControl
    {
        // This class does NOT extend Avalonia.Controls.Control
    }

    [SimpleRouter.GeneratedViewLocator]
    public partial class TestLocator : SimpleRouter.Avalonia.ViewLocatorBase
    {
    }
}
";
        var diagnostics = GeneratorTestHelper.GetDiagnostics<ViewLocatorGenerator>(source);

        Assert.Contains(diagnostics, d => d.Id == "SR0005");
    }
}
