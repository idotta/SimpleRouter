using Microsoft.CodeAnalysis;

namespace SimpleRouter.SourceGenerators;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor RouteFactoryClassMustBePartial = new(
        id: "SR0001",
        title: "[GeneratedRouteFactory] class must be partial",
        messageFormat: "Class '{0}' is marked with [GeneratedRouteFactory] but is not declared as partial",
        category: "SimpleRouter",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ViewLocatorClassMustBePartial = new(
        id: "SR0002",
        title: "[GeneratedViewLocator] class must be partial",
        messageFormat: "Class '{0}' is marked with [GeneratedViewLocator] but is not declared as partial",
        category: "SimpleRouter",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ViewLocatorClassMustExtendViewLocatorBase = new(
        id: "SR0003",
        title: "[GeneratedViewLocator] class must extend ViewLocatorBase",
        messageFormat: "Class '{0}' is marked with [GeneratedViewLocator] but does not extend ViewLocatorBase",
        category: "SimpleRouter",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RouteClassMustImplementIRoute = new(
        id: "SR0004",
        title: "[Route] class must implement IRoute",
        messageFormat: "Class '{0}' is marked with [Route] but does not implement IRoute",
        category: "SimpleRouter",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RouteViewClassShouldExtendControl = new(
        id: "SR0005",
        title: "[RouteView] class should extend Control",
        messageFormat: "Class '{0}' is marked with [RouteView] but does not extend Control",
        category: "SimpleRouter",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
