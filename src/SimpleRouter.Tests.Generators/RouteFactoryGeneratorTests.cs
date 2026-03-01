using Microsoft.CodeAnalysis;
using SimpleRouter.SourceGenerators;

namespace SimpleRouter.Tests.Generators;

public class RouteFactoryGeneratorTests
{
    private const string Preamble = @"
namespace SimpleRouter
{
    public interface IRoute { string RouteName { get; } IRouterHost RouterHost { get; } }
    public interface IRouterHost { }
    public delegate IRoute? RouteFactory(System.Type routeType, params object[] parameters);
}
";

    // ---------------------------------------------------------------
    // Attribute emission
    // ---------------------------------------------------------------

    [Fact]
    public void EmitsRouteAttribute()
    {
        var source = Preamble;
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);

        Assert.Contains(sources, s => s.HintName == "RouteAttribute.g.cs");
    }

    [Fact]
    public void EmitsGeneratedRouteFactoryAttribute()
    {
        var source = Preamble;
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);

        Assert.Contains(sources, s => s.HintName == "GeneratedRouteFactoryAttribute.g.cs");
    }

    // ---------------------------------------------------------------
    // Basic generation (no groups)
    // ---------------------------------------------------------------

    [Fact]
    public void SingleRoute_NoGroup_ProducesFactory()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route]
    public class HomeRoute : SimpleRouter.IRoute
    {
        public HomeRoute(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""Home"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.GeneratedRouteFactory]
    public partial class MyHost
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);
        var diagnostics = GeneratorTestHelper.GetDiagnostics<RouteFactoryGenerator>(source);

        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);

        var factorySource = Assert.Single(sources, s => s.HintName == "MyHost.RouteFactory.g.cs");
        Assert.Contains("GeneratedCreateRoutes", factorySource.Source);
        Assert.Contains("global::TestApp.HomeRoute", factorySource.Source);
        Assert.Contains("CreateGeneratedRouteFactory", factorySource.Source);
    }

    [Fact]
    public void MultipleRoutes_NoGroup_ProducesFactoryWithAll()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route]
    public class Route1 : SimpleRouter.IRoute
    {
        public Route1(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""R1"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.Route]
    public class Route2 : SimpleRouter.IRoute
    {
        public Route2(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""R2"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.Route]
    public class Route3 : SimpleRouter.IRoute
    {
        public Route3(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""R3"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.GeneratedRouteFactory]
    public partial class MyHost
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);
        var factorySource = Assert.Single(sources, s => s.HintName == "MyHost.RouteFactory.g.cs");

        Assert.Contains("global::TestApp.Route1", factorySource.Source);
        Assert.Contains("global::TestApp.Route2", factorySource.Source);
        Assert.Contains("global::TestApp.Route3", factorySource.Source);
    }

    [Fact]
    public void NoRoutes_ProducesEmptyFactory()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.GeneratedRouteFactory]
    public partial class MyHost
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);
        var factorySource = Assert.Single(sources, s => s.HintName == "MyHost.RouteFactory.g.cs");

        // The method should exist but return null for all types (no route branches)
        Assert.Contains("GeneratedCreateRoutes", factorySource.Source);
        Assert.Contains("return null", factorySource.Source);
    }

    // ---------------------------------------------------------------
    // Group scoping
    // ---------------------------------------------------------------

    [Fact]
    public void MatchingGroup_OnlyIncludesMatchingRoutes()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route(Group = ""A"")]
    public class RouteA : SimpleRouter.IRoute
    {
        public RouteA(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""A"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.Route(Group = ""B"")]
    public class RouteB : SimpleRouter.IRoute
    {
        public RouteB(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""B"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.GeneratedRouteFactory(Group = ""A"")]
    public partial class HostA
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);
        var factorySource = Assert.Single(sources, s => s.HintName == "HostA.RouteFactory.g.cs");

        Assert.Contains("global::TestApp.RouteA", factorySource.Source);
        Assert.DoesNotContain("global::TestApp.RouteB", factorySource.Source);
    }

    [Fact]
    public void NoGroup_DoesNotIncludeGroupedRoutes()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route(Group = ""A"")]
    public class GroupedRoute : SimpleRouter.IRoute
    {
        public GroupedRoute(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""Grouped"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.Route]
    public class UngroupedRoute : SimpleRouter.IRoute
    {
        public UngroupedRoute(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""Ungrouped"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.GeneratedRouteFactory]
    public partial class DefaultHost
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);
        var factorySource = Assert.Single(sources, s => s.HintName == "DefaultHost.RouteFactory.g.cs");

        Assert.Contains("global::TestApp.UngroupedRoute", factorySource.Source);
        Assert.DoesNotContain("global::TestApp.GroupedRoute", factorySource.Source);
    }

    [Fact]
    public void GroupedFactory_DoesNotIncludeUngroupedRoutes()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route]
    public class UngroupedRoute : SimpleRouter.IRoute
    {
        public UngroupedRoute(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""Ungrouped"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.GeneratedRouteFactory(Group = ""A"")]
    public partial class GroupedHost
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);
        var factorySource = Assert.Single(sources, s => s.HintName == "GroupedHost.RouteFactory.g.cs");

        Assert.DoesNotContain("global::TestApp.UngroupedRoute", factorySource.Source);
    }

    [Fact]
    public void MultipleFactories_DifferentGroups()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route(Group = ""X"")]
    public class RouteX : SimpleRouter.IRoute
    {
        public RouteX(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""X"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.Route(Group = ""Y"")]
    public class RouteY : SimpleRouter.IRoute
    {
        public RouteY(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""Y"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.GeneratedRouteFactory(Group = ""X"")]
    public partial class HostX
    {
    }

    [SimpleRouter.GeneratedRouteFactory(Group = ""Y"")]
    public partial class HostY
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);

        var factoryX = Assert.Single(sources, s => s.HintName == "HostX.RouteFactory.g.cs");
        Assert.Contains("global::TestApp.RouteX", factoryX.Source);
        Assert.DoesNotContain("global::TestApp.RouteY", factoryX.Source);

        var factoryY = Assert.Single(sources, s => s.HintName == "HostY.RouteFactory.g.cs");
        Assert.Contains("global::TestApp.RouteY", factoryY.Source);
        Assert.DoesNotContain("global::TestApp.RouteX", factoryY.Source);
    }

    [Fact]
    public void EmptyGroupMatchesDefaultGroup()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route(Group = """")]
    public class ExplicitEmptyRoute : SimpleRouter.IRoute
    {
        public ExplicitEmptyRoute(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""Empty"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.Route]
    public class ImplicitDefaultRoute : SimpleRouter.IRoute
    {
        public ImplicitDefaultRoute(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""Default"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.GeneratedRouteFactory(Group = """")]
    public partial class ExplicitEmptyHost
    {
    }

    [SimpleRouter.GeneratedRouteFactory]
    public partial class ImplicitDefaultHost
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);

        // Both factories should contain both routes since Group="" and no Group are equivalent
        var explicitFactory = Assert.Single(sources, s => s.HintName == "ExplicitEmptyHost.RouteFactory.g.cs");
        Assert.Contains("global::TestApp.ExplicitEmptyRoute", explicitFactory.Source);
        Assert.Contains("global::TestApp.ImplicitDefaultRoute", explicitFactory.Source);

        var implicitFactory = Assert.Single(sources, s => s.HintName == "ImplicitDefaultHost.RouteFactory.g.cs");
        Assert.Contains("global::TestApp.ExplicitEmptyRoute", implicitFactory.Source);
        Assert.Contains("global::TestApp.ImplicitDefaultRoute", implicitFactory.Source);
    }

    // ---------------------------------------------------------------
    // Constructor handling
    // ---------------------------------------------------------------

    [Fact]
    public void Route_NoPublicConstructor_EmptyParams()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route]
    public class PrivateCtorRoute : SimpleRouter.IRoute
    {
        private PrivateCtorRoute() { }
        public string RouteName => ""Private"";
        public SimpleRouter.IRouterHost RouterHost { get; } = null!;
    }

    [SimpleRouter.GeneratedRouteFactory]
    public partial class MyHost
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);
        var factorySource = Assert.Single(sources, s => s.HintName == "MyHost.RouteFactory.g.cs");

        Assert.Contains("global::TestApp.PrivateCtorRoute", factorySource.Source);
        // Should generate with no constructor parameters (empty params)
        Assert.Contains("new global::TestApp.PrivateCtorRoute()", factorySource.Source);
    }

    [Fact]
    public void Route_MultipleConstructors_PicksLongest()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route]
    public class MultiCtorRoute : SimpleRouter.IRoute
    {
        public MultiCtorRoute(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public MultiCtorRoute(SimpleRouter.IRouterHost host, int id, string name)
        {
            RouterHost = host;
        }
        public string RouteName => ""Multi"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.GeneratedRouteFactory]
    public partial class MyHost
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);
        var factorySource = Assert.Single(sources, s => s.HintName == "MyHost.RouteFactory.g.cs");

        // Should pick the 3-param constructor (longest)
        Assert.Contains("global::TestApp.MultiCtorRoute", factorySource.Source);
        // Should have casts for 3 parameters: IRouterHost, int, string
        Assert.Contains("parameters[0]", factorySource.Source);
        Assert.Contains("parameters[1]", factorySource.Source);
        Assert.Contains("parameters[2]", factorySource.Source);
    }

    [Fact]
    public void Route_ConstructorParams_CorrectCasts()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route]
    public class TypedRoute : SimpleRouter.IRoute
    {
        public TypedRoute(SimpleRouter.IRouterHost host, int id, string name)
        {
            RouterHost = host;
        }
        public string RouteName => ""Typed"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.GeneratedRouteFactory]
    public partial class MyHost
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);
        var factorySource = Assert.Single(sources, s => s.HintName == "MyHost.RouteFactory.g.cs");

        // Verify fully qualified type names in parameter casts
        Assert.Contains("(global::SimpleRouter.IRouterHost)parameters[0]", factorySource.Source);
        Assert.Contains("(int)parameters[1]", factorySource.Source);
        Assert.Contains("(string)parameters[2]", factorySource.Source);
    }

    // ---------------------------------------------------------------
    // Diagnostics
    // ---------------------------------------------------------------

    [Fact]
    public void SR0001_NonPartialFactory()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.GeneratedRouteFactory]
    public class NonPartialHost
    {
    }
}
";
        var diagnostics = GeneratorTestHelper.GetDiagnostics<RouteFactoryGenerator>(source);

        Assert.Contains(diagnostics, d => d.Id == "SR0001");
    }

    [Fact]
    public void SR0004_RouteWithoutIRoute()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route]
    public class NotARoute
    {
    }

    [SimpleRouter.GeneratedRouteFactory]
    public partial class Host
    {
    }
}
";
        var diagnostics = GeneratorTestHelper.GetDiagnostics<RouteFactoryGenerator>(source);

        Assert.Contains(diagnostics, d => d.Id == "SR0004");
    }

    [Fact]
    public void SR0001_DoesNotGenerateCode()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route]
    public class HomeRoute : SimpleRouter.IRoute
    {
        public HomeRoute(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""Home"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.GeneratedRouteFactory]
    public class NonPartialHost
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);

        // Should NOT generate a factory file when the class is not partial
        Assert.DoesNotContain(sources, s => s.HintName == "NonPartialHost.RouteFactory.g.cs");
    }

    // ---------------------------------------------------------------
    // Edge cases
    // ---------------------------------------------------------------

    [Fact]
    public void Factory_InGlobalNamespace()
    {
        var source = Preamble + @"
[SimpleRouter.Route]
public class GlobalRoute : SimpleRouter.IRoute
{
    public GlobalRoute(SimpleRouter.IRouterHost host) { RouterHost = host; }
    public string RouteName => ""Global"";
    public SimpleRouter.IRouterHost RouterHost { get; }
}

[SimpleRouter.GeneratedRouteFactory]
public partial class GlobalHost
{
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);
        var factorySource = Assert.Single(sources, s => s.HintName == "GlobalHost.RouteFactory.g.cs");

        Assert.Contains("GeneratedCreateRoutes", factorySource.Source);
        Assert.Contains("global::GlobalRoute", factorySource.Source);
        // Should NOT wrap in a namespace block
        Assert.DoesNotContain("namespace", factorySource.Source);
    }

    [Fact]
    public void Route_InDifferentNamespace()
    {
        var source = Preamble + @"
namespace Routes
{
    [SimpleRouter.Route]
    public class OtherRoute : SimpleRouter.IRoute
    {
        public OtherRoute(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""Other"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }
}

namespace Hosts
{
    [SimpleRouter.GeneratedRouteFactory]
    public partial class MyHost
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);
        var factorySource = Assert.Single(sources, s => s.HintName == "MyHost.RouteFactory.g.cs");

        // The factory is in Hosts namespace but should reference the route with fully qualified name
        Assert.Contains("namespace Hosts", factorySource.Source);
        Assert.Contains("global::Routes.OtherRoute", factorySource.Source);
    }

    [Fact]
    public void MultipleFactories_SameGroup()
    {
        var source = Preamble + @"
namespace TestApp
{
    [SimpleRouter.Route(Group = ""Shared"")]
    public class SharedRoute : SimpleRouter.IRoute
    {
        public SharedRoute(SimpleRouter.IRouterHost host) { RouterHost = host; }
        public string RouteName => ""Shared"";
        public SimpleRouter.IRouterHost RouterHost { get; }
    }

    [SimpleRouter.GeneratedRouteFactory(Group = ""Shared"")]
    public partial class HostOne
    {
    }

    [SimpleRouter.GeneratedRouteFactory(Group = ""Shared"")]
    public partial class HostTwo
    {
    }
}
";
        var sources = GeneratorTestHelper.GetGeneratedSources<RouteFactoryGenerator>(source);

        var factoryOne = Assert.Single(sources, s => s.HintName == "HostOne.RouteFactory.g.cs");
        Assert.Contains("global::TestApp.SharedRoute", factoryOne.Source);

        var factoryTwo = Assert.Single(sources, s => s.HintName == "HostTwo.RouteFactory.g.cs");
        Assert.Contains("global::TestApp.SharedRoute", factoryTwo.Source);
    }
}
